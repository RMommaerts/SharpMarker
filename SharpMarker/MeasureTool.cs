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
    class MeasureTool : ICanvasTool
    {
        private ImageCanvas _canvas;
        private MouseState _mouseState;

        private Point _lastMouseHoverPosition;
        private Point _selectDown;
        private Point _selectUp;

        private Line _lineOverlay;

        public MeasureTool(ImageCanvas canvas)
        {
            _lineOverlay = null;
            _canvas = canvas;
            _mouseState = MouseState.WaitingForDown;
        }

        public void OnMouseDown(IInputElement relativeTo, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _mouseState == MouseState.WaitingForDown)
            {
                _mouseState = MouseState.WaitingForUp;
                _selectDown = e.GetPosition(relativeTo);

                _EnsureLineOverlayInitialized();
            }
        }

        public void OnMouseUp(IInputElement relativeTo, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _mouseState == MouseState.WaitingForUp)
            {
                _mouseState = MouseState.WaitingForDown;
                _selectUp = e.GetPosition(relativeTo);
            }
        }

        public void OnMouseMove(IInputElement relativeTo, MouseEventArgs e)
        {
            if (_mouseState == MouseState.WaitingForUp)
            {
                Debug.Assert(_lineOverlay != null);
                _lastMouseHoverPosition = e.GetPosition(relativeTo);

                _UpdateOverlayForMouseMovement();
            }
        }

        private void _EnsureLineOverlayInitialized()
        {
            if (_lineOverlay == null)
            {
                _lineOverlay = new Line();
                _lineOverlay.Stroke = System.Windows.Media.Brushes.Red;
                _lineOverlay.StrokeThickness = 2.0;

                _canvas.SetOverlay(new UIElement[] { _lineOverlay });
            }
        }

        private void _UpdateOverlayForMouseMovement()
        {
            Debug.Assert(_lineOverlay != null);

            _lineOverlay.X1 = _selectDown.X;
            _lineOverlay.Y1 = _selectDown.Y;
            _lineOverlay.X2 = _lastMouseHoverPosition.X;
            _lineOverlay.Y2 = _lastMouseHoverPosition.Y;
        }

        public event EventHandler Completed;
        public event EventHandler OverlayUpdated;

        private enum MouseState
        {
            WaitingForDown,
            WaitingForUp,
        }
    }
}
