using System;
using System.Threading; // Required for CancellationToken (引入此命名空间以支持取消正在运行的任务)
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FlashShake
{
    public partial class MainPage : ContentPage
    {
        // Variable to track if the flashlight is on or off (用来记录手电筒是开启还是关闭的变量)
        private bool isFlashlightOn = false;

        // Token source to control and cancel the SOS loop (用于控制和随时打断 SOS 循环的标记)
        private CancellationTokenSource sosCts;

        // Constructor of the page (页面的构造函数)
        public MainPage()
        {
            InitializeComponent();
        }

        // The method executed when the big circular power button is clicked (中心大开关的点击逻辑)
        private async void PowerButton_Clicked(object sender, EventArgs e)
        {
            await CheckAndRequestCameraPermission();

            // Stop any running SOS mode before toggling (在切换前停止可能正在运行的 SOS 模式)
            sosCts?.Cancel();

            // Toggle the boolean state (切换布尔状态)
            isFlashlightOn = !isFlashlightOn;
            // Default to normal mode (默认普通模式)
            await UpdateFlashlightUI("Normal");
        }

        // Helper method to check and request camera permission (检查并请求相机权限的辅助方法)
        private async Task CheckAndRequestCameraPermission()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied (权限被拒绝)", "Cannot use flashlight without camera permission. (没有相机权限无法使用手电筒。)", "OK (确定)");
            }
        }

        // The method executed when the SOS MODE button is clicked (点击 SOS 模式按钮时执行的方法)
        private async void SOSMode_Clicked(object sender, EventArgs e)
        {
            await CheckAndRequestCameraPermission();

            // Cancel any previous SOS task to avoid overlapping (取消之前的 SOS 任务以避免冲突)
            sosCts?.Cancel();
            // Create a new cancellation token for this SOS run (为这次 SOS 运行创建新的取消标记)
            sosCts = new CancellationTokenSource();

            // Force turn on the flashlight state (强制设置手电筒状态为开启)
            isFlashlightOn = true;
            await UpdateFlashlightUI("SOS");

            // Start the Morse code blinking in the background without awaiting it, so UI doesn't freeze 
            // (在后台开始摩斯电码闪烁，不使用 await 以免卡死界面)
            _ = RunSOSMorseCodeAsync(sosCts.Token);
        }

        // ============ Morse Code Logic (摩斯电码核心逻辑) ============

        // Method to run international standard SOS Morse code (运行国际标准 SOS 摩斯电码的方法)
        private async Task RunSOSMorseCodeAsync(CancellationToken token)
        {
            // Standard timings in milliseconds (毫秒级标准时间设定)
            int dot = 200;      // Short blink for 'S' (代表'S'的短闪烁)
            int dash = 600;     // Long blink for 'O' (代表'O'的长闪烁)
            int gap = 200;      // Gap between blinks (单次闪烁间的间隔)
            int letterGap = 600;// Gap between letters (字母间的间隔)
            int wordGap = 1400; // Gap between SOS sequences (每一次SOS循环完的间隔)

            try
            {
                // Loop infinitely until user cancels it (无限循环直到用户关闭)
                while (!token.IsCancellationRequested)
                {
                    // Letter S: 3 short blinks (字母 S: 3次短闪)
                    await BlinkSequenceAsync(dot, gap, 3, token);
                    await Task.Delay(letterGap - gap, token);

                    // Letter O: 3 long blinks (字母 O: 3次长闪)
                    await BlinkSequenceAsync(dash, gap, 3, token);
                    await Task.Delay(letterGap - gap, token);

                    // Letter S: 3 short blinks (字母 S: 3次短闪)
                    await BlinkSequenceAsync(dot, gap, 3, token);
                    await Task.Delay(wordGap, token);
                }
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled by the user, exit loop smoothly (任务被用户打断，平滑退出)
            }
            catch (Exception)
            {
                // Handle hardware errors silently (静默处理硬件异常)
            }
        }

        // Helper to execute a sequence of blinks (执行连续闪烁的底层辅助方法)
        private async Task BlinkSequenceAsync(int duration, int gap, int times, CancellationToken token)
        {
            for (int i = 0; i < times; i++)
            {
                token.ThrowIfCancellationRequested(); // Check if stopped before turning ON (开灯前检查是否被叫停)
                await Flashlight.Default.TurnOnAsync();
                await Task.Delay(duration, token);

                token.ThrowIfCancellationRequested(); // Check if stopped before turning OFF (关灯前检查是否被叫停)
                await Flashlight.Default.TurnOffAsync();
                await Task.Delay(gap, token);
            }
        }

        // ==========================================================

        // Helper method to uniformly control UI and Hardware (统一控制 UI 和硬件的辅助方法)
        private async Task UpdateFlashlightUI(string mode)
        {
            try
            {
                if (isFlashlightOn)
                {
                    StatusLabel.Text = "Status: Open";
                    StatusLabel.TextColor = Colors.Green;
                    ModeLabel.Text = $"Model: {mode}";

                    PowerButton.BackgroundColor = Colors.LightGreen;
                    PowerButton.Text = "ON";

                    // Turn on physical flashlight ONLY IF it's not SOS mode
                    // (只有在非 SOS 模式下，才在这里直接长亮手电筒。SOS模式已由摩斯电码方法接管)
                    if (mode != "SOS")
                    {
                        await Flashlight.Default.TurnOnAsync();
                    }
                }
                else
                {
                    StatusLabel.Text = "Status: Close";
                    StatusLabel.TextColor = Colors.Black;
                    ModeLabel.Text = "Model: Normal";

                    PowerButton.BackgroundColor = Color.FromArgb("#E0E0E0");
                    PowerButton.Text = "OFF";

                    // Turn off the physical flashlight (关闭物理手电筒)
                    await Flashlight.Default.TurnOffAsync();
                }
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Not Supported (不支持)", "This device does not have a flashlight. (此设备没有闪光灯。)", "OK (确定)");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error (错误)", $"Flashlight error: {ex.Message} (手电筒错误)", "OK (确定)");
            }
        }

        // ============ Bottom navigation bar click events (底部导航栏的点击事件) ============

        private void NavMain_Tapped(object sender, TappedEventArgs e)
        {
            // Already on the Main page, no need to jump (当前已经在主页，不用跳转)
        }

        private void NavShake_Tapped(object sender, TappedEventArgs e)
        {
            Application.Current.MainPage = new ShakePage();
        }

        private void NavSupport_Tapped(object sender, TappedEventArgs e)
        {
            DisplayAlert("Navigation", "Jumping to Support Page...", "OK");
        }

        private void NavSettings_Tapped(object sender, TappedEventArgs e)
        {
            DisplayAlert("Navigation", "Jumping to Settings Page...", "OK");
        }
    }
}
