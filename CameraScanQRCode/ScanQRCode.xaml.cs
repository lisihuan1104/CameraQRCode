using AForge.Video.DirectShow;
using com.google.zxing;
using com.google.zxing.common;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CameraScanQRCode
{
    /// <summary>
    /// ScanQRCode.xaml 的交互逻辑
    /// </summary>
    public partial class ScanQRCode : Window
    {
        private VideoCaptureDevice Camera = null;
        private DispatcherTimer scanTimer;
        private DispatcherTimer scanLineTimer;
        private Bitmap img;
        private int top = -300;
        public ScanQRCode()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                scanTimer = new DispatcherTimer();
                scanTimer.Interval = TimeSpan.FromMilliseconds(1000);
                scanTimer.Tick += ScanTimer_Tick;
                scanLineTimer = new DispatcherTimer();
                scanLineTimer.Interval = TimeSpan.FromMilliseconds(10);
                scanLineTimer.Tick += ScanLineTimer_Tick;
                var devs = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (devs.Count != 0)
                {
                    Camera = new VideoCaptureDevice(devs[0].MonikerString);
                    Camera.VideoResolution = Camera.VideoCapabilities[0];
                    Camera.NewFrame += Camera_NewFrame;
                    Camera.Start();
                    scanLineTimer.Start();
                    scanTimer.Start();
                }
                else
                {
                    MessageBox.Show("摄像头不存在!");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("摄像头不存在!");
            }
        }

        private void ScanLineTimer_Tick(object sender, EventArgs e)
        {
            imgScan.Margin = new Thickness(0, top, 0, 0);
            top += 5;
            if (top >= 300)
            {
                top = -300;
            }
        }

        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            deQRCode();
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        private ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            wpfBitmap.Freeze();
            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return wpfBitmap;
        }

        private void Camera_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
            img= (Bitmap)eventArgs.Frame.Clone();
            bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                imagePlay.Source = ChangeBitmapToImageSource(bmp);
            }));

        }

        private Task<bool> deQRCode()
        {
            var task = Task.Run(() =>
            {
                bool result = Zxing();
                return result;
            });
            return task;
        }

        private bool Zxing()
        {
            LuminanceSource source = new RGBLuminanceSource(img, img.Width, img.Height);
            com.google.zxing.BinaryBitmap bitmap1 = new com.google.zxing.BinaryBitmap(new HybridBinarizer(source));
            Result result;
            try
            {
                result = new MultiFormatReader().decode(bitmap1);
                Camera.Stop();
                scanTimer.Stop();
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    epStatus.Visibility = Visibility.Visible;
                    imgScan.Visibility = Visibility.Collapsed;
                    QRCodeInfo qRCodeInfo = new QRCodeInfo(this, result.Text);
                    qRCodeInfo.Show();
                }));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Camera != null)
            {
                Camera.Stop();
            }
        }

        public void ReStart()
        {
            Camera.Start();
            epStatus.Visibility = Visibility.Collapsed;
            imgScan.Visibility = Visibility.Visible;
            scanTimer.Start();
        }
    }
}
