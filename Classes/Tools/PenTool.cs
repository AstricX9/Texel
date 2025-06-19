using System.Drawing;
using System.Windows.Forms;
using Texel.Classes;

namespace Texel.Classes.Tools
{
    internal class PenTool : IDrawingTool
    {
        public void OnMouseDown(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            if (IsInside(gridPoint, canvas))
            {
                canvas.Pixels[gridPoint.X, gridPoint.Y] = canvas.SelectedColor;
                canvas.Invalidate();
            }
        }

        public void OnMouseMove(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            if (e.Button == MouseButtons.Left && IsInside(gridPoint, canvas))
            {
                canvas.Pixels[gridPoint.X, gridPoint.Y] = canvas.SelectedColor;
                canvas.Invalidate();
            }
        }

        public void OnMouseUp(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas) { }

        public void OnKeyDown(KeyEventArgs e, PixelCanvasControl canvas) { }

        public bool IsInside(Point p, PixelCanvasControl canvas)
        {
            return p.X >= 0 && p.X < canvas.GridWidth &&
                   p.Y >= 0 && p.Y < canvas.GridHeight;
        }
    }
}
