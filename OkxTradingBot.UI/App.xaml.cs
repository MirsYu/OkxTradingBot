using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace OkxTradingBot.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 创建CefSharp设置
            var settings = new CefSettings();
            // 初始化 CefSharp
            Cef.Initialize(settings);

            base.OnStartup(e); // 调用基类的启动逻辑
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 关闭 CefSharp 浏览器进程
            Cef.Shutdown();
            base.OnExit(e);
        }
    }
}
