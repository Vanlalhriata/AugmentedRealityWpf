using SLARToolKit;
using System;
using System.Collections.Generic;
using System.IO;
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
using WPFMediaKit.DirectShow.Controls;

namespace AugmentedRealityWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WIDTH = 640;
        private const int HEIGHT = 480;

        private GrayBufferMarkerDetector arDetector;
        private bool isInitialized;
        private bool isDetecting;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var device = MultimediaUtil.VideoInputDevices.First();
            videoCaptureElement.VideoCaptureDevice = device;

            InitializeAR();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap(WIDTH, HEIGHT, 96, 96, PixelFormats.Default);
            bmp.Render(videoCaptureElement);

            byte[] bytes = GetGrayscaleByteArray(bmp);

            // Don't need the code after this line

            previewImage.Source = bmp;
        }

        private void InitializeAR()
        {
            //  Initialize the Detector
            arDetector = new GrayBufferMarkerDetector();

            // Load the marker pattern. It has 16x16 segments and a width of 80 millimeters
            var marker = Marker.LoadFromResource("Assets/data/Marker_SLAR_16x16segments_80width.pat", 16, 16, 80);

            // The perspective projection has the near plane at 1 and the far plane at 4000
            arDetector.Initialize(WIDTH, HEIGHT, 1, 4000, marker);

            isInitialized = true;
        }

        private static byte[] GetGrayscaleByteArray(BitmapSource bitmapSource)
        {
            FormatConvertedBitmap fcb = new FormatConvertedBitmap(bitmapSource, PixelFormats.Gray8, null, 0);

            // Stride = bytes in each row = pixel width * bytes per pixel
            int stride = fcb.PixelWidth * ((fcb.Format.BitsPerPixel + 7) / 8);

            byte[] bytes = new byte[stride * fcb.PixelHeight];

            fcb.CopyPixels(bytes, stride, 0);

            return bytes;
        }
    }
}
