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
        private Point _mouseDown;
        private Point _mouseUp;

        private Line _lineOverlay;
        private TextBlock _mouseDownText;
        private TextBlock _mouseFollowText;

        public MeasureTool(ImageCanvas canvas)
        {
            _lineOverlay = null;
            _mouseDownText = null;
            _mouseFollowText = null;

            _canvas = canvas;
            _mouseState = MouseState.WaitingForDown;
        }

        public void OnMouseDown(IInputElement relativeTo, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _mouseState == MouseState.WaitingForDown)
            {
                _mouseState = MouseState.WaitingForUp;
                _mouseDown = e.GetPosition(relativeTo);

                _EnsureOverlayElementsInitialized();

                _lineOverlay.X1 = _mouseDown.X;
                _lineOverlay.Y1 = _mouseDown.Y;

                SetCanvasPosition(_mouseDownText, _mouseDown);
                _mouseDownText.Text = _FormatPointForOverlayDisplay(_mouseDown);
            }
        }

        public void OnMouseUp(IInputElement relativeTo, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _mouseState == MouseState.WaitingForUp)
            {
                _mouseState = MouseState.WaitingForDown;
                _mouseUp = e.GetPosition(relativeTo);
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

        private void _EnsureOverlayElementsInitialized()
        {
            // Assume if one hasn't been initalized we need to init all of them
            if (_lineOverlay == null)
            {
                _lineOverlay = new Line();
                _lineOverlay.Stroke = System.Windows.Media.Brushes.Red;
                _lineOverlay.StrokeThickness = 2.0;

                _mouseDownText = _CreateOverlayTextBlock();
                _mouseFollowText = _CreateOverlayTextBlock();

                _canvas.SetOverlay(_OverlayElements);
            }
        }

        private TextBlock _CreateOverlayTextBlock()
        {
            TextBlock toReturn = new TextBlock();

            toReturn.Foreground = System.Windows.Media.Brushes.Red;

            return toReturn;
        }

        private void _UpdateOverlayForMouseMovement()
        {
            Debug.Assert(_lineOverlay != null);

            _lineOverlay.X2 = _lastMouseHoverPosition.X;
            _lineOverlay.Y2 = _lastMouseHoverPosition.Y;

            _mouseFollowText.Text = _FormatPointForOverlayDisplay(_lastMouseHoverPosition);

            Point leftPoint;
            Point rightPoint;

            if (_mouseDown.X < _lastMouseHoverPosition.X)
            {
                leftPoint = _mouseDown;
                rightPoint = _lastMouseHoverPosition;
            }
            else
            {
                leftPoint = _lastMouseHoverPosition;
                rightPoint = _mouseDown;
            }

            double rise = _lastMouseHoverPosition.Y - _mouseDown.Y;
            double run = _lastMouseHoverPosition.X - _mouseDown.X;

            double slope = rise / run;
            double angle = Double.NaN;

            if (!Double.IsNaN(slope))
            {
                angle = Math.Atan2(rise, run);
            }
            else if (rise > 0)
            {
                angle = Math.PI / 2;
            }
            else if (rise < 0)
            {
                angle = -Math.PI / 2;
            }

            int adjustmentDistance = 35;
            Point hoverAdjusted = _lastMouseHoverPosition;

            hoverAdjusted.X += adjustmentDistance * Math.Cos(angle);
            hoverAdjusted.Y += adjustmentDistance * Math.Sin(angle);

            // The adjusted point that we get here is the centered 
            // position, TextBlocks position from the top left
            hoverAdjusted.X -= (_mouseFollowText.ActualWidth / 2);
            hoverAdjusted.Y -= (_mouseFollowText.ActualHeight / 2);

            Point downAdjusted = _mouseDown;
            adjustmentDistance = -adjustmentDistance;

            downAdjusted.X += adjustmentDistance * Math.Cos(angle);
            downAdjusted.Y += adjustmentDistance * Math.Sin(angle);

            // The adjusted point that we get here is the centered 
            // position, TextBlocks position from the top left
            downAdjusted.X -= (_mouseFollowText.ActualWidth / 2);
            downAdjusted.Y -= (_mouseFollowText.ActualHeight / 2);

            SetCanvasPosition(_mouseFollowText, hoverAdjusted);
            SetCanvasPosition(_mouseDownText, downAdjusted);
        }

        private void SetCanvasPosition(UIElement element, Point pt)
        {
            element.SetValue(Canvas.LeftProperty, pt.X);
            element.SetValue(Canvas.TopProperty, pt.Y);
        }

        private string _FormatPointForOverlayDisplay(Point pt)
        {
            return string.Format("({0:F0},{1:F0})", pt.X, pt.Y);
        }

        private IEnumerable<UIElement> _OverlayElements
        {
            get
            {
                yield return _lineOverlay;
                yield return _mouseDownText;
                yield return _mouseFollowText;
            }
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
