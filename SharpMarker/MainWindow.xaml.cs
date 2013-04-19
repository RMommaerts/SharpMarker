using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SharpMarker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _lastPoint = new Point(0, 0);

        private ICanvasTool _activeTool;
        private Action _toolCompletedCallback;
        
        public MainWindow()
        {
            InitializeComponent();

            _activeTool = null;
            _toolCompletedCallback = null;

            imageCanvas.ImageMouseMove += imageCanvas_ImageMouseMove;
            imageCanvas.ImageMouseDown += imageCanvas_MouseDown;
            imageCanvas.ImageMouseUp += imageCanvas_MouseUp;
            imageCanvas.KeyUp += _OnImageKeyUp;
        }

        void imageCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (HasActiveCanvasTool)
            {
                _activeTool.OnMouseUp((IInputElement)sender, e);
            }
        }

        void imageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (HasActiveCanvasTool)
            {
                _activeTool.OnMouseDown((IInputElement)sender, e);
            }
        }

        private void imageCanvas_ImageMouseMove(object sender, MouseEventArgs e)
        {
            var senderAsInputElement = (IInputElement)sender;

            _lastPoint = e.GetPosition(senderAsInputElement);
            this.Title = string.Format("X: [{0:F2}] Y: [{1:F2}]", _lastPoint.X, _lastPoint.Y);

            if (HasActiveCanvasTool)
            {
                _activeTool.OnMouseMove(senderAsInputElement, e);
            }
        }

        private void _OnImageKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                _PasteFromClipboardToCanvas();
            }
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            _PasteFromClipboardToCanvas();
        }

        private void _PasteFromClipboardToCanvas()
        {
            if (Clipboard.ContainsImage())
            {
                BitmapSource bitmap = Clipboard.GetImage();
                imageCanvas.SetSource(bitmap);
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            btnSelect.IsEnabled = false;
            _SetActiveTool(new CropTool(imageCanvas), delegate { btnSelect.IsEnabled = true; });
        }

        private void _SetActiveTool(ICanvasTool tool, Action callback)
        {
            if (HasActiveCanvasTool)
            {
                OnActiveToolCompleted(this, EventArgs.Empty);
            }

            _activeTool = tool;
            _toolCompletedCallback = callback;
            _activeTool.Completed += OnActiveToolCompleted;
        }

        private void OnActiveToolCompleted(object sender, EventArgs e)
        {
            if (_toolCompletedCallback != null)
            {
                _toolCompletedCallback();
            }

            _activeTool.Completed -= OnActiveToolCompleted;
            _activeTool = null;

            imageCanvas.ClearOverlay();
        }

        public bool HasActiveCanvasTool
        {
            get { return _activeTool != null; }
        }

        private void btnMeasure_Click(object sender, RoutedEventArgs e)
        {
            btnMeasure.IsEnabled = false;
            _SetActiveTool(new MeasureTool(imageCanvas), delegate { btnMeasure.IsEnabled = true; });
        }
    }
}
