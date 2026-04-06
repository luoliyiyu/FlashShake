using System;
using System.Threading; // Required for CancellationTokenSource (引入此命名空间以支持取消异步任务)
using System.Threading.Tasks; // Required for async Task (引入此命名空间以支持异步任务)
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FlashShake
{
    public partial class SupportPage : ContentPage
    {
        // Token source to control and cancel the scrolling animation (用于控制和随时打断文字滚动循环的标记)
        private CancellationTokenSource scrollCts;

        // Current state variables (当前的内部状态变量)
        private string currentScrollDirection = "Center"; // Center, Left, Right
        private double currentSpeedPixelsPerSecond = 100; // Pixels per second (每秒滚动的像素数)
        private bool isFirstLayout = true; // Flag for initial width calculation (用于初始宽度计算的标记)

        // Constructor of the page (页面的构造函数)
        public SupportPage()
        {
            InitializeComponent();
        }

        // Triggered after layout is done to handle initial width issues (在布局完成后触发，处理初始宽度获取问题)
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (isFirstLayout && PreviewFrame.Width > 0 && PreviewLabel.Width > 0)
            {
                isFirstLayout = false;
                // Wait a tiny bit more to ensure width measurements are stable in MAUI
                // (多等极其微小的一段时间以确保在 MAUI 中宽度测量已稳定)
                Task.Delay(50).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => StartScrollingAnimation()));
            }
        }

        // ============ 1. Marquee Logic (跑马灯滚动逻辑) ============

        // Helper method to stop current animation and start new based on current state (停止当前动画并根据当前状态开始新动画的辅助方法)
        private void StartScrollingAnimation()
        {
            // 1. Cancel previous scrolling task (1. 取消之前的滚动任务)
            scrollCts?.Cancel();
            scrollCts = new CancellationTokenSource();

            // 2. Start the infinite scrolling loop in background without awaiting (2. 在后台开始无限滚动循环，不使用 await 以免卡死界面)
            _ = RunMarqueeAnimationAsync(scrollCts.Token);
        }

        // The method running the infinite animation loop (运行无限动画循环的方法)
        private async Task RunMarqueeAnimationAsync(CancellationToken token)
        {
            // Time between frame updates in milliseconds (帧更新的时间间隔，毫秒，~60fps)
            int dtMs = 16;
            double dtSeconds = dtMs / 1000.0;

            try
            {
                // Loop infinitely until task is cancelled (无限循环直到任务被用户取消)
                while (!token.IsCancellationRequested)
                {
                    // Center Mode: Reset translation and HorizontalOptions, don't move (居中模式：重置平移和水平对齐，不动)
                    if (currentScrollDirection == "Center")
                    {
                        PreviewLabel.TranslationX = 0;
                        PreviewLabel.HorizontalOptions = LayoutOptions.Center;
                        await Task.Delay(50, token); // Wait longer when static (静态时等待久一些)
                        continue;
                    }

                    // For Left/Right, disable HorizontalOptions constraint (对于左/右滚动，禁用水平对齐约束)
                    PreviewLabel.HorizontalOptions = LayoutOptions.Start;

                    // 1. Get current width data (1. 获取当前宽度数据)
                    double viewportWidth = ViewportGrid.Bounds.Width; // Visible area width (可见区域宽度)
                    double contentWidth = PreviewLabel.DesiredSize.Width; // Actual text length (实际文字长度)

                    // 2. Ensure widths are measured (2. 确保宽度已被测量)
                    if (viewportWidth <= 0 || contentWidth <= 0)
                    {
                        await Task.Delay(100, token);
                        continue;
                    }

                    // 3. Movement logic (3. 移动逻辑)
                    double nextTranslationX = PreviewLabel.TranslationX;
                    double step = currentSpeedPixelsPerSecond * dtSeconds; // Pixels to move this frame (这一帧要移动的像素数)

                    if (currentScrollDirection == "Left") // LTR (从左向右滚动)
                    {
                        nextTranslationX += step;
                    }
                    else if (currentScrollDirection == "Right") // RTL (从右向左滚动)
                    {
                        nextTranslationX -= step;
                    }

                    // 4. Wrap around Logic (4. 越界回绕逻辑)
                    if (currentScrollDirection == "Left") // LTR Wrap (从左向右回绕)
                    {
                        // Wrap back to left side when fully disappeared off the right edge (从右边缘完全消失后从左侧重新出现)
                        if (nextTranslationX > viewportWidth + contentWidth / 2)
                            nextTranslationX = -viewportWidth / 2 - contentWidth;
                    }
                    else if (currentScrollDirection == "Right") // RTL Wrap (从右向左回绕)
                    {
                        // Wrap back to right side when fully disappeared off the left edge (从左边缘完全消失后从右侧重新出现)
                        if (nextTranslationX < -viewportWidth / 2 - contentWidth / 2)
                            nextTranslationX = viewportWidth / 2 + contentWidth;
                    }

                    // 5. Apply and Wait (5. 应用变动并等待下一帧)
                    PreviewLabel.TranslationX = nextTranslationX;
                    await Task.Delay(dtMs, token);
                }
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled by the user, exit loop smoothly (任务被用户叫停，平滑退出)
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Marquee Error: {ex.Message} (跑马灯错误)");
            }
        }

        // ============ 2. Option Buttons Logic (选项按钮逻辑) ============

        // Font size settings (字体大小设置逻辑)
        private void FontSize_Clicked(object sender, EventArgs e)
        {
            ResetButtonGroupStyle(BtnFontLow, BtnFontMedium, BtnFontHigh);
            HighlightSelectedButtonStyle((Button)sender);

            Button clicked = (Button)sender;
            if (clicked == BtnFontLow) PreviewLabel.FontSize = 24;
            else if (clicked == BtnFontMedium) PreviewLabel.FontSize = 36;
            else if (clicked == BtnFontHigh) PreviewLabel.FontSize = 48;

            // FontSize change affects width, restart loop to measure immediately 
            // (字号变化会影响宽度，立即重新启动循环进行测量)
            StartScrollingAnimation();
        }

        // Scroll settings (滚动方向设置逻辑 - Capitalized Scroll / 首字母大写)
        private void Scroll_Clicked(object sender, EventArgs e)
        {
            ResetButtonGroupStyle(BtnScrollLeft, BtnScrollCenter, BtnScrollRight);
            HighlightSelectedButtonStyle((Button)sender);

            Button clicked = (Button)sender;
            if (clicked == BtnScrollLeft) currentScrollDirection = "Left";
            else if (clicked == BtnScrollCenter) currentScrollDirection = "Center";
            else if (clicked == BtnScrollRight) currentScrollDirection = "Right";

            // Direction change, restart loop immediately (方向变化，立即重新启动循环)
            StartScrollingAnimation();
        }

        // Speed settings (速度设置逻辑 - Capitalized Speed / 首字母大写)
        private void Speed_Clicked(object sender, EventArgs e)
        {
            ResetButtonGroupStyle(BtnSpeedSlow, BtnSpeedNormal, BtnSpeedQuick);
            HighlightSelectedButtonStyle((Button)sender);

            Button clicked = (Button)sender;
            // Define speed in Pixels per second (定义每秒滚动的像素数)
            if (clicked == BtnSpeedSlow) currentSpeedPixelsPerSecond = 50;
            else if (clicked == BtnSpeedNormal) currentSpeedPixelsPerSecond = 100;
            else if (clicked == BtnSpeedQuick) currentSpeedPixelsPerSecond = 200;

            // Speed change, the loop reads currentSpeed automatically, no need to restart 
            // (速度变化，循环会自动读取新值，不需要重启)
        }

        // Helper method to reset button group colors (辅助方法：重置按钮组的样式为浅灰)
        private void ResetButtonGroupStyle(Button b1, Button b2, Button b3)
        {
            Color unselectedBg = Color.FromArgb("#E0E0E0"); // Light Gray (浅灰色)
            Color unselectedText = Colors.Black; // Black text (黑色文字)

            b1.BackgroundColor = unselectedBg; b1.TextColor = unselectedText;
            b2.BackgroundColor = unselectedBg; b2.TextColor = unselectedText;
            b3.BackgroundColor = unselectedBg; b3.TextColor = unselectedText;
        }

        // Helper method to highlight selected button (辅助方法：将选中的按钮样式设置为黑色高亮)
        private void HighlightSelectedButtonStyle(Button b)
        {
            b.BackgroundColor = Colors.Black;
            b.TextColor = Colors.White;
        }

        // ============ 3. Text Submit Logic (提交文字逻辑) ============
        private void Submit_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TextInput.Text))
            {
                PreviewLabel.Text = TextInput.Text;

                // Text change affects width, restart loop immediately to measure new width 
                // (文字变化会影响宽度，立即重新启动循环以测量新宽度)
                StartScrollingAnimation();
            }
        }

        // ============ 4. Color Scheme Logic (颜色方案逻辑) ============
        private void ColorScheme_Tapped(object sender, TappedEventArgs e)
        {
            // Reset all borders to transparent (将所有的边框重置为透明)
            Color1.Stroke = Colors.Transparent; Color2.Stroke = Colors.Transparent;
            Color3.Stroke = Colors.Transparent; Color4.Stroke = Colors.Transparent;
            Color5.Stroke = Colors.Transparent; Color6.Stroke = Colors.Transparent;
            Color7.Stroke = Colors.Transparent; Color8.Stroke = Colors.Transparent;

            // Get the clicked border (获取当前被点击的框)
            Border clickedBorder = (Border)sender;
            // Set a white border to indicate it's selected (添加白色线框标识选中状态)
            clickedBorder.Stroke = Colors.White;

            // Change the preview frame background and label text color accordingly (相应地改变预览框的背景色和文字颜色)
            string param = e.Parameter?.ToString();
            switch (param)
            {
                case "1": PreviewFrame.BackgroundColor = Colors.Black; PreviewLabel.TextColor = Colors.Blue; break; // Black bg, Blue text (黑底蓝字)
                case "2": PreviewFrame.BackgroundColor = Colors.Orange; PreviewLabel.TextColor = Colors.Black; break; // Orange bg, Black text (橙底黑字)
                case "3": PreviewFrame.BackgroundColor = Colors.Blue; PreviewLabel.TextColor = Colors.White; break;
                case "4": PreviewFrame.BackgroundColor = Colors.Red; PreviewLabel.TextColor = Colors.White; break;
                case "5": PreviewFrame.BackgroundColor = Colors.LightBlue; PreviewLabel.TextColor = Colors.Green; break;
                case "6": PreviewFrame.BackgroundColor = Colors.Green; PreviewLabel.TextColor = Colors.Black; break;
                case "7": PreviewFrame.BackgroundColor = Colors.Black; PreviewLabel.TextColor = Colors.Red; break;
                case "8": PreviewFrame.BackgroundColor = Colors.LightPink; PreviewLabel.TextColor = Colors.White; break;
            }
        }

        // ============ Bottom navigation bar click events (底部导航栏跳转事件) ============
        private void NavMain_Tapped(object sender, TappedEventArgs e)
        {
            scrollCts?.Cancel(); // Ensure animation stops on navigation (导航跳转前确保停止动画)
            Application.Current.MainPage = new MainPage();
        }

        private void NavShake_Tapped(object sender, TappedEventArgs e)
        {
            scrollCts?.Cancel(); // Ensure animation stops on navigation (导航跳转前确保停止动画)
            Application.Current.MainPage = new ShakePage();
        }

        private void NavSetting_Tapped(object sender, TappedEventArgs e)
        {
            scrollCts?.Cancel(); // Ensure animation stops on navigation (导航跳转前确保停止动画)
            Application.Current.MainPage = new SettingPage();
        }
    }
}
