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
    internal class FillTool : IDrawingTool
    {
        public void OnMouseDown(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            if (IsInside(gridPoint, canvas))
            {
                var targetColor = canvas.Pixels[gridPoint.X, gridPoint.Y];
                if (targetColor != canvas.SelectedColor)
                {
                    FloodFill(gridPoint.X, gridPoint.Y, targetColor, canvas.SelectedColor, canvas);
                    canvas.Invalidate();
                }
            }
        }

        public void OnMouseMove(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas) { }
        public void OnMouseUp(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas) { }
        public void OnKeyDown(KeyEventArgs e, PixelCanvasControl canvas) { }

        private bool IsInside(Point p, PixelCanvasControl canvas)
        {
            return p.X >= 0 && p.X < canvas.GridWidth &&
                   p.Y >= 0 && p.Y < canvas.GridHeight;
        }

        private void FloodFill(int x, int y, Color targetColor, Color replacementColor, PixelCanvasControl canvas)
        {
            if (targetColor == replacementColor || canvas.Pixels[x, y] != targetColor)
                return;

            var q = new System.Collections.Generic.Queue<Point>();
            q.Enqueue(new Point(x, y));

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (p.X < 0 || p.Y < 0 || p.X >= canvas.GridWidth || p.Y >= canvas.GridHeight)
                    continue;
                if (canvas.Pixels[p.X, p.Y] != targetColor)
                    continue;

                canvas.Pixels[p.X, p.Y] = replacementColor;

                q.Enqueue(new Point(p.X + 1, p.Y));
                q.Enqueue(new Point(p.X - 1, p.Y));
                q.Enqueue(new Point(p.X, p.Y + 1));
                q.Enqueue(new Point(p.X, p.Y - 1));
            }
        }
    }
}
