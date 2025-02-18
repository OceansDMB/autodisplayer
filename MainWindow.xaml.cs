using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using Microsoft.Web.WebView2.Core;
using System.Windows.Threading;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace Onbichi_dev.Views
{
    public partial class ContentsManagement : Window
    {
        private DispatcherTimer _timer;
        private string? _lastContentHash;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        public ContentsManagement()
        {
            InitializeComponent();
            this.WindowState = WindowState.Normal; // 창 상태를 Normal로 설정
            this.WindowState = WindowState.Maximized; // 창을 최대화면으로 설정
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;
            this.UseLayoutRounding = true; // Layout rounding 사용
            this.KeyDown += Window_KeyDown;
            this.Unloaded += OnUnloaded;
            this.Loaded += OnLoaded; // Loaded 이벤트 핸들러 추가
            InitializeWebView();
            this.SourceInitialized += OnSourceInitialized;

            // DispatcherTimer 설정
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5); // 5초 간격으로 체크
            _timer.Tick += CheckForUpdates;
            _timer.Start();
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
        }

        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Navigate("http://192.168.0.100:8080/display");
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.KeyDown -= Window_KeyDown;
            _timer.Stop();
            _timer.Tick -= CheckForUpdates;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized; // 창을 최대화면으로 설정
            this.Topmost = true; // 창을 항상 위에 표시

            // 작업표시줄까지 덮는 전체 화면 모드 설정
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            // 화면 중앙 클릭 시뮬레이션
            SimulateMouseClick();
        }

        private async void CheckForUpdates(object? sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetStringAsync("http://192.168.0.100:8080/display");
                    var currentContentHash = response.GetHashCode().ToString();

                    if (_lastContentHash != null && _lastContentHash != currentContentHash)
                    {
                        // 내용이 변경되었을 때 새로고침
                        webView.CoreWebView2.Reload();
                    }

                    _lastContentHash = currentContentHash;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking for updates: {ex.Message}");
                }
            }
        }

        private void SimulateMouseClick()
        {
            // 화면 중앙 좌표 계산
            int centerX = (int)(SystemParameters.PrimaryScreenWidth / 2);
            int centerY = (int)(SystemParameters.PrimaryScreenHeight / 2);

            for (int i = 0; i < 3; i++) // 3번 클릭
            {
                SetCursorPos(centerX, centerY);
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                Thread.Sleep(100); // 클릭 간격 100ms
            }

            // 마우스를 화면 밖으로 이동
            SetCursorPos(10000, 10000);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetCursorPos(int x, int y);
    }
}