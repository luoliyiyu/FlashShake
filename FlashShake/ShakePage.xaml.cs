using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

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
            }
        }

        private void NormalPlus_Clicked(object sender, EventArgs e)
        {
            normalShakeCount++;
            NormalTimesLabel.Text = normalShakeCount.ToString();
        }

        private void SOSMinus_Clicked(object sender, EventArgs e)
        {
            // Ensure shake count doesn't go below 1 (确保摇晃次数不低于 1)
            if (sosShakeCount > 1)
            {
                sosShakeCount--;
                SOSTimesLabel.Text = sosShakeCount.ToString();
            }
        }

        private void SOSPlus_Clicked(object sender, EventArgs e)
        {
            sosShakeCount++;
            SOSTimesLabel.Text = sosShakeCount.ToString();
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
