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
    internal class SelectTool : IDrawingTool
    {
        private Point? _start;
        private Point? _end;
        private bool _dragging;

        public void OnMouseDown(Point gridPoint, MouseEventArgs e, PixelCanvasControl canvas)
        {
            _start = gridPoint;
            _end = gridPoint;
            _dragging = true;
            canvas.Invalidate();
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
                canvas.ApplySelection(_start, _end);
                _dragging = false;
                _start = _end = null;
            }
        }

        public void OnKeyDown(KeyEventArgs e, PixelCanvasControl canvas) { }
    }
}
