using ccxt;
using CommunityToolkit.Mvvm.Input;
using OkxTradingBot.Core.Utils;
using OkxTradingBot.UI.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace OkxTradingBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(); // 设置 ViewModel 作为 DataContext
        }
        private void SendScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            // 调用发送截图的方法
            ScreenshotHelper.SendScreenshot(this);
        }

        private void SendCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotHelper.SendText("测试Text9527ABCD#$/");
        }
    }
}