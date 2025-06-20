using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Texel.Classes.UndoSystem;

namespace Texel.Classes.Tools
{
    internal class LineTool : IDrawingTool
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
                var pixelAction = new PixelEditAction();
                DrawLine(_start.Value, _end.Value, canvas.SelectedColor, canvas, pixelAction);
                canvas.PushUndoAction(pixelAction);
                
                _dragging = false;
                _start = _end = null;
                canvas.Invalidate();
                canvas.ClearPreview();
            }
        }
        
        public void OnKeyDown(KeyEventArgs e, PixelCanvasControl canvas) { }
        
        private void DrawLine(Point start, Point end, Color color, PixelCanvasControl canvas, PixelEditAction action = null)
        {
            // Bresenham's line algorithm implementation
            int x0 = start.X;
            int y0 = start.Y;
            int x1 = end.X;
            int y1 = end.Y;
            
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                // Swap x0, y0
                int temp = x0;
                x0 = y0;
                y0 = temp;
                
                // Swap x1, y1
                temp = x1;
                x1 = y1;
                y1 = temp;
            }
            
            if (x0 > x1)
            {
                // Swap x0, x1
                int temp = x0;
                x0 = x1;
                x1 = temp;
                
                // Swap y0, y1
                temp = y0;
                y0 = y1;
                y1 = temp;
            }
            
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            
            for (int x = x0; x <= x1; x++)
            {
                int drawX = steep ? y : x;
                int drawY = steep ? x : y;
                
                if (drawX >= 0 && drawX < canvas.GridWidth && drawY >= 0 && drawY < canvas.GridHeight)
                {
                    if (action != null)
                    {
                        action.AddChange(drawX, drawY, canvas.Pixels[drawX, drawY], color);
                    }
                    canvas.Pixels[drawX, drawY] = color;
                }
                
                error -= dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }
    }
}
