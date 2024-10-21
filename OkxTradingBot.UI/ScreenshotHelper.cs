using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OkxTradingBot
{
    public static class ScreenshotHelper
    {
        public static byte[] CaptureWindowScreenshot(Window window)
        {
            // 获取窗口的尺寸
            var bounds = VisualTreeHelper.GetDescendantBounds(window);

            // 创建 RenderTargetBitmap
            var renderTarget = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);

            // 渲染窗口内容
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                var brush = new VisualBrush(window);
                drawingContext.DrawRectangle(brush, null, new Rect(new System.Windows.Point(), bounds.Size));
            }

            renderTarget.Render(drawingVisual);

            // 将 RenderTargetBitmap 转换为 PNG 格式的字节数组
            using (var memoryStream = new MemoryStream())
            {
                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderTarget));
                pngEncoder.Save(memoryStream);
                return memoryStream.ToArray(); // 返回字节数组
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern bool ActivateKeyboardLayout(IntPtr hkl, uint flags);


        // 英文输入法的键盘布局标识符
        private static readonly IntPtr EnglishKeyboardLayout = new IntPtr(0x0409); // 美国英语

        private const byte VK_CONTROL = 0x11;
        private const byte VK_V = 0x56;
        private const byte VK_RETURN = 0x0D;

        public static void SendScreenshot(Window window)
        {
            // 切换到英文输入法
            SwitchToEnglishInputMethod();
            // 捕获窗口截图
            byte[] screenshotBytes = ScreenshotHelper.CaptureWindowScreenshot(window);

            // 找到微信窗口
            IntPtr wechatWindow = FindWindow(null, "灰眼1307"); // 使用中文标题“微信”
            if (wechatWindow == IntPtr.Zero)
            {
                System.Windows.MessageBox.Show("未找到微信窗口。");
                return;
            }

            // 激活微信窗口
            ShowWindow(wechatWindow, 5); // SW_SHOW
            SetForegroundWindow(wechatWindow);

            // 将截图发送到剪贴板
            using (var image = (Bitmap)Image.FromStream(new MemoryStream(screenshotBytes)))
            {
                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    image.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
                System.Windows.Clipboard.SetImage(bitmapSource);
            }

            // 模拟 Ctrl + V 发送截图
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero); // 按下 Ctrl
            keybd_event(VK_V, 0, 0, UIntPtr.Zero);       // 按下 V
            keybd_event(VK_V, 0, 2, UIntPtr.Zero);       // 释放 V
            keybd_event(VK_CONTROL, 0, 2, UIntPtr.Zero); // 释放 Ctrl

            // 模拟 Enter 键
            keybd_event(VK_RETURN, 0, 0, UIntPtr.Zero); // 按下 Enter
            keybd_event(VK_RETURN, 0, 2, UIntPtr.Zero); // 释放 Enter
        }

        public static void SendText(String Text)
        {
            // 切换到英文输入法
            SwitchToEnglishInputMethod();
            // 获取输入文本
            string inputText = Text;

            // 找到微信窗口
            IntPtr wechatWindow = ScreenshotHelper.FindWindow(null, "灰眼1303"); // 使用中文标题“微信”
            if (wechatWindow == IntPtr.Zero)
            {
                //System.Windows.MessageBox.Show("未找到微信窗口。");
                return;
            }

            // 激活微信窗口
            ScreenshotHelper.ShowWindow(wechatWindow, 5); // SW_SHOW
            ScreenshotHelper.SetForegroundWindow(wechatWindow);

            // 输入字符
            foreach (char c in inputText)
            {
                Thread.Sleep(50); // 可以调节速度
                // 模拟键盘输入字符
                SendKeys.SendWait(c.ToString());
            }

            // 模拟 Enter 键
            keybd_event(VK_RETURN, 0, 0, UIntPtr.Zero); // 按下 Enter
            keybd_event(VK_RETURN, 0, 2, UIntPtr.Zero); // 释放 Enter
        }

        private static void SwitchToEnglishInputMethod()
        {
            // 获取当前线程的输入法
            IntPtr hkl = GetKeyboardLayout((uint)System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (hkl != EnglishKeyboardLayout)
            {
                // 切换到英文输入法
                ActivateKeyboardLayout(EnglishKeyboardLayout, 0);
            }

            //// 切换到英文输入法
            //var currentLang = InputLanguage.CurrentInputLanguage;
            //System.Windows.MessageBox.Show($"当前输入法: {currentLang.Culture.Name}"); // 输出当前输入法

            //var englishCulture = new CultureInfo("en-US");
            //InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(englishCulture);

            //System.Windows.MessageBox.Show($"切换后输入法: {InputLanguage.CurrentInputLanguage.Culture.Name}"); // 输出切换后的输入法
        }
    }
}
