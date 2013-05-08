using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        private Size _scaleFactor;

        private double _dpiX;
        private double _dpiY;
        
        public ImageCanvas()
        {
            InitializeComponent();
            _scaleFactor = new Size(1, 1);
        }

        public void SetOverlay(IEnumerable<UIElement> toAdd)
        {
            ClearOverlay();
            foreach (UIElement element in toAdd)
            {
                canvasOverlay.Children.Add(element);
            }
        }

        public void ClearOverlay()
        {
            canvasOverlay.Children.Clear();
        }

        public void ZoomIn()
        {
            SetScaleFactor(_scaleFactor.Width + 0.5, _scaleFactor.Height + 0.5);
        }

        public void ZoomOut()
        {
            SetScaleFactor(Math.Max(1.0, _scaleFactor.Width - 0.5), Math.Max(1.0, _scaleFactor.Height - 0.5));
        }

        private void SetScaleFactor(double x, double y)
        {
            _scaleFactor = new Size(x, y);

            mainImage.RenderTransform = new ScaleTransform(x, y, _widthPixels / 2.0, _heightPixels / 2.0);
            SetControlSizes(_widthPixels * x, _heightPixels * y);
        }

        public void SetSource(BitmapSource source)
        {
            _widthPixels = source.PixelWidth;
            _heightPixels = source.PixelHeight;
            _palette = source.Palette;
            _pixelFormat = source.Format;
            _dpiX = source.DpiX;
            _dpiY = source.DpiY;
            
            _stride = _GetStride(_widthPixels, _pixelFormat.BitsPerPixel);

            _pixels = new byte[_stride * _heightPixels];
            source.CopyPixels(_pixels, _stride, offset: 0);

            // Just set all the alpha values to 255 for now since for some reason
            // the PrintScreen screen cap data contains 0 for all the alpha values
            for (int i = 3; i < _pixels.Length; i += 4)
            {
                _pixels[i] = byte.MaxValue;
            }

            _ReconstructBitmap();
            _SetSourceToInteralBitmap();
        }

        private void _SetSourceToInteralBitmap()
        {
            Debug.Assert(_bitmap != null);
            
            // Fix the width and height of the image to that of the bitmap so that we
            // correctly align to device pixels. If the bitmap and image are different
            // sizes WPF will try to be smart and make it fit which can cause blurring.
            SetControlSizes(_bitmap.PixelWidth, _bitmap.PixelHeight);
            mainImage.Source = _bitmap;
        }

        private void SetControlSizes(double x, double y)
        {
            mainImage.Width = x;
            mainImage.Height = y;

            canvas.Width = x;
            canvas.Height = y;

            canvasOverlay.Width = x;
            canvasOverlay.Height = y;
        }

        public void Crop(Rect rect)
        {
            var intRect = new Int32Rect((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);

            _widthPixels = intRect.Width;
            _heightPixels = intRect.Height;

            _stride = _GetStride(_widthPixels, _pixelFormat.BitsPerPixel);
            _pixels = new byte[_stride * _heightPixels];
            _bitmap.CopyPixels(intRect, _pixels, _stride, offset: 0);
            
            _ReconstructBitmap();
            _SetSourceToInteralBitmap(); 
        }

        private void _ReconstructBitmap()
        {
            _bitmap = BitmapSource.Create(_widthPixels, _heightPixels, _dpiX, _dpiY, _pixelFormat, _palette, _pixels, _stride);
        }

        private static int _GetStride(int bitmapWidthPixels, int bitmapBitsPerPixel)
        {
            return bitmapWidthPixels * ((bitmapBitsPerPixel + 7) / 8);
        }

        private void _OnImageMouseMove(object sender, MouseEventArgs e)
        {
            if (ImageMouseMove != null)
            {
                ImageMouseMove(sender, e);
            }
        }

        private void _OnImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ImageMouseDown != null)
            {
                ImageMouseDown(sender, e);
            }
        }

        private void _OnImageMouseUp(object sender, MouseButtonEventArgs e)
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
