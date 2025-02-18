using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using Microsoft.Web.WebView2.Core;
using System.Windows.Threading;
using System.Net.Http;

namespace Onbichi_dev.Views
{
    public partial class ContentsManagement : Window
    {
        private DispatcherTimer _timer;
        private string? _lastContentHash;

        public ContentsManagement()
        {
            InitializeComponent();
            this.WindowStyle = WindowStyle.None;
            this.UseLayoutRounding = true; // Layout rounding 사용
            this.KeyDown += Window_KeyDown;
            this.Unloaded += OnUnloaded;
            this.Loaded += OnLoaded; // Loaded 이벤트 핸들러 추가
            InitializeWebView();
            this.SourceInitialized += OnSourceInitialized;

            // WindowChrome 설정
            var chrome = new WindowChrome
            {
                GlassFrameThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(0)
            };
            WindowChrome.SetWindowChrome(this, chrome);

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
            webView.CoreWebView2.Navigate("http://localhost:8080/display");
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
        }

        private async void CheckForUpdates(object? sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetStringAsync("http://localhost:8080/display");
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
    }
}