using System;
using System.Drawing;
using System.Windows.Forms;
using Texel.Classes;

namespace Texel.Classes.Tools
{
    internal class EyedropperTool : IDrawingTool
    {
        private Cursor _eyedropperCursor;
        
        public EyedropperTool()
        {
            // Create a custom eyedropper cursor or use a built-in one
            _eyedropperCursor = Cursors.Cross; // Fallback to Cross if custom cursor not available
        }
        
        public void OnMouseDown(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            if (IsInside(gridPoint, canvas))
            {
                Color sampledColor = canvas.Pixels[gridPoint.X, gridPoint.Y];
                if (sampledColor.A > 0) // Only sample if it's not transparent
                {
                    canvas.SelectedColor = sampledColor;
                    canvas.OnColorSampled(sampledColor);
                }
            }
        }
        
        public void OnMouseMove(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            // No action needed for mouse move
        }
        
        public void OnMouseUp(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            // No action needed for mouse up
        }
        
        public void OnKeyDown(KeyEventArgs e, PixelCanvasControl canvas) { }
        
        private bool IsInside(Point p, PixelCanvasControl canvas)
        {
            return p.X >= 0 && p.X < canvas.GridWidth &&
                   p.Y >= 0 && p.Y < canvas.GridHeight;
        }
    }
}
