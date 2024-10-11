using ccxt;
using CommunityToolkit.Mvvm.Input;
using OkxTradingBot.Core.Api;
using OkxTradingBot.Core.Strategies;
using OkxTradingBot.Core.Utils;
using OkxTradingBot.UI.ViewModel;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace OkxTradingBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel();
            DataContext = viewModel; // 设置 ViewModel 作为 DataContext
                                     // 订阅交易信号变化事件
            viewModel.TradeSignalChanged += ViewModel_TradeSignalChanged;
            viewModel.LogEntries.CollectionChanged += LogListBox_CollectionChanged; // 绑定事件
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

        private void ViewModel_TradeSignalChanged(object sender, string signal)
        {
            // 判断信号类型并执行相应的方法
            // Hold
            if (signal.Contains("Buy"))
            {
                // 执行买入逻辑
                ScreenshotHelper.SendText("生成买入信号");
                ScreenshotHelper.SendScreenshot(this);
            }
            else if (signal.Contains("Sell"))
            {
                // 执行卖出逻辑
                ScreenshotHelper.SendText("生成卖出信号");
                ScreenshotHelper.SendScreenshot(this);
            }
        }

        private void LogListBox_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (logListBox.Items.Count > 0)
                    {
                        logListBox.ScrollIntoView(logListBox.Items[^1]); // 滚动到最后一项
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }


        // Helper 方法，用于查找 Visual Tree 中的子元素
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }

    }
}