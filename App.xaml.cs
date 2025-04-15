using System;
using System.IO;
using System.Security.Principal;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace autodisplayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // 시작 프로그램 등록 여부만 확인하고 등록
                // 관리자 권한 요청 코드 제거
                if (!IsRegisteredInStartup())
                {
                    // HKEY_CURRENT_USER 아래에 등록하면 관리자 권한 필요 없음
                    RegisterInStartup();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"프로그램 시작 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 시작 프로그램에 이미 등록되어 있는지 확인하는 메서드
        private bool IsRegisteredInStartup()
        {
            try
            {
                string appName = "autodisplayer";
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);

                return registryKey?.GetValue(appName) != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"시작 프로그램 확인 중 오류가 발생했습니다: {ex.Message}",
                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // 시작 프로그램에 등록하는 메서드
        private void RegisterInStartup()
        {
            try
            {
                // HKEY_CURRENT_USER에는 일반 사용자 권한으로도 쓰기 가능
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                string appName = "autodisplayer";

                // 현재 실행 중인 애플리케이션의 경로
                string appPath = Process.GetCurrentProcess().MainModule.FileName;
                registryKey.SetValue(appName, appPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"시작 프로그램 등록 중 오류가 발생했습니다: {ex.Message}",
                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}