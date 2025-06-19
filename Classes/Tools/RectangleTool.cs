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
    internal class RectangleTool : IDrawingTool
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
                canvas.Invalidate();
                canvas.SetPreview(_start, _end, _dragging);
            }
        }

        public void OnMouseUp(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            if (_dragging && _start.HasValue && _end.HasValue)
            {
                DrawRectangle(_start.Value, _end.Value, canvas.SelectedColor, canvas);
                _dragging = false;
                _start = _end = null;
                canvas.Invalidate();
                canvas.ClearPreview();
            }
        }

        public void OnKeyDown(KeyEventArgs e, PixelCanvasControl canvas) { }

        private void DrawRectangle(Point start, Point end, Color color, PixelCanvasControl canvas)
        {
            int x1 = System.Math.Max(0, System.Math.Min(canvas.GridWidth - 1, System.Math.Min(start.X, end.X)));
            int x2 = System.Math.Max(0, System.Math.Min(canvas.GridWidth - 1, System.Math.Max(start.X, end.X)));
            int y1 = System.Math.Max(0, System.Math.Min(canvas.GridHeight - 1, System.Math.Min(start.Y, end.Y)));
            int y2 = System.Math.Max(0, System.Math.Min(canvas.GridHeight - 1, System.Math.Max(start.Y, end.Y)));

            for (int x = x1; x <= x2; x++)
            {
                if (y1 >= 0 && y1 < canvas.GridHeight) canvas.Pixels[x, y1] = color;
                if (y2 >= 0 && y2 < canvas.GridHeight) canvas.Pixels[x, y2] = color;
            }
            for (int y = y1; y <= y2; y++)
            {
                if (x1 >= 0 && x1 < canvas.GridWidth) canvas.Pixels[x1, y] = color;
                if (x2 >= 0 && x2 < canvas.GridWidth) canvas.Pixels[x2, y] = color;
            }
        }
    }
}
