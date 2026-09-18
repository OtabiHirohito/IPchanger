using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IPchanger
{
    /// <summary>
    /// Ping / ipconfig ツールウィンドウ
    /// </summary>
    public partial class PingWindow : Window
    {
        public event EventHandler? WindowClosedByUI;
        private bool _isRunning;

        [DllImport("kernel32.dll")]
        private static extern uint GetConsoleOutputCP();

        [DllImport("kernel32.dll")]
        private static extern uint GetOEMCP();

        static PingWindow()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
            catch
            {
                // Ignore if already registered or unsupported
            }
        }

        public PingWindow()
        {
            InitializeComponent();
            Closing += PingWindow_Closing;
        }

        private static Encoding GetCommandEncoding()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    uint cp = GetConsoleOutputCP();
                    if (cp == 0) cp = GetOEMCP();
                    if (cp > 0)
                    {
                        return Encoding.GetEncoding((int)cp);
                    }
                }
            }
            catch
            {
                // Fallback if P/Invoke fails
            }

            try
            {
                int oemCp = CultureInfo.CurrentCulture.TextInfo.OEMCodePage;
                if (oemCp > 0)
                {
                    return Encoding.GetEncoding(oemCp);
                }
            }
            catch
            {
                // Fallback
            }

            return Encoding.Default;
        }

        private async void btnPing_Click(object sender, RoutedEventArgs e)
        {
            var targetIp = txtTargetIp.Text?.Trim();
            if (string.IsNullOrWhiteSpace(targetIp))
            {
                txtResult.Text = LanguageManager.Instance.GetString("IpRequired");
                return;
            }

            await RunCommandAsync("ping", targetIp);
        }

        private async void btnIpconfig_Click(object sender, RoutedEventArgs e)
        {
            await RunCommandAsync("ipconfig", "");
        }

        private async Task RunCommandAsync(string command, string arguments)
        {
            if (_isRunning) return;

            _isRunning = true;
            btnPing.IsEnabled = false;
            btnIpconfig.IsEnabled = false;
            txtResult.Text = LanguageManager.Instance.GetString("Executing") + Environment.NewLine + Environment.NewLine;

            try
            {
                var commandEncoding = GetCommandEncoding();
                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = commandEncoding,
                    StandardErrorEncoding = commandEncoding
                };

                using var process = new Process { StartInfo = psi };

                process.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            txtResult.AppendText(e.Data + Environment.NewLine);
                            txtResult.ScrollToEnd();
                        });
                    }
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            txtResult.AppendText(e.Data + Environment.NewLine);
                            txtResult.ScrollToEnd();
                        });
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();
            }
            catch (Exception ex)
            {
                txtResult.AppendText(Environment.NewLine + $"Error: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
                btnPing.IsEnabled = true;
                btnIpconfig.IsEnabled = true;
            }
        }

        private void PingWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.PingWidth = Width;
            Properties.Settings.Default.PingHeight = Height;
            Properties.Settings.Default.Save();

            WindowClosedByUI?.Invoke(this, EventArgs.Empty);
        }
    }
}
