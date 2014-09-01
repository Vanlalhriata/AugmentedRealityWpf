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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var device = MultimediaUtil.VideoInputDevices.First();
            videoCaptureElement.VideoCaptureDevice = device;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap(640, 480, 96, 96, PixelFormats.Default);
            bmp.Render(videoCaptureElement);
            
            FormatConvertedBitmap fcb = new FormatConvertedBitmap(bmp, PixelFormats.Gray8, null, 0);

            // Stride = bytes in each row = pixel width * bytes per pixel
            int stride = fcb.PixelWidth * ((fcb.Format.BitsPerPixel + 7) / 8);

            byte[] bytes = new byte[stride * fcb.PixelHeight];

            fcb.CopyPixels(bytes, stride, 0);

            // Don't need the code after this line

            previewImage.Source = fcb;
        }
    }
}
