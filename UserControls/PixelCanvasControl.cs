using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Texel
{
    public enum ToolMode { Pen, Fill, Select, Rectangle, Ellipse, Eraser }

    public partial class PixelCanvasControl : UserControl
    {
        public int CellSize = 16;
        public Color[,] Pixels { get; private set; } = new Color[64, 64]; // or 128,128
        public ToolMode CurrentTool { get; set; } = ToolMode.Pen;
        public Color SelectedColor { get; set; } = Color.Black;

        public int GridWidth => Pixels.GetLength(0);
        public int GridHeight => Pixels.GetLength(1);

        private Point? selectionStart = null;
        private Point? selectionEnd = null;
        private bool isDragging = false;
        private bool hasSelection = false;

        private Point panOffset = Point.Empty;
        private Point panStart = Point.Empty;
        private bool isPanning = false;
        private bool spaceDown = false;

        public PixelCanvasControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            this.MouseDown += PixelCanvas_MouseDown;
            this.MouseMove += PixelCanvas_MouseMove;
            this.MouseUp += PixelCanvas_MouseUp;
            this.KeyDown += PixelCanvas_KeyDown;
            this.KeyUp += PixelCanvas_KeyUp;
            this.MouseWheel += PixelCanvas_MouseWheel;
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TranslateTransform(panOffset.X, panOffset.Y);

            // Draw the pixel grid
            for (int x = 0; x < Pixels.GetLength(0); x++)
            {
                for (int y = 0; y < Pixels.GetLength(1); y++)
                {
                    Rectangle rect = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);

                    // Checkerboard background
                    bool isAlt = (x + y) % 2 == 0;
                    g.FillRectangle(new SolidBrush(isAlt ? Color.White : Color.LightGray), rect);

                    // Pixel color
                    if (Pixels[x, y].A > 0)
                        g.FillRectangle(new SolidBrush(Pixels[x, y]), rect);

                    // Selection overlay
                    if (hasSelection && IsInSelection(x, y))
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(80, Color.Cyan)))
                            g.FillRectangle(brush, rect);
                    }

                    // Grid lines
                    g.DrawRectangle(Pens.DarkGray, rect);
                }
            }

            // Draw pixel-perfect preview for shapes while dragging
            if ((CurrentTool == ToolMode.Rectangle || CurrentTool == ToolMode.Ellipse)
                && selectionStart.HasValue && selectionEnd.HasValue && isDragging)
            {
                var (start, end) = GetAdjustedPoints(selectionStart.Value, selectionEnd.Value);
                if (CurrentTool == ToolMode.Rectangle)
                {
                    DrawRectanglePreview(g, start, end);
                }
                else if (CurrentTool == ToolMode.Ellipse)
                {
                    DrawEllipsePreview(g, start, end);
                }
            }

            // Draw selection rectangle border
            if ((CurrentTool == ToolMode.Select || hasSelection)
                && selectionStart.HasValue && selectionEnd.HasValue)
            {
                var (start, end) = GetAdjustedPoints(selectionStart.Value, selectionEnd.Value);
                int x1 = Math.Min(start.X, end.X) * CellSize;
                int y1 = Math.Min(start.Y, end.Y) * CellSize;
                int x2 = (Math.Max(start.X, end.X) + 1) * CellSize - 1;
                int y2 = (Math.Max(start.Y, end.Y) + 1) * CellSize - 1;
                Rectangle selRect = Rectangle.FromLTRB(x1, y1, x2, y2);

                using (var pen = new Pen(Color.Blue) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    g.DrawRectangle(pen, selRect);
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            try
            {
                // Allow zooming with Ctrl or Alt (or both)
                if ((ModifierKeys & Keys.Control) == Keys.Control || (ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    int oldCellSize = CellSize;
                    int minCellSize = 4, maxCellSize = 64;
                    int newCellSize = CellSize;

                    newCellSize = e.Delta > 0
                        ? Math.Min(maxCellSize, CellSize + 2)
                        : Math.Max(minCellSize, CellSize - 2);

                    if (newCellSize != CellSize)
                    {
                        var mouse = e.Location;
                        float logicalX = (mouse.X - panOffset.X) / (float)oldCellSize;
                        float logicalY = (mouse.Y - panOffset.Y) / (float)oldCellSize;

                        CellSize = newCellSize;

                        panOffset.X = mouse.X - (int)(logicalX * CellSize);
                        panOffset.Y = mouse.Y - (int)(logicalY * CellSize);

                        Invalidate();
                    }
                }
                else
                {
                    base.OnMouseWheel(e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Zoom Error: " + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        // Helper: Clamp a point to the grid
        private Point ClampToGrid(Point p)
        {
            int x = Math.Max(0, Math.Min(Pixels.GetLength(0) - 1, p.X));
            int y = Math.Max(0, Math.Min(Pixels.GetLength(1) - 1, p.Y));
            return new Point(x, y);
        }

        // Helper: Clamp a value to the grid
        private int ClampX(int x) => Math.Max(0, Math.Min(Pixels.GetLength(0) - 1, x));
        private int ClampY(int y) => Math.Max(0, Math.Min(Pixels.GetLength(1) - 1, y));

        // Helper: Get the clamped selection rectangle in grid coordinates
        private Rectangle GetClampedSelectionRect()
        {
            if (!selectionStart.HasValue || !selectionEnd.HasValue)
                return Rectangle.Empty;

            var (start, end) = GetAdjustedPoints(selectionStart.Value, selectionEnd.Value);
            int x1 = ClampX(Math.Min(start.X, end.X));
            int y1 = ClampY(Math.Min(start.Y, end.Y));
            int x2 = ClampX(Math.Max(start.X, end.X));
            int y2 = ClampY(Math.Max(start.Y, end.Y));
            return Rectangle.FromLTRB(x1, y1, x2 + 1, y2 + 1); // +1 because Rectangle is exclusive at the end
        }

        private void PixelCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();

            if (spaceDown && e.Button == MouseButtons.Left)
            {
                isPanning = true;
                panStart = e.Location;
                Cursor = Cursors.Hand;
                return;
            }

            int px = (e.X - panOffset.X) / CellSize;
            int py = (e.Y - panOffset.Y) / CellSize;
            Point mousePoint = new Point(px, py);

            if (CurrentTool == ToolMode.Select)
            {
                selectionStart = mousePoint;
                selectionEnd = mousePoint;
                isDragging = true;
                hasSelection = false;
                Invalidate();
            }
            else
            {
                // Clear selection when switching to a drawing tool
                if (hasSelection)
                {
                    hasSelection = false;
                    selectionStart = selectionEnd = null;
                    Invalidate();
                }

                // Only draw if inside grid
                if (px < 0 || py < 0 || px >= Pixels.GetLength(0) || py >= Pixels.GetLength(1))
                    return;

                if (CurrentTool == ToolMode.Pen)
                {
                    Pixels[px, py] = SelectedColor;
                    Invalidate();
                }
                else if (CurrentTool == ToolMode.Fill)
                {
                    FloodFill(px, py, Pixels[px, py], SelectedColor);
                    Invalidate();
                }
                else if (CurrentTool == ToolMode.Eraser)
                {
                    Pixels[px, py] = Color.Transparent;
                    Invalidate();
                }
                else if (CurrentTool == ToolMode.Rectangle || CurrentTool == ToolMode.Ellipse)
                {
                    selectionStart = mousePoint;
                    selectionEnd = selectionStart;
                    isDragging = true;
                    hasSelection = false;
                    Invalidate();
                }
            }
        }

        private void PixelCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                panOffset.X += e.X - panStart.X;
                panOffset.Y += e.Y - panStart.Y;
                panStart = e.Location;
                Invalidate();
                return;
            }

            int px = (e.X - panOffset.X) / CellSize;
            int py = (e.Y - panOffset.Y) / CellSize;
            Point mousePoint = new Point(px, py);

            if (e.Button == MouseButtons.Left)
            {
                if (CurrentTool == ToolMode.Pen)
                {
                    if (px >= 0 && py >= 0 && px < Pixels.GetLength(0) && py < Pixels.GetLength(1))
                    {
                        Pixels[px, py] = SelectedColor;
                        Invalidate();
                    }
                }
                else if (CurrentTool == ToolMode.Eraser)
                {
                    if (px >= 0 && py >= 0 && px < Pixels.GetLength(0) && py < Pixels.GetLength(1))
                    {
                        Pixels[px, py] = Color.Transparent;
                        Invalidate();
                    }
                }
                else if (isDragging && selectionStart.HasValue)
                {
                    selectionEnd = mousePoint;
                    Invalidate();
                }
            }
        }

        private void PixelCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            OnMouseWheel(e);
        }

        private void PixelCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (isPanning && e.Button == MouseButtons.Left)
            {
                isPanning = false;
                Cursor = Cursors.Default;
                return;
            }

            int px = (e.X - panOffset.X) / CellSize;
            int py = (e.Y - panOffset.Y) / CellSize;

            if (CurrentTool == ToolMode.Select && selectionStart.HasValue && selectionEnd.HasValue)
            {
                hasSelection = true;
                isDragging = false;
                Invalidate();
            }
            else if (isDragging && selectionStart.HasValue && selectionEnd.HasValue)
            {
                var (start, end) = GetAdjustedPoints(selectionStart.Value, selectionEnd.Value);

                if (CurrentTool == ToolMode.Rectangle)
                {
                    DrawRectangle(start, end, SelectedColor);
                }
                else if (CurrentTool == ToolMode.Ellipse)
                {
                    DrawEllipse(start, end, SelectedColor);
                }
                selectionStart = selectionEnd = null;
                isDragging = false;
                Invalidate();
            }
        }

        private void PixelCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                spaceDown = true;
                Cursor = Cursors.Hand;
                return;
            }

            if (e.KeyCode == Keys.Delete && hasSelection && selectionStart.HasValue && selectionEnd.HasValue)
            {
                Rectangle sel = GetClampedSelectionRect();
                for (int x = sel.Left; x < sel.Right; x++)
                {
                    for (int y = sel.Top; y < sel.Bottom; y++)
                    {
                        Pixels[x, y] = Color.Transparent;
                    }
                }
                hasSelection = false;
                selectionStart = selectionEnd = null;
                Invalidate();
            }
        }

        private void PixelCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                spaceDown = false;
                if (isPanning)
                {
                    isPanning = false;
                    Cursor = Cursors.Default;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// Returns true if the given pixel is inside the current selection.
        /// </summary>
        private bool IsInSelection(int x, int y)
        {
            if (!selectionStart.HasValue || !selectionEnd.HasValue || !hasSelection)
                return false;
            Rectangle sel = GetClampedSelectionRect();
            return sel.Contains(x, y);
        }

        /// <summary>
        /// Adjusts the end point for aspect ratio (Shift) and centering (Shift+Alt).
        /// </summary>
        private (Point, Point) GetAdjustedPoints(Point start, Point end)
        {
            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
            bool alt = (ModifierKeys & Keys.Alt) == Keys.Alt;

            int dx = end.X - start.X;
            int dy = end.Y - start.Y;

            if (shift)
            {
                int size = Math.Max(Math.Abs(dx), Math.Abs(dy));
                int signX = dx >= 0 ? 1 : -1;
                int signY = dy >= 0 ? 1 : -1;

                if (alt)
                {
                    // Centered from start
                    Point newStart = new Point(start.X - size * signX, start.Y - size * signY);
                    Point newEnd = new Point(start.X + size * signX, start.Y + size * signY);
                    // Clamp to grid
                    newStart.X = Math.Max(0, Math.Min(Pixels.GetLength(0) - 1, newStart.X));
                    newStart.Y = Math.Max(0, Math.Min(Pixels.GetLength(1) - 1, newStart.Y));
                    newEnd.X = Math.Max(0, Math.Min(Pixels.GetLength(0) - 1, newEnd.X));
                    newEnd.Y = Math.Max(0, Math.Min(Pixels.GetLength(1) - 1, newEnd.Y));
                    return (newStart, newEnd);
                }
                else
                {
                    // Square from start to end
                    Point newEnd = new Point(start.X + size * signX, start.Y + size * signY);
                    // Clamp to grid
                    newEnd.X = Math.Max(0, Math.Min(Pixels.GetLength(0) - 1, newEnd.X));
                    newEnd.Y = Math.Max(0, Math.Min(Pixels.GetLength(1) - 1, newEnd.Y));
                    return (start, newEnd);
                }
            }
            else
            {
                // No adjustment
                return (start, end);
            }
        }

        private void FloodFill(int x, int y, Color targetColor, Color replacementColor)
        {
            if (targetColor == replacementColor || Pixels[x, y] != targetColor)
                return;

            Queue<Point> q = new Queue<Point>();
            q.Enqueue(new Point(x, y));

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (p.X < 0 || p.Y < 0 || p.X >= Pixels.GetLength(0) || p.Y >= Pixels.GetLength(1))
                    continue;
                if (Pixels[p.X, p.Y] != targetColor)
                    continue;

                Pixels[p.X, p.Y] = replacementColor;

                q.Enqueue(new Point(p.X + 1, p.Y));
                q.Enqueue(new Point(p.X - 1, p.Y));
                q.Enqueue(new Point(p.X, p.Y + 1));
                q.Enqueue(new Point(p.X, p.Y - 1));
            }
        }

        private void DrawRectangle(Point start, Point end, Color color)
        {
            int x1 = Math.Max(0, Math.Min(Pixels.GetLength(0) - 1, Math.Min(start.X, end.X)));
            int x2 = Math.Max(0, Math.Min(Pixels.GetLength(0) - 1, Math.Max(start.X, end.X)));
            int y1 = Math.Max(0, Math.Min(Pixels.GetLength(1) - 1, Math.Min(start.Y, end.Y)));
            int y2 = Math.Max(0, Math.Min(Pixels.GetLength(1) - 1, Math.Max(start.Y, end.Y)));

            for (int x = x1; x <= x2; x++)
            {
                if (y1 >= 0 && y1 < Pixels.GetLength(1)) Pixels[x, y1] = color;
                if (y2 >= 0 && y2 < Pixels.GetLength(1)) Pixels[x, y2] = color;
            }
            for (int y = y1; y <= y2; y++)
            {
                if (x1 >= 0 && x1 < Pixels.GetLength(0)) Pixels[x1, y] = color;
                if (x2 >= 0 && x2 < Pixels.GetLength(0)) Pixels[x2, y] = color;
            }
        }

        private void DrawEllipse(Point start, Point end, Color color)
        {
            int x1 = Math.Min(start.X, end.X);
            int x2 = Math.Max(start.X, end.X);
            int y1 = Math.Min(start.Y, end.Y);
            int y2 = Math.Max(start.Y, end.Y);

            int a = (x2 - x1) / 2;
            int b = (y2 - y1) / 2;
            int cx = x1 + a;
            int cy = y1 + b;

            if (a == 0 || b == 0) return;

            int a2 = a * a;
            int b2 = b * b;
            int fa2 = 4 * a2, fb2 = 4 * b2;
            int x, y, sigma;

            // Region 1
            for (x = 0, y = b, sigma = 2 * b2 + a2 * (1 - 2 * b); b2 * x <= a2 * y; x++)
            {
                SetPixelEllipse(cx, cy, x, y, color);
                if (sigma >= 0)
                {
                    sigma += fa2 * (1 - y);
                    y--;
                }
                sigma += b2 * ((4 * x) + 6);
            }
            // Region 2
            for (x = a, y = 0, sigma = 2 * a2 + b2 * (1 - 2 * a); a2 * y <= b2 * x; y++)
            {
                SetPixelEllipse(cx, cy, x, y, color);
                if (sigma >= 0)
                {
                    sigma += fb2 * (1 - x);
                    x--;
                }
                sigma += a2 * ((4 * y) + 6);
            }
        }

        private void SetPixelEllipse(int cx, int cy, int x, int y, Color color)
        {
            int w = Pixels.GetLength(0);
            int h = Pixels.GetLength(1);
            if (cx + x >= 0 && cx + x < w && cy + y >= 0 && cy + y < h)
                Pixels[cx + x, cy + y] = color;
            if (cx - x >= 0 && cx - x < w && cy + y >= 0 && cy + y < h)
                Pixels[cx - x, cy + y] = color;
            if (cx + x >= 0 && cx + x < w && cy - y >= 0 && cy - y < h)
                Pixels[cx + x, cy - y] = color;
            if (cx - x >= 0 && cx - x < w && cy - y >= 0 && cy - y < h)
                Pixels[cx - x, cy - y] = color;
        }

        // Pixel-perfect preview for rectangle
        private void DrawRectanglePreview(Graphics g, Point start, Point end)
        {
            int x1 = Math.Min(start.X, end.X);
            int x2 = Math.Max(start.X, end.X);
            int y1 = Math.Min(start.Y, end.Y);
            int y2 = Math.Max(start.Y, end.Y);

            using (var brush = new SolidBrush(Color.FromArgb(120, SelectedColor)))
            {
                for (int x = x1; x <= x2; x++)
                {
                    if (y1 >= 0 && y1 < Pixels.GetLength(1))
                        g.FillRectangle(brush, x * CellSize, y1 * CellSize, CellSize, CellSize);
                    if (y2 >= 0 && y2 < Pixels.GetLength(1))
                        g.FillRectangle(brush, x * CellSize, y2 * CellSize, CellSize, CellSize);
                }
                for (int y = y1; y <= y2; y++)
                {
                    if (x1 >= 0 && x1 < Pixels.GetLength(0))
                        g.FillRectangle(brush, x1 * CellSize, y * CellSize, CellSize, CellSize);
                    if (x2 >= 0 && x2 < Pixels.GetLength(0))
                        g.FillRectangle(brush, x2 * CellSize, y * CellSize, CellSize, CellSize);
                }
            }
        }

        // Pixel-perfect preview for ellipse
        private void DrawEllipsePreview(Graphics g, Point start, Point end)
        {
            int x1 = Math.Min(start.X, end.X);
            int x2 = Math.Max(start.X, end.X);
            int y1 = Math.Min(start.Y, end.Y);
            int y2 = Math.Max(start.Y, end.Y);

            int a = (x2 - x1) / 2;
            int b = (y2 - y1) / 2;
            int cx = x1 + a;
            int cy = y1 + b;

            if (a == 0 || b == 0) return;

            int a2 = a * a;
            int b2 = b * b;
            int fa2 = 4 * a2, fb2 = 4 * b2;
            int x, y, sigma;

            using (var brush = new SolidBrush(Color.FromArgb(120, SelectedColor)))
            {
                // Region 1
                for (x = 0, y = b, sigma = 2 * b2 + a2 * (1 - 2 * b); b2 * x <= a2 * y; x++)
                {
                    FillEllipsePreviewPixel(g, cx, cy, x, y, brush);
                    if (sigma >= 0)
                    {
                        sigma += fa2 * (1 - y);
                        y--;
                    }
                    sigma += b2 * ((4 * x) + 6);
                }
                // Region 2
                for (x = a, y = 0, sigma = 2 * a2 + b2 * (1 - 2 * a); a2 * y <= b2 * x; y++)
                {
                    FillEllipsePreviewPixel(g, cx, cy, x, y, brush);
                    if (sigma >= 0)
                    {
                        sigma += fb2 * (1 - x);
                        x--;
                    }
                    sigma += a2 * ((4 * y) + 6);
                }
            }
        }

        private void FillEllipsePreviewPixel(Graphics g, int cx, int cy, int x, int y, Brush brush)
        {
            int w = Pixels.GetLength(0);
            int h = Pixels.GetLength(1);
            if (cx + x >= 0 && cx + x < w && cy + y >= 0 && cy + y < h)
                g.FillRectangle(brush, (cx + x) * CellSize, (cy + y) * CellSize, CellSize, CellSize);
            if (cx - x >= 0 && cx - x < w && cy + y >= 0 && cy + y < h)
                g.FillRectangle(brush, (cx - x) * CellSize, (cy + y) * CellSize, CellSize, CellSize);
            if (cx + x >= 0 && cx + x < w && cy - y >= 0 && cy - y < h)
                g.FillRectangle(brush, (cx + x) * CellSize, (cy - y) * CellSize, CellSize, CellSize);
            if (cx - x >= 0 && cx - x < w && cy - y >= 0 && cy - y < h)
                g.FillRectangle(brush, (cx - x) * CellSize, (cy - y) * CellSize, CellSize, CellSize);
        }

        public void SetGridSize(int width, int height)
        {
            var newPixels = new Color[width, height];
            int minW = Math.Min(width, Pixels.GetLength(0));
            int minH = Math.Min(height, Pixels.GetLength(1));
            for (int x = 0; x < minW; x++)
                for (int y = 0; y < minH; y++)
                    newPixels[x, y] = Pixels[x, y];
            Pixels = newPixels;
            Invalidate();
        }
    }
}
