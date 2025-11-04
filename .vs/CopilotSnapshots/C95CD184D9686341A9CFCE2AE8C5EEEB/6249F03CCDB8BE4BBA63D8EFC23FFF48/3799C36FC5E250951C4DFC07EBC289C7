using System;
using System.Drawing;
using System.Windows.Forms;
using Texel.Classes.Tools;

namespace Texel.Classes.Input
{
    internal class InputHandler
    {
        private readonly PixelCanvasControl _canvas;

        private bool _spaceDown;
        private bool _isPanning;
        private Point _panStart;

        public InputHandler(PixelCanvasControl canvas)
        {
            _canvas = canvas;
            AttachEvents();
        }

        private void AttachEvents()
        {
            _canvas.MouseDown += OnMouseDown;
            _canvas.MouseMove += OnMouseMove;
            _canvas.MouseUp += OnMouseUp;
            _canvas.KeyDown += OnKeyDown;
            _canvas.KeyUp += OnKeyUp;
            _canvas.MouseWheel += OnMouseWheel;
        }

        public void OnMouseDown(object sender, MouseEventArgs e)
        {
            _canvas.Focus();

            if (_spaceDown && e.Button == MouseButtons.Left)
            {
                _isPanning = true;
                _panStart = e.Location;
                _canvas.Cursor = Cursors.Hand;
                return;
            }

            _canvas.HandleMouseDown(e);
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                _canvas.PanOffset = new Point(
                    _canvas.PanOffset.X + e.X - _panStart.X,
                    _canvas.PanOffset.Y + e.Y - _panStart.Y
                );
                _panStart = e.Location;
                _canvas.Invalidate();
                return;
            }

            _canvas.HandleMouseMove(e);
        }

        public void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (_isPanning && e.Button == MouseButtons.Left)
            {
                _isPanning = false;
                _canvas.Cursor = Cursors.Default;
                return;
            }

            _canvas.HandleMouseUp(e);
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                _spaceDown = true;
                _canvas.Cursor = Cursors.Hand;
                return;
            }

            _canvas.HandleKeyDown(e);
        }



        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                _spaceDown = false;
                if (_isPanning)
                {
                    _isPanning = false;
                    _canvas.Cursor = Cursors.Default;
                }
                else
                {
                    _canvas.Cursor = Cursors.Default;
                }
                return;
            }

            _canvas.HandleKeyUp(e);
        }

        public void OnMouseWheel(object sender, MouseEventArgs e)
        {
            _canvas.HandleMouseWheel(e);
        }
    }
}
