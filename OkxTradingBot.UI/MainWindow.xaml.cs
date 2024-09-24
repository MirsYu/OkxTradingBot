using ccxt;
using CommunityToolkit.Mvvm.Input;
using OkxTradingBot.Core.Utils;
using OkxTradingBot.UI.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace OkxTradingBot.UI
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
    }
}