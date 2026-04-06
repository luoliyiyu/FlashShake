using System;
using System.Threading.Tasks; // Required for async animation Tasks (引入以支持异步动画任务)
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes; // Required for Border and Ellipse (引入以识别 XAML 中的图形)
using Microsoft.Maui.Graphics;

namespace FlashShake
{
    public partial class SettingPage : ContentPage
    {
        // Constructor (构造函数)
        public SettingPage()
        {
            InitializeComponent();
        }

        // ============ Custom Drawn Switch Logic (纯代码手绘开关逻辑) ============

        // The method executed when our custom Border switch is tapped (点击自定义边框开关时执行的方法)
        private async void CustomSwitch_Tapped(object sender, EventArgs e)
        {
            // Cast sender to Border (将触发对象转换为外层边框 Border)
            Border switchBorder = (Border)sender;

            // Get the inner circle/thumb which is the only child of the Border (获取边框内部唯一的子元素：小圆圈)
            Ellipse thumb = (Ellipse)switchBorder.Content;

            // Determine current state by checking TranslationX (通过平移坐标判断当前状态)
            // TranslationX == 0 means it's OFF (在最左侧，代表关闭)
            bool isOff = thumb.TranslationX == 0;

            if (isOff)
            {
                // Turn ON action (执行开启操作)

                // 1. Change colors to match active theme (Blue background, White circle)
                // (1. 更改颜色以匹配激活主题：蓝色背景，白色小球)
                switchBorder.BackgroundColor = Color.FromArgb("#0050EF");
                switchBorder.Stroke = Color.FromArgb("#0050EF");
                thumb.Fill = Colors.White;

                // 2. Animate the thumb to the right side (2. 播放小球向右平移的动画)
                // 50 (Total Width) - 4 (Padding) - 22 (Thumb Width) = 24 pixels to move
                await thumb.TranslateTo(24, 0, 150, Easing.CubicInOut);
            }
            else
            {
                // Turn OFF action (执行关闭操作)

                // 1. Revert colors to default gray theme (1. 将颜色还原为默认的灰色主题)
                switchBorder.BackgroundColor = Colors.Transparent;
                switchBorder.Stroke = Color.FromArgb("#A0A0A0");
                thumb.Fill = Color.FromArgb("#A0A0A0");

                // 2. Animate the thumb back to the left side (2. 播放小球向左平移回到原点的动画)
                await thumb.TranslateTo(0, 0, 150, Easing.CubicInOut);
            }
        }

        // ============ Action Buttons Logic (操作按钮逻辑) ============

        // Logic for Restore default (恢复默认设置逻辑)
        private async void Restore_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Restore Default", "Are you sure you want to restore all settings to default? (确定要恢复默认设置吗？)", "Yes", "No");
            if (answer)
            {
                await DisplayAlert("Success", "Settings restored. (设置已恢复)", "OK");
            }
        }

        // Logic for Saving settings (保存设置逻辑)
        private async void Save_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Success", "Settings saved successfully! (设置保存成功！)", "OK");
        }

        // ============ Navigation Events (底部导航栏跳转事件) ============

        private void NavMain_Tapped(object sender, TappedEventArgs e)
        {
            Application.Current.MainPage = new MainPage();
        }

        private void NavShake_Tapped(object sender, TappedEventArgs e)
        {
            Application.Current.MainPage = new ShakePage();
        }

        private void NavSupport_Tapped(object sender, TappedEventArgs e)
        {
            Application.Current.MainPage = new SupportPage();
        }
    }
}
