using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for ImageCanvas.xaml
    /// </summary>
    public partial class ImageCanvas : UserControl
    {
        private BitmapSource _bitmap;

        private byte[] _pixels;
        private int _stride;
        private BitmapPalette _palette;
        private PixelFormat _pixelFormat;
        private int _widthPixels;
        private int _heightPixels;

        private double _dpiX;
        private double _dpiY;
        
        public ImageCanvas()
        {
            InitializeComponent();
        }

        public void SetSource(BitmapSource source)
        {
            _widthPixels = source.PixelWidth;
            _heightPixels = source.PixelHeight;
            _palette = source.Palette;
            _pixelFormat = source.Format;
            _dpiX = source.DpiX;
            _dpiY = source.DpiY;
            
            _stride = GetStride(_widthPixels, _pixelFormat.BitsPerPixel);

            _pixels = new byte[_stride * _heightPixels];
            source.CopyPixels(_pixels, _stride, offset: 0);

            // Just set all the alpha values to 255 for now since for some reason
            // the PrintScreen screen cap data contains 0 for all the alpha values
            for (int i = 3; i < _pixels.Length; i += 4)
            {
                _pixels[i] = byte.MaxValue;
            }

            ReconstructBitmap();
            SetSourceToInteralBitmap();
        }

        private void SetSourceToInteralBitmap()
        {
            Debug.Assert(_bitmap != null);
            
            // Fix the width and height of the image to that of the bitmap so that we
            // correctly align to device pixels. If the bitmap and image are different
            // sizes WPF will try to be smart and make it fit which can cause blurring.
            mainImage.Width = _bitmap.PixelWidth;
            mainImage.Height = _bitmap.PixelHeight;
            mainImage.Source = _bitmap;

            canvas.Width = _bitmap.PixelWidth;
            canvas.Height = _bitmap.PixelHeight;
        }

        public void Crop(Rect rect)
        {
            var intRect = new Int32Rect((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);

            _widthPixels = intRect.Width;
            _heightPixels = intRect.Height;

            _stride = GetStride(_widthPixels, _pixelFormat.BitsPerPixel);
            _pixels = new byte[_stride * _heightPixels];
            _bitmap.CopyPixels(intRect, _pixels, _stride, offset: 0);
            
            ReconstructBitmap();
            SetSourceToInteralBitmap();
        }

        private void ReconstructBitmap()
        {
            _bitmap = BitmapSource.Create(_widthPixels, _heightPixels, _dpiX, _dpiY, _pixelFormat, _palette, _pixels, _stride);
        }

        private static int GetStride(int bitmapWidthPixels, int bitmapBitsPerPixel)
        {
            return bitmapWidthPixels * ((bitmapBitsPerPixel + 7) / 8);
        }

        private void OnImageMouseMove(object sender, MouseEventArgs e)
        {
            if (ImageMouseMove != null)
            {
                ImageMouseMove(sender, e);
            }
        }

        private void OnImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ImageMouseDown != null)
            {
                ImageMouseDown(sender, e);
            }
        }

        private void OnImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ImageMouseUp != null)
            {
                ImageMouseUp(sender, e);
            }
        }

        public event EventHandler<MouseEventArgs> ImageMouseMove;
        public event EventHandler<MouseButtonEventArgs> ImageMouseDown;
        public event EventHandler<MouseButtonEventArgs> ImageMouseUp;
    }
}
