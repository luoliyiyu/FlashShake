using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace FlashShake
{
    public partial class App : Application
    {
        // Variables used for engine shake counting and timing (用于引擎统计次数和计时)
        private int _shakeCount = 0;
        private DateTime _lastShakeTime = DateTime.MinValue;
        private System.Timers.Timer _waitTimer;
        private bool _isFlashOn = false;

        public App()
        {
            InitializeComponent();

            // Note: If you are not using AppShell, change this to new MainPage(); (注意：如果你没有用 AppShell，这里请改为 new MainPage();)
            MainPage = new AppShell();

            // Start the global shake listening engine (启动摇一摇全局监听引擎)
            InitShakeEngine();
        }

        private void InitShakeEngine()
        {
            if (Accelerometer.Default.IsSupported)
            {
                // Initialize the settlement timer: if there is no new shake action within 1.2 seconds, calculate the total shakes (初始化结算计时器：如果 1.2 秒内没有新的摇晃动作，就去结算刚刚一共摇了几次)
                _waitTimer = new System.Timers.Timer(1200);
                _waitTimer.AutoReset = false;
                _waitTimer.Elapsed += (s, e) => ProcessShakeResult();

                // Subscribe to sensor changes and start (订阅传感器变化并启动)
                Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            // Get the sensitivity set by the user in ShakePage and convert it to the underlying G-force threshold (获取用户在 ShakePage 设置的灵敏度，转换为对应的底层 G力 阈值)
            string sensitivity = Preferences.Default.Get("ShakeSensitivity", "Medium");
            double threshold = sensitivity switch
            {
                "High" => 1.4, // High sensitivity means easily triggered, so the threshold is low (High 灵敏度代表轻易触发，所以阈值低)
                "Low" => 2.8,  // Low sensitivity means strong force is needed, so the threshold is high (Low 灵敏度代表需要用力，所以阈值高)
                _ => 2.0       // Medium default (Medium 默认)
            };

            // Calculate the total G-force of the current three-axis acceleration (计算当前三轴加速度总和的 G 力)
            var acc = e.Reading.Acceleration;
            double gForce = Math.Sqrt(acc.X * acc.X + acc.Y * acc.Y + acc.Z * acc.Z);

            if (gForce > threshold)
            {
                DateTime now = DateTime.Now;

                // Debounce design: at least 250 milliseconds between valid shakes to prevent one swing from being counted multiple times (防抖设计：每次有效摇动之间至少间隔 250 毫秒，避免甩一下被算作好几次)
                if ((now - _lastShakeTime).TotalMilliseconds > 250)
                {
                    _shakeCount++;
                    _lastShakeTime = now;

                    // Reset the settlement timer (重置结算计时器)
                    _waitTimer.Stop();
                    _waitTimer.Start();
                }
            }
        }

        // Process settlement logic (处理结算逻辑)
        private async void ProcessShakeResult()
        {
            int count = _shakeCount;

            // Reset the counter to prepare for the next shake (重置计数器，为下次摇晃做准备)
            _shakeCount = 0;

            // Read the corresponding times set by the user (读取用户设定的对应次数)
            int targetNormal = Preferences.Default.Get("NormalShakeCount", 2);
            int targetSOS = Preferences.Default.Get("SOSShakeCount", 3);

            // Determine the execution mode (判定执行模式)
            if (count == targetSOS)
            {
                await RunSOSPattern();
            }
            else if (count == targetNormal)
            {
                await ToggleFlashlight();
            }
        }

        // Normal mode: toggle flashlight (Normal 模式：开关闪光灯)
        private async Task ToggleFlashlight()
        {
            try
            {
                if (_isFlashOn)
                    await Flashlight.Default.TurnOffAsync();
                else
                    await Flashlight.Default.TurnOnAsync();

                _isFlashOn = !_isFlashOn;
            }
            catch { /* Catch exceptions like emulator not supporting flashlight (捕捉模拟器不支持或其他异常) */ }
        }

        // SOS mode: simple flashing (SOS 模式：简单闪烁)
        private async Task RunSOSPattern()
        {
            try
            {
                // Simulate SOS flashing logic (flash three times) (模拟SOS闪烁逻辑（闪烁三次）)
                for (int i = 0; i < 3; i++)
                {
                    await Flashlight.Default.TurnOnAsync();
                    await Task.Delay(200);
                    await Flashlight.Default.TurnOffAsync();
                    await Task.Delay(200);
                }
                _isFlashOn = false;
            }
            catch { }
        }
    }
}
