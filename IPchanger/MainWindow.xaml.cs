using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IPchanger
{
    /// <summary>
    /// IPアドレス切替器 - メインウィンドウ
    /// </summary>
    public partial class MainWindow : Window
    {
        private MemoWindow? _memoWindow;
        private bool _isShuttingDown;

        public MainWindow()
        {
            InitializeComponent();

            LocationChanged += (s, e) => SyncMemoPosition();
            Loaded += OnWindowLoaded;
            
            InitializeApplication();
        }

        #region 初期化処理

        private void InitializeApplication()
        {
            try
            {
                var s = Properties.Settings.Default;
                Width = s.WindowWidth > 100 ? s.WindowWidth : 850;
                Height = s.WindowHeight > 100 ? s.WindowHeight : 500;

                LanguageManager.Instance.PropertyChanged += (sender, args) =>
                {
                    UpdateCurrentIpDisplay();
                };

                if (s.IsEnglishMode)
                {
                    LanguageManager.Instance.ChangeLanguage("en");
                }
                else
                {
                    LanguageManager.Instance.ChangeLanguage("ja");
                }

                LoadSettings();
                RefreshAdapterList();
                ApplyGwModeVisibility();
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageManager.Instance.GetString("InitErrorFormat", ex.Message), LanguageManager.Instance.GetString("InitErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.IsMemoOpen)
            {
                OpenMemoWindow();
            }
        }

        #endregion

        #region 設定・データ管理

        private void LoadSettings()
        {
            var s = Properties.Settings.Default;
            txtIp1.Text = s.IP1; txtSub1.Text = s.Sub1; txtGw1.Text = s.GW1; txtDns1.Text = s.DNS1;
            txtIp2.Text = s.IP2; txtSub2.Text = s.Sub2; txtGw2.Text = s.GW2; txtDns2.Text = s.DNS2;
            txtIp3.Text = s.IP3; txtSub3.Text = s.Sub3; txtGw3.Text = s.GW3; txtDns3.Text = s.DNS3;
            txtIp4.Text = s.IP4; txtSub4.Text = s.Sub4; txtGw4.Text = s.GW4; txtDns4.Text = s.DNS4;
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            var s = Properties.Settings.Default;
            s.IP1 = txtIp1.Text; s.Sub1 = txtSub1.Text; s.GW1 = txtGw1.Text; s.DNS1 = txtDns1.Text;
            s.IP2 = txtIp2.Text; s.Sub2 = txtSub2.Text; s.GW2 = txtGw2.Text; s.DNS2 = txtDns2.Text;
            s.IP3 = txtIp3.Text; s.Sub3 = txtSub3.Text; s.GW3 = txtGw3.Text; s.DNS3 = txtDns3.Text;
            s.IP4 = txtIp4.Text; s.Sub4 = txtSub4.Text; s.GW4 = txtGw4.Text; s.DNS4 = txtDns4.Text;
            s.Save();
        }

        #endregion

        #region ネットワークアダプター操作

        private void RefreshAdapterList()
        {
            var lastAdapter = Properties.Settings.Default.LastAdapter;
            cmbAdapters.Items.Clear();

            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback && n.OperationalStatus == OperationalStatus.Up)
                .OrderByDescending(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .ThenBy(n => n.Name);

            foreach (var adapter in adapters)
            {
                cmbAdapters.Items.Add(adapter.Name);
            }

            if (cmbAdapters.Items.Count > 0)
            {
                var index = cmbAdapters.Items.IndexOf(lastAdapter);
                cmbAdapters.SelectedIndex = index >= 0 ? index : 0;
            }
        }

        private void cmbAdapters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAdapters.SelectedItem == null) return;
            
            Properties.Settings.Default.LastAdapter = cmbAdapters.SelectedItem.ToString();
            Properties.Settings.Default.Save();
            UpdateCurrentIpDisplay();
        }

        private void UpdateCurrentIpDisplay()
        {
            if (cmbAdapters.SelectedItem == null) return;

            var adapter = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.Name == cmbAdapters.SelectedItem.ToString());

            if (adapter != null)
            {
                var ip = adapter.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                var ipText = ip != null ? ip.Address.ToString() : LanguageManager.Instance.GetString("NotAcquired");
                lblCurrentIp.Text = LanguageManager.Instance.GetString("CurrentIpFormat", ipText);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshAdapterList();
            UpdateCurrentIpDisplay();
        }

        #endregion

        #region IP適用ロジック (netsh)

        private void btnApply1_Click(object sender, RoutedEventArgs e) => ApplySettings(txtIp1.Text, txtSub1.Text, txtGw1.Text, txtDns1.Text, 1);
        private void btnApply2_Click(object sender, RoutedEventArgs e) => ApplySettings(txtIp2.Text, txtSub2.Text, txtGw2.Text, txtDns2.Text, 2);
        private void btnApply3_Click(object sender, RoutedEventArgs e) => ApplySettings(txtIp3.Text, txtSub3.Text, txtGw3.Text, txtDns3.Text, 3);
        private void btnApply4_Click(object sender, RoutedEventArgs e) => ApplySettings(txtIp4.Text, txtSub4.Text, txtGw4.Text, txtDns4.Text, 4);

        private async void ApplySettings(string ip, string sub, string gw, string dns, int patternNo)
        {
            if (cmbAdapters.SelectedItem == null || string.IsNullOrWhiteSpace(ip)) return;

            var adapterName = cmbAdapters.SelectedItem.ToString();
            var isGwMode = chkGwMode.IsChecked ?? false;

            try
            {
                SetStatus(LanguageManager.Instance.GetString("StatusApplying"), Brushes.OrangeRed);

                await Task.Run(() => {
                    var ipArgs = (isGwMode && !string.IsNullOrWhiteSpace(gw))
                        ? $"interface ip set address name=\"{adapterName}\" static {ip} {sub} {gw} 1"
                        : $"interface ip set address name=\"{adapterName}\" static {ip} {sub} none";
                    RunNetsh(ipArgs);

                    var dnsArgs = (isGwMode && !string.IsNullOrWhiteSpace(dns))
                        ? $"interface ip set dns name=\"{adapterName}\" static {dns} primary"
                        : $"interface ip set dns name=\"{adapterName}\" dhcp";
                    RunNetsh(dnsArgs);
                });

                await Task.Delay(1000);
                UpdateCurrentIpDisplay();
                SetStatus(LanguageManager.Instance.GetString("StatusAppliedPattern", patternNo, DateTime.Now.ToString("HH:mm:ss")), Brushes.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageManager.Instance.GetString("ApplyErrorFormat", ex.Message));
                SetStatus(LanguageManager.Instance.GetString("StatusError"), Brushes.Red);
            }
        }

        private async void btnSetDhcp_Click(object sender, RoutedEventArgs e)
        {
            if (cmbAdapters.SelectedItem == null) return;
            var adapterName = cmbAdapters.SelectedItem.ToString();

            try
            {
                SetStatus(LanguageManager.Instance.GetString("StatusSettingDhcp"), Brushes.OrangeRed);
                lblCurrentIp.Text = LanguageManager.Instance.GetString("CurrentIpFormat", LanguageManager.Instance.GetString("UpdatingIp"));

                await Task.Run(() => {
                    RunNetsh($"interface ip set address name=\"{adapterName}\" dhcp");
                    RunNetsh($"interface ip set dns name=\"{adapterName}\" dhcp");
                });

                for (var i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    UpdateCurrentIpDisplay();
                    if (!lblCurrentIp.Text.Contains(LanguageManager.Instance.GetString("NotAcquired")) &&
                        !lblCurrentIp.Text.Contains(LanguageManager.Instance.GetString("UpdatingIp"))) break;
                }
                SetStatus(LanguageManager.Instance.GetString("StatusDhcpCompleted"), Brushes.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex?.Message ?? LanguageManager.Instance.GetString("UnknownError"));
                SetStatus(LanguageManager.Instance.GetString("StatusError"), Brushes.Red);
            }
        }

        private void RunNetsh(string args)
        {
            ProcessStartInfo psi = new("netsh", args)
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var p = Process.Start(psi);
            p?.WaitForExit();
        }

        #endregion

        #region UIレイアウト制御

        private void chkLanguage_Click(object sender, RoutedEventArgs e)
        {
            var isEnglish = chkLanguage.IsChecked ?? false;
            Properties.Settings.Default.IsEnglishMode = isEnglish;
            Properties.Settings.Default.Save();
            LanguageManager.Instance.ChangeLanguage(isEnglish ? "en" : "ja");
        }

        private void chkGwMode_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsGatewayMode = chkGwMode.IsChecked ?? false;
            Properties.Settings.Default.Save();
            ApplyGwModeVisibility();
        }

        private void ApplyGwModeVisibility()
        {
            var isVisible = chkGwMode.IsChecked == true;
            var v = isVisible ? Visibility.Visible : Visibility.Collapsed;
            GridLength gl = new(isVisible ? 1 : 0, GridUnitType.Star);

            colHeaderGw.Width = gl; txtHeaderGw.Visibility = v;
            colHeaderDns.Width = gl; txtHeaderDns.Visibility = v;

            ColumnDefinition[] cols = { colBoxGw1, colBoxDns1, colBoxGw2, colBoxDns2, colBoxGw3, colBoxDns3, colBoxGw4, colBoxDns4 };
            foreach (var col in cols) col.Width = gl;

            TextBox[] boxes = { txtGw1, txtDns1, txtGw2, txtDns2, txtGw3, txtDns3, txtGw4, txtDns4 };
            foreach (var b in boxes) b.Visibility = v;
        }

        private void SetStatus(string text, Brush color)
        {
            txtStatus.Text = text;
            txtStatus.Foreground = color;
        }

        #endregion

        #region メモ帳・ウィンドウ管理

        private void btnMemo_Click(object sender, RoutedEventArgs e)
        {
            if (btnMemo.IsChecked == true) OpenMemoWindow();
            else CloseMemoWindow();
        }

        private void OpenMemoWindow()
        {
            if (_memoWindow != null) return;

            _memoWindow = new();
            _memoWindow.WindowClosedByUI += (s, e) => {
                _memoWindow = null;
                if (!_isShuttingDown)
                {
                    btnMemo.IsChecked = false;
                    SaveMemoState(false);
                }
            };

            _memoWindow.Show();
            SyncMemoPosition();
            SaveMemoState(true);
        }

        private void CloseMemoWindow()
        {
            if (_memoWindow != null)
            {
                _memoWindow.Close();
                _memoWindow = null;
            }
            if (!_isShuttingDown) SaveMemoState(false);
        }

        private void SaveMemoState(bool isOpen)
        {
            Properties.Settings.Default.IsMemoOpen = isOpen;
            Properties.Settings.Default.Save();
        }

        private void SyncMemoPosition()
        {
            if (_memoWindow != null && _memoWindow.IsVisible)
            {
                _memoWindow.Left = Left - _memoWindow.ActualWidth + 7; 
                _memoWindow.Top = Top;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _isShuttingDown = true;
            CloseMemoWindow();

            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.WindowWidth = Width;
                Properties.Settings.Default.WindowHeight = Height;
                Properties.Settings.Default.Save();
            }
        }

        #endregion
    }
}