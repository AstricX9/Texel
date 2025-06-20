using System;
using System.Collections.Generic;
using System.Drawing;
using Texel.Models;

namespace Texel.Classes.UndoSystem
{
    public class UndoManager
    {
        private readonly Stack<EditAction> _undoStack = new Stack<EditAction>();
        private readonly Stack<EditAction> _redoStack = new Stack<EditAction>();
        private readonly int _maxUndoSteps = 50;
        
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        
        public event EventHandler UndoRedoStateChanged;
        
        public void PushAction(EditAction action)
        {
            _undoStack.Push(action);
            _redoStack.Clear(); // Clear redo stack when a new action is performed
            
            // Limit undo stack size
            if (_undoStack.Count > _maxUndoSteps)
            {
                // Create a new stack with everything except the oldest item
                var tempStack = new Stack<EditAction>();
                for (int i = 0; i < _maxUndoSteps; i++)
                {
                    if (_undoStack.Count > 0)
                        tempStack.Push(_undoStack.Pop());
                }
                
                // Reverse back into the undo stack
                _undoStack.Clear();
                while (tempStack.Count > 0)
                    _undoStack.Push(tempStack.Pop());
            }
            
            OnUndoRedoStateChanged();
        }
        
        public void Undo(PixelCanvasControl canvas)
        {
            if (!CanUndo)
                return;
                
            var action = _undoStack.Pop();
            action.Undo(canvas);
            
            _redoStack.Push(action);
            OnUndoRedoStateChanged();
        }
        
        public void Redo(PixelCanvasControl canvas)
        {
            if (!CanRedo)
                return;
                
            var action = _redoStack.Pop();
            action.Redo(canvas);
            
            _undoStack.Push(action);
            OnUndoRedoStateChanged();
        }
        
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnUndoRedoStateChanged();
        }
        
        private void OnUndoRedoStateChanged()
        {
            UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public abstract class EditAction
    {
        public abstract void Undo(PixelCanvasControl canvas);
        public abstract void Redo(PixelCanvasControl canvas);
    }
    
    public class PixelEditAction : EditAction
    {
        private readonly List<PixelChange> _changes = new List<PixelChange>();
        
        public void AddChange(int x, int y, Color oldColor, Color newColor)
        {
            _changes.Add(new PixelChange(x, y, oldColor, newColor));
        }
        
        public override void Undo(PixelCanvasControl canvas)
        {
            foreach (var change in _changes)
            {
                canvas.Pixels[change.X, change.Y] = change.OldColor;
            }
            canvas.Invalidate();
        }
        
        public override void Redo(PixelCanvasControl canvas)
        {
            foreach (var change in _changes)
            {
                canvas.Pixels[change.X, change.Y] = change.NewColor;
            }
            canvas.Invalidate();
        }
        
        private struct PixelChange
        {
            public int X { get; }
            public int Y { get; }
            public Color OldColor { get; }
            public Color NewColor { get; }
            
            public PixelChange(int x, int y, Color oldColor, Color newColor)
            {
                X = x;
                Y = y;
                OldColor = oldColor;
                NewColor = newColor;
            }
        }
    }
    
    public class CanvasSizeAction : EditAction
    {
        private readonly int _oldWidth;
        private readonly int _oldHeight;
        private readonly int _newWidth;
        private readonly int _newHeight;
        private readonly Color[,] _oldPixels;
        
        public CanvasSizeAction(int oldWidth, int oldHeight, int newWidth, int newHeight, Color[,] oldPixels)
        {
            _oldWidth = oldWidth;
            _oldHeight = oldHeight;
            _newWidth = newWidth;
            _newHeight = newHeight;
            _oldPixels = oldPixels;
        }
        
        public override void Undo(PixelCanvasControl canvas)
        {
            var pixels = new Color[_oldWidth, _oldHeight];
            
            for (int x = 0; x < _oldWidth; x++)
            {
                for (int y = 0; y < _oldHeight; y++)
                {
                    pixels[x, y] = _oldPixels[x, y];
                }
            }
            
            canvas.SetGridSize(_oldWidth, _oldHeight, pixels);
        }
        
        public override void Redo(PixelCanvasControl canvas)
        {
            canvas.SetGridSize(_newWidth, _newHeight);
        }
    }
}
