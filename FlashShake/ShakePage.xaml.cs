using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage; // Required for Preferences local storage (引入以支持 Preferences 本地存储)

namespace FlashShake
{
    public partial class ShakePage : ContentPage
    {
        // Variables to store shake times (用来存储摇晃次数的变量)
        private int normalShakeCount = 2;
        private int sosShakeCount = 3;

        // Constructor of the page (页面的构造函数)
        public ShakePage()
        {
            InitializeComponent();
            // Load saved shake settings when page initializes (页面加载时读取已保存的摇一摇设置)
            LoadCurrentSettings();
        }

        // Method to load saved settings into UI (加载已保存的设置到 UI 的方法)
        private void LoadCurrentSettings()
        {
            // Load saved counts, use default values if not found (加载保存的次数，如果没有则使用默认值)
            normalShakeCount = Preferences.Default.Get("NormalShakeCount", 2);
            sosShakeCount = Preferences.Default.Get("SOSShakeCount", 3);
            NormalTimesLabel.Text = normalShakeCount.ToString();
            SOSTimesLabel.Text = sosShakeCount.ToString();

            // Load sensitivity and restore the corresponding button highlight state (加载灵敏度并恢复对应的按钮高亮状态)
            string sensitivity = Preferences.Default.Get("ShakeSensitivity", "Medium");
            ResetButtonsStyle();
            if (sensitivity == "Low") { BtnLow.BackgroundColor = Colors.Black; BtnLow.TextColor = Colors.White; }
            else if (sensitivity == "High") { BtnHigh.BackgroundColor = Colors.Black; BtnHigh.TextColor = Colors.White; }
            else { BtnMedium.BackgroundColor = Colors.Black; BtnMedium.TextColor = Colors.White; }
        }

        // Method executed when any sensitivity button is clicked (点击任何灵敏度按钮时执行的方法)
        private void Sensitivity_Clicked(object sender, EventArgs e)
        {
            // Reset all buttons to unselected style (将所有按钮重置为未选中时的浅灰色样式)
            ResetButtonsStyle();

            // Cast the sender to a Button object to know which one was clicked (将触发事件的对象转换为 Button)
            Button clickedButton = (Button)sender;

            // Set the clicked button to selected style (将当前点击的按钮设置为选中时的黑色背景和白色文字)
            clickedButton.BackgroundColor = Colors.Black;
            clickedButton.TextColor = Colors.White;

            // Save the selected sensitivity to local storage in real-time (将选择的灵敏度实时保存到本地)
            Preferences.Default.Set("ShakeSensitivity", clickedButton.Text);
        }

        // Helper method to reset button styles (重置按钮样式的辅助方法)
        private void ResetButtonsStyle()
        {
            Color unselectedBgColor = Color.FromArgb("#E0E0E0");
            Color unselectedTextColor = Colors.Black;

            BtnLow.BackgroundColor = unselectedBgColor;
            BtnLow.TextColor = unselectedTextColor;

            BtnMedium.BackgroundColor = unselectedBgColor;
            BtnMedium.TextColor = unselectedTextColor;

            BtnHigh.BackgroundColor = unselectedBgColor;
            BtnHigh.TextColor = unselectedTextColor;
        }

        // ============ Shake times logic (摇晃次数增减逻辑) ============

        private void NormalMinus_Clicked(object sender, EventArgs e)
        {
            // Ensure shake count doesn't go below 1 (确保摇晃次数不低于 1)
            if (normalShakeCount > 1)
            {
                normalShakeCount--;
                NormalTimesLabel.Text = normalShakeCount.ToString();

                // Save shake count for Normal mode (保存 Normal 模式摇晃次数)
                Preferences.Default.Set("NormalShakeCount", normalShakeCount);
            }
        }

        private void NormalPlus_Clicked(object sender, EventArgs e)
        {
            normalShakeCount++;
            NormalTimesLabel.Text = normalShakeCount.ToString();

            // Save shake count for Normal mode (保存 Normal 模式摇晃次数)
            Preferences.Default.Set("NormalShakeCount", normalShakeCount);
        }

        private void SOSMinus_Clicked(object sender, EventArgs e)
        {
            // Ensure shake count doesn't go below 1 (确保摇晃次数不低于 1)
            if (sosShakeCount > 1)
            {
                sosShakeCount--;
                SOSTimesLabel.Text = sosShakeCount.ToString();

                // Save shake count for SOS mode (保存 SOS 模式摇晃次数)
                Preferences.Default.Set("SOSShakeCount", sosShakeCount);
            }
        }

        private void SOSPlus_Clicked(object sender, EventArgs e)
        {
            sosShakeCount++;
            SOSTimesLabel.Text = sosShakeCount.ToString();

            // Save shake count for SOS mode (保存 SOS 模式摇晃次数)
            Preferences.Default.Set("SOSShakeCount", sosShakeCount);
        }

        // ============ Bottom navigation bar click events (底部导航栏的点击事件) ============

        private void NavMain_Tapped(object sender, TappedEventArgs e)
        {
            // Switch the root page back to MainPage (将应用程序的根页面替换回 MainPage)
            Application.Current.MainPage = new MainPage();
        }

        private void NavSupport_Tapped(object sender, TappedEventArgs e)
        {
            Application.Current.MainPage = new SupportPage();
        }

        private void NavSettings_Tapped(object sender, TappedEventArgs e)
        {
            Application.Current.MainPage = new SettingPage();
        }
    }
}
