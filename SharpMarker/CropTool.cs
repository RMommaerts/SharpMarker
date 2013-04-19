using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SharpMarker
{
    class CropTool : ICanvasTool
    {
        private ImageCanvas _canvas;
        private MouseState _selectionState;

        private Point _lastMouseHoverPosition;
        private Point _selectDown;
        private Point _selectUp;

        private Rectangle _rectOverlay;
        
        public CropTool(ImageCanvas canvas)
        {
            _rectOverlay = null;
            _canvas = canvas;
            _selectionState = MouseState.WaitingForDown;
        }

        public void OnMouseDown(IInputElement relativeTo, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _selectionState == MouseState.WaitingForDown)
            {
                _selectionState = MouseState.WaitingForUp;
                _selectDown = e.GetPosition(relativeTo);
            }
        }

        public void OnMouseUp(IInputElement relativeTo, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _selectionState == MouseState.WaitingForUp)
            {
                _selectionState = MouseState.Completed;
                _selectUp = e.GetPosition(relativeTo);

                _DragCompleted();
            }
        }

        private void _DragCompleted()
        {
            _canvas.Crop(new Rect(_selectDown, _selectUp));
            if (Completed != null)
            {
                Completed(this, EventArgs.Empty);
            }
        }

        public void OnMouseMove(IInputElement relativeTo, MouseEventArgs e)
        {
            if (_selectionState == MouseState.WaitingForUp)
            {
                _EnsureRectOverlayInitialized();
                _lastMouseHoverPosition = e.GetPosition(relativeTo);

                _UpdateOverlayForMouseMovement();
            }
        }

        private void _EnsureRectOverlayInitialized()
        {
            if (_rectOverlay == null)
            {
                _rectOverlay = new Rectangle();
                _rectOverlay.Stroke = System.Windows.Media.Brushes.Red;
                _rectOverlay.StrokeThickness = 2.0;

                _canvas.SetOverlay(new UIElement[] { _rectOverlay });
            }
        }

        private void _UpdateOverlayForMouseMovement()
        {
            Debug.Assert(_rectOverlay != null);

            Rect rect = new Rect(_selectDown, _lastMouseHoverPosition);

            _rectOverlay.Width = rect.Width + (2 * _rectOverlay.StrokeThickness);
            _rectOverlay.Height = rect.Height + (2 * _rectOverlay.StrokeThickness);
            _rectOverlay.SetValue(Canvas.LeftProperty, rect.Left - _rectOverlay.StrokeThickness);
            _rectOverlay.SetValue(Canvas.TopProperty, rect.Top - _rectOverlay.StrokeThickness);
        }


        public event EventHandler Completed;
        public event EventHandler OverlayUpdated;

        private enum MouseState
        {
            WaitingForDown,
            WaitingForUp,
            Completed
        }
    }
}
