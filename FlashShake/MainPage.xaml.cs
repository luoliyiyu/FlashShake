using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FlashShake
{
    public partial class MainPage : ContentPage
    {
        // Variable to track if the flashlight is on or off (用来记录手电筒是开启还是关闭的变量)
        private bool isFlashlightOn = false;

        // Constructor of the page (页面的构造函数)
        public MainPage()
        {
            InitializeComponent();
        }

        // The method executed when the big circular power button is clicked (中心大开关的点击逻辑)
        private void PowerButton_Clicked(object sender, EventArgs e)
        {
            // Toggle the boolean state (切换布尔状态)
            isFlashlightOn = !isFlashlightOn; 
            // Default to normal mode (默认普通模式)
            UpdateFlashlightUI("Normal"); 
        }

        // The method executed when the SOS MODE button is clicked (点击 SOS 模式按钮时执行的方法)
        private void SOSMode_Clicked(object sender, EventArgs e)
        {
            // Force turn on the flashlight (强制开启手电筒)
            isFlashlightOn = true; 
            // Switch to SOS mode (切换到 SOS 模式)
            UpdateFlashlightUI("SOS"); 
        }

        // Helper method to uniformly control UI changes to avoid duplicate code (统一控制 UI 变化的辅助方法，避免重复写代码)
        private void UpdateFlashlightUI(string mode)
        {
            if (isFlashlightOn)
            {
                // If turned ON, change UI to active state (如果已开启，将界面改为激活状态的样式)
                StatusLabel.Text = "Status: Open";
                StatusLabel.TextColor = Colors.Green;
                ModeLabel.Text = $"Model: {mode}";
                
                PowerButton.BackgroundColor = Colors.LightGreen;
                PowerButton.Text = "ON";
            }
            else
            {
                // If turned OFF, change UI back to default state (如果已关闭，将界面改回默认状态样式)
                StatusLabel.Text = "Status: Close";
                StatusLabel.TextColor = Colors.Black;
                ModeLabel.Text = "Model: Normal";
                
                PowerButton.BackgroundColor = Color.FromArgb("#E0E0E0");
                PowerButton.Text = "OFF";
            }
        }

        // ============ Bottom navigation bar click events (底部导航栏的点击事件) ============

        private void NavMain_Tapped(object sender, TappedEventArgs e)
        {
            // Already on the Main page, no need to jump (当前已经在主页，不用跳转)
        }

        private void NavShake_Tapped(object sender, TappedEventArgs e)
        {
            // Future code to navigate to the Shake page (未来跳转到摇一摇页面的代码)
            DisplayAlert("Navigation", "Jumping to Shake Page...", "OK");
        }

        private void NavSupport_Tapped(object sender, TappedEventArgs e)
        {
            // Future code to navigate to the Support page (未来跳转到辅助页面的代码)
            DisplayAlert("Navigation", "Jumping to Support Page...", "OK");
        }

        private void NavSettings_Tapped(object sender, TappedEventArgs e)
        {
            // Future code to navigate to the Settings page (未来跳转到设置页面的代码)
            DisplayAlert("Navigation", "Jumping to Settings Page...", "OK");
        }
    }
}
