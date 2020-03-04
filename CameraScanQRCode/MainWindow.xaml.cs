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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CameraScanQRCode
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCaptureDevice Camera = null;
        private DispatcherTimer scanTimer;
        private Bitmap img;
        private bool isScaning = false;
        private int top = 50;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                scanTimer = new DispatcherTimer();
                scanTimer.Interval = TimeSpan.FromMilliseconds(1000);
                scanTimer.Tick += ScanTimer_Tick;
                var devs = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (devs.Count != 0)
                {
                    Camera = new VideoCaptureDevice(devs[0].MonikerString);
                    Camera.VideoResolution = Camera.VideoCapabilities[0];
                    Camera.NewFrame += Camera_NewFrame;
                    Camera.Start();
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



        private void Camera_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
            if (isScaning)
            {
                img = (Bitmap)eventArgs.Frame.Clone();
            }
            bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
            if (isScaning)
            {
                Bitmap img2 = (Bitmap)bmp.Clone();
                System.Drawing.Pen p = new System.Drawing.Pen(System.Drawing.Color.YellowGreen, (float)4);
                Graphics g = Graphics.FromImage(img2);
                System.Drawing.Point p1 = new System.Drawing.Point(15, top);
                System.Drawing.Point p2 = new System.Drawing.Point(imagePlay.Width * 2, top);
                g.DrawLine(p, p1, p2);
                g.Dispose();
                top += 5;
                if (top >= 400)
                {
                    top = 50;
                }
                //top = top % imagePlay.Height;
                imagePlay.Image = img2;
            }
            else
            {
                imagePlay.Image = bmp;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Camera.Stop();
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
                isScaning = false;
                scanTimer.Stop();
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    btnScan.Content = "Scan";
                    tbQRCode.Text = result.Text;
                }));

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            deQRCode();
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            if (!isScaning)
            {
                scanTimer.Start();
                isScaning = true;
                btnScan.Content = "Stop";
            }
            else
            {
                scanTimer.Stop();
                isScaning = false;
                btnScan.Content = "Scan";
            }
        }
    }
}
