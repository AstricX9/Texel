using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Texel.Classes;

namespace Texel.Classes.Tools
{
    internal class EllipseTool : IDrawingTool
    {
        private Point? _start;
        private Point? _end;
        private bool _dragging;

        public void OnMouseDown(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            _start = gridPoint;
            _end = gridPoint;
            _dragging = true;
            canvas.SetPreview(_start, _end, _dragging);
        }

        public void OnMouseMove(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            if (_dragging && e.Button == MouseButtons.Left)
            {
                _end = gridPoint;
                canvas.SetPreview(_start, _end, _dragging);
            }
        }

        public void OnMouseUp(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            if (_dragging && _start.HasValue && _end.HasValue)
            {
                DrawEllipse(_start.Value, _end.Value, canvas.SelectedColor, canvas);
                _dragging = false;
                _start = _end = null;
                canvas.ClearPreview();
            }
        }

        public void OnKeyDown(KeyEventArgs e, PixelCanvasControl canvas) { }

        private void DrawEllipse(Point start, Point end, Color color, PixelCanvasControl canvas)
        {
            int x1 = System.Math.Min(start.X, end.X);
            int x2 = System.Math.Max(start.X, end.X);
            int y1 = System.Math.Min(start.Y, end.Y);
            int y2 = System.Math.Max(start.Y, end.Y);

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
                SetPixelEllipse(cx, cy, x, y, color, canvas);
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
                SetPixelEllipse(cx, cy, x, y, color, canvas);
                if (sigma >= 0)
                {
                    sigma += fb2 * (1 - x);
                    x--;
                }
                sigma += a2 * ((4 * y) + 6);
            }
        }

        private void SetPixelEllipse(int cx, int cy, int x, int y, Color color, PixelCanvasControl canvas)
        {
            int w = canvas.GridWidth;
            int h = canvas.GridHeight;
            if (cx + x >= 0 && cx + x < w && cy + y >= 0 && cy + y < h)
                canvas.Pixels[cx + x, cy + y] = color;
            if (cx - x >= 0 && cx - x < w && cy + y >= 0 && cy + y < h)
                canvas.Pixels[cx - x, cy + y] = color;
            if (cx + x >= 0 && cx + x < w && cy - y >= 0 && cy - y < h)
                canvas.Pixels[cx + x, cy - y] = color;
            if (cx - x >= 0 && cx - x < w && cy - y >= 0 && cy - y < h)
                canvas.Pixels[cx - x, cy - y] = color;
        }
    }
}
