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
using System.Windows.Threading;
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

        private const double DETECTION_RATE = 30;   // maximum number of detections per second
        private const double CONFIDENCE_THRESHOLD = 0.5;

        private GrayBufferMarkerDetector arDetector;
        private bool isInitialized;
        private bool isDetecting;
        private DetectionResult markerResult;

        private byte[] buffer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var device = MultimediaUtil.VideoInputDevices.First();
            videoCaptureElement.VideoCaptureDevice = device;

            InitializeAR();

            var dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.0 / DETECTION_RATE) };
            dispatcherTimer.Tick += (s1, e1) => Detect();
            dispatcherTimer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CheckConfidence();
        }

        private void InitializeAR()
        {
            //  Initialize the Detector
            arDetector = new GrayBufferMarkerDetector();

            // Load the marker pattern. It has 16x16 segments and a width of 80 millimeters
            var marker = Marker.LoadFromResource("Assets/data/Marker_L_16x16segments_80width.pat", 16, 16, 80);

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

        private void Detect()
        {
            if (isDetecting || !isInitialized)
                return;

            //Here is where we try to detect the marker
            isDetecting = true;

            try
            {
                // Update buffer size
                var pixelWidth = WIDTH;
                var pixelHeight = HEIGHT;
                if (buffer == null || buffer.Length != pixelWidth * pixelHeight)
                {
                    buffer = new byte[pixelWidth * pixelHeight];
                }

                // Grab snapshot for the marker detection
                //_photoDevice.GetPreviewBufferY(buffer);
                RenderTargetBitmap bmp = new RenderTargetBitmap(WIDTH, HEIGHT, 96, 96, PixelFormats.Default);
                bmp.Render(videoCaptureElement);
                buffer = GetGrayscaleByteArray(bmp);

                //Detect the markers
                arDetector.Threshold = 150;
                var dr = arDetector.DetectAllMarkers(buffer, pixelWidth, pixelHeight);

                //Set the marker result if the marker is found
                //markerResult = dr.HasResults ? dr[0] : null;
                markerResult = dr.HasResults ? dr.OrderByDescending(r => r.Confidence).FirstOrDefault() : null;

                if (null != markerResult && markerResult.Confidence < CONFIDENCE_THRESHOLD)
                    markerResult = null;

                CheckConfidence();
            }
            finally
            {
                isDetecting = false;
            }
        }

        private void CheckConfidence()
        {
            if (null == markerResult)
            {
                confidenceTextBlock.Text = "0.00";
                centerMarker.Visibility = Visibility.Hidden;
            }
            else
            {
                confidenceTextBlock.Text = markerResult.Confidence.ToString();
                Canvas.SetLeft(centerMarker, markerResult.Square.Center.X);
                Canvas.SetTop(centerMarker, markerResult.Square.Center.Y);
                centerMarker.Visibility = Visibility.Visible;

            }
        }
    }
}
