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
using System.Windows.Shapes;

namespace CameraScanQRCode
{
    /// <summary>
    /// QRCodeInfo.xaml 的交互逻辑
    /// </summary>
    public partial class QRCodeInfo : Window
    {
        private ScanQRCode scanQRCode;
        public QRCodeInfo(ScanQRCode main, string info)
        {
            InitializeComponent();
            scanQRCode = main;
            tbInfo.Text = info;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            scanQRCode.ReStart();
        }
    }
}
