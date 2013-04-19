using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharpMarker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point _lastPoint = new Point(0, 0);

        Point _selectDown;
        Point _selectUp;
        SelectionState _selectionState;
        
        public MainWindow()
        {
            InitializeComponent();
            _selectionState = SelectionState.NotStarted;

            imageCanvas.ImageMouseMove += imageCanvas_ImageMouseMove;
            imageCanvas.ImageMouseDown += imageCanvas_MouseDown;
            imageCanvas.ImageMouseUp += imageCanvas_MouseUp;
        }

        private void SelectionCompleted()
        {
            imageCanvas.Crop(new Rect(_selectDown, _selectUp));
        }

        void imageCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _selectionState == SelectionState.WaitingForUp)
            {
                _selectionState = SelectionState.NotStarted;
                _selectUp = e.GetPosition((IInputElement)sender);

                SelectionCompleted();
            }
        }

        void imageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _selectionState == SelectionState.WaitingForDown)
            {
                _selectionState = SelectionState.WaitingForUp;
                _selectDown = e.GetPosition((IInputElement)sender);
            }
        }

        private void imageCanvas_ImageMouseMove(object sender, MouseEventArgs e)
        {
            _lastPoint = e.GetPosition((IInputElement)sender);
            this.Title = string.Format("X: [{0:F2}] Y: [{1:F2}]", _lastPoint.X, _lastPoint.Y);
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                BitmapSource bitmap = Clipboard.GetImage();
                imageCanvas.SetSource(bitmap);
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            _selectionState = SelectionState.WaitingForDown;
        }

        private enum SelectionState
        {
            NotStarted,
            WaitingForDown,
            WaitingForUp
        }
    }
}
