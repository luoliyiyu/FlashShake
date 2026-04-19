using System;
using System.Threading.Tasks; // Required for async animation Tasks (引入以支持异步动画任务)
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes; // Required for Border and Ellipse (引入以识别 XAML 中的图形)
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage; // [Added] Required for Preferences local storage (引入以支持 Preferences 本地存储)
using Microsoft.Maui.Devices; // [Added] Required for hardware flashlight control (引入以支持底层硬件闪光灯控制)

namespace FlashShake
{
    public partial class SettingPage : ContentPage
    {
        // Constructor (构造函数)
        public SettingPage()
        {
            InitializeComponent();

            // [Added] Load saved settings when page initializes (页面加载时读取已保存的设置)
            LoadSettings();
        }

        // ============ [Added] Settings Initialization Logic (设置初始化逻辑) ============

        // Method to load visual states of custom switches (加载自定义开关视觉状态的方法)
        private void LoadSettings()
        {
            // Initialize visual states without animation based on saved Preferences (根据保存的偏好设置，无动画地初始化视觉状态)
            if (SwitchFlashlight != null) SetSwitchVisualState(SwitchFlashlight, Preferences.Default.Get("IsFlashlightOn", false));
            if (SwitchMode != null) SetSwitchVisualState(SwitchMode, Preferences.Default.Get("IsSpecialMode", false));
            if (SwitchSOS != null) SetSwitchVisualState(SwitchSOS, Preferences.Default.Get("IsSOSActive", false));
            if (SwitchPowerSaving != null) SetSwitchVisualState(SwitchPowerSaving, Preferences.Default.Get("PowerSaving", false));
            if (SwitchCheckUpdate != null) SetSwitchVisualState(SwitchCheckUpdate, Preferences.Default.Get("CheckUpdate", true)); // Default true (默认开启)
            if (SwitchAutoSave != null) SetSwitchVisualState(SwitchAutoSave, Preferences.Default.Get("AutoSave", true)); // Default true (默认开启)
        }

        // Helper method to set custom switch visual state instantly (立即设置自定义开关视觉状态的辅助方法)
        private void SetSwitchVisualState(Border switchBorder, bool isOn)
        {
            if (switchBorder.Content is Ellipse thumb)
            {
                if (isOn)
                {
                    switchBorder.BackgroundColor = Color.FromArgb("#0050EF");
                    switchBorder.Stroke = Color.FromArgb("#0050EF");
                    thumb.Fill = Colors.White;
                    thumb.TranslationX = 24; // Directly position to the right (直接定位到右侧)
                }
                else
                {
                    switchBorder.BackgroundColor = Colors.Transparent;
                    switchBorder.Stroke = Color.FromArgb("#A0A0A0");
                    thumb.Fill = Color.FromArgb("#A0A0A0");
                    thumb.TranslationX = 0; // Directly position to the left (直接定位到左侧)
                }
            }
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

            // [Added] Variable to hold the new state after tap (存储点击后的新状态变量)
            bool newState = false;

            if (isOff)
            {
                // Turn ON action (执行开启操作)
                newState = true;

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
                newState = false;

                // 1. Revert colors to default gray theme (1. 将颜色还原为默认的灰色主题)
                switchBorder.BackgroundColor = Colors.Transparent;
                switchBorder.Stroke = Color.FromArgb("#A0A0A0");
                thumb.Fill = Color.FromArgb("#A0A0A0");

                // 2. Animate the thumb back to the left side (2. 播放小球向左平移回到原点的动画)
                await thumb.TranslateTo(0, 0, 150, Easing.CubicInOut);
            }

            // [Added] Handle specific functionality based on which switch triggered the event (根据触发事件的开关处理具体功能)
            await ExecuteSwitchFunction(switchBorder, newState);
        }

        // [Added] Method to route logic to the correct feature (将逻辑路由到正确功能的方法)
        private async Task ExecuteSwitchFunction(Border switchBorder, bool newState)
        {
            // Check if Auto Save is enabled globally (检查全局是否启用了自动保存)
            bool isAutoSaveOn = Preferences.Default.Get("AutoSave", true);

            if (switchBorder == SwitchFlashlight)
            {
                if (isAutoSaveOn) Preferences.Default.Set("IsFlashlightOn", newState);
                try
                {
                    if (newState) await Flashlight.Default.TurnOnAsync();
                    else await Flashlight.Default.TurnOffAsync();
                }
                catch { /* Handle emulator limits (处理模拟器限制) */ }
            }
            else if (switchBorder == SwitchMode)
            {
                if (isAutoSaveOn) Preferences.Default.Set("IsSpecialMode", newState);
            }
            else if (switchBorder == SwitchSOS)
            {
                if (isAutoSaveOn) Preferences.Default.Set("IsSOSActive", newState);

                if (newState)
                {
                    // Turn off normal flashlight visually and functionally to avoid conflict (在视觉和功能上关闭普通闪光灯以避免冲突)
                    if (SwitchFlashlight != null) SetSwitchVisualState(SwitchFlashlight, false);
                    if (isAutoSaveOn) Preferences.Default.Set("IsFlashlightOn", false);

                    // You can call your global SOS method here (你可以在此处调用全局 SOS 方法)
                    // App.StartSOSPattern(); 
                }
                else
                {
                    // App.StopSOSPattern();
                    try { await Flashlight.Default.TurnOffAsync(); } catch { }
                }
            }
            else if (switchBorder == SwitchPowerSaving)
            {
                if (isAutoSaveOn) Preferences.Default.Set("PowerSaving", newState);
            }
            else if (switchBorder == SwitchCheckUpdate)
            {
                if (isAutoSaveOn) Preferences.Default.Set("CheckUpdate", newState);
            }
            else if (switchBorder == SwitchAutoSave)
            {
                // Auto save toggle itself must always be saved instantly (自动保存开关本身必须始终立即保存)
                Preferences.Default.Set("AutoSave", newState);
            }
        }

        // ============ Action Buttons Logic (操作按钮逻辑) ============

        // Logic for Restore default (恢复默认设置逻辑)
        private async void Restore_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Restore Default", "Are you sure you want to restore all settings to default? (确定要恢复默认设置吗？)", "Yes", "No");
            if (answer)
            {
                // [Added] Clear all saved preferences (清除所有保存的偏好设置)
                Preferences.Default.Clear();

                // [Added] Turn off hardware flashlight if it's currently on (如果硬件闪光灯当前亮着则关闭)
                try { await Flashlight.Default.TurnOffAsync(); } catch { }

                // [Added] Reload UI to match default false/true states (重新加载UI以匹配默认的假/真状态)
                LoadSettings();

                await DisplayAlert("Success", "Settings restored. (设置已恢复)", "OK");
            }
        }

        // Logic for Saving settings (保存设置逻辑)
        private async void Save_Clicked(object sender, EventArgs e)
        {
            // [Added] Manually read the translation value of each custom switch and save it (手动读取每个自定义开关的平移值并保存)
            if (SwitchFlashlight?.Content is Ellipse t1) Preferences.Default.Set("IsFlashlightOn", t1.TranslationX > 0);
            if (SwitchMode?.Content is Ellipse t2) Preferences.Default.Set("IsSpecialMode", t2.TranslationX > 0);
            if (SwitchSOS?.Content is Ellipse t3) Preferences.Default.Set("IsSOSActive", t3.TranslationX > 0);
            if (SwitchPowerSaving?.Content is Ellipse t4) Preferences.Default.Set("PowerSaving", t4.TranslationX > 0);
            if (SwitchCheckUpdate?.Content is Ellipse t5) Preferences.Default.Set("CheckUpdate", t5.TranslationX > 0);
            if (SwitchAutoSave?.Content is Ellipse t6) Preferences.Default.Set("AutoSave", t6.TranslationX > 0);

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
