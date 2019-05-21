using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimerTool.Windows;
using static TimerTool.Windows.MouseHook;

namespace TimerTool.UI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        struct KeyInfo
        {
            public string KeyName { get; set; }
            //See: https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes
            public int VK_Code { get; set; }

            public KeyInfo(int virtualKey)
            {
                KeyName = System.Windows.Input.KeyInterop.KeyFromVirtualKey(virtualKey).ToString();
                VK_Code = virtualKey;
            }

            public KeyInfo(System.Windows.Input.Key key)
            {
                KeyName = key.ToString();
                VK_Code = System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);
            }
        }

        int status = 0; // 0: nothing ; 1: engaged ; 2: running

        int captureKey = 0; // 0: no capture ; 1: start key ; 2: reset key
        KeyInfo key_Start = new KeyInfo(0x79);  // F10 (VK_F10)
        KeyInfo key_Reset = new KeyInfo(0x78);  // F9 (VK_F9)

        Stopwatch timer = new Stopwatch();
        BackgroundWorker uiworker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MouseHook.Start();
            MouseHook.MouseAction += new EventHandler(MouseEvent);

            uiworker = new BackgroundWorker();
            uiworker.DoWork += new DoWorkEventHandler(uiworker_init);
            uiworker.ProgressChanged += new ProgressChangedEventHandler(uiworker_onupdate);
            uiworker.WorkerReportsProgress = true;

            Update();
        }

        private void MouseEvent(object sender, EventArgs e)
        {
            switch ((e as MouseEventArgs).MouseMessage)
            {
                case MouseMessages.ButtonLeftDown:
                case MouseMessages.ButtonLeftUp:
                case MouseMessages.ButtonRightDown:
                case MouseMessages.ButtonRightUp:
                    OnMouseClick();
                    break;
                case MouseMessages.MouseMove:
                case MouseMessages.MouseWheel:
                    break;
            }
        }

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private const int HOTKEY_START_ID = 9001;
        private const int HOTKEY_RESET_ID = 9002;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            uint startKey = (uint)key_Start.VK_Code;
            uint resetKey = (uint)key_Reset.VK_Code;

            if (!RegisterHotKey(helper.Handle, HOTKEY_START_ID, 0, startKey))
            {
                // handle error
            }
            if (!RegisterHotKey(helper.Handle, HOTKEY_RESET_ID, 0, resetKey))
            {
                // handle error
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_START_ID);
            UnregisterHotKey(helper.Handle, HOTKEY_RESET_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_START_ID:
                            OnStartKeyPressed();
                            handled = true;
                            break;
                        case HOTKEY_RESET_ID:
                            OnResetKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }


        private void OnMouseClick()
        {
            if (status == 1)
            {
                timer.Start();
                status = 2;
            }
        }

        private void OnStartKeyPressed()
        {
            switch (status)
            {
                default:
                case 0:
                    status = 1; 
                    if (!SettingsPane.MouseTriggerEnabled)
                    {
                        timer.Start();
                        status = 2; break;
                    }
                    break;
                case 3: //pause
                case 1: //engaged
                    timer.Start();
                    status = 2; break;
                case 2: //running
                    timer.Stop();
                    status = 3; break;
            }
        }

        private void OnResetKeyPressed()
        {
            timer.Stop();
            timer.Reset();
            status = 0;
        }

        public void Update()
        {
            uiworker.RunWorkerAsync();
        }

        public void uiworker_init(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            while (!worker.CancellationPending)
            {
                Thread.Sleep(16);
                uiworker.ReportProgress(0, "");
            }
        }

        // This event handler updates the UI
        private void uiworker_onupdate(object sender, ProgressChangedEventArgs e)
        {
            int sec = (int)(timer.ElapsedMilliseconds / 1000);
            double ms = timer.ElapsedMilliseconds - (sec * 1000);
            TimerPane.TimerLabel_Seconds.Content = String.Format("{0}", sec);
            TimerPane.TimerLabel_Millis.Content = String.Format("{0:000} sec", ms);

            string str;
            switch (status)
            {
                default:
                case 0:
                    str = "⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯"; break;
                case 1:
                    str = "E N G A G E D"; break;
                case 2:
                    str = "R U N N I N G"; break;
                case 3:
                    str = "P A U S E D"; break;
            }

            TimerPane.StatusLabel.Content = str;
            ControlPane.InfoLabel_Mouse.Visibility = status == 1 ? Visibility.Visible : Visibility.Hidden;

            if (captureKey == 0)
            {
                if (ControlPane.IsVisible)
                {
                    UpdateKeyBtn(ControlPane.StartBtn, key_Start.KeyName);
                    UpdateKeyBtn(ControlPane.ResetBtn, key_Reset.KeyName);
                }
                if (SettingsPane.IsVisible)
                {
                    UpdateKeyBtn(SettingsPane.StartButton, key_Start.KeyName);
                    UpdateKeyBtn(SettingsPane.ResetButton, key_Reset.KeyName);
                }
            }
        }

        private void UpdateKeyBtn(Label control, string key)
        {
            string text = (string)control.Content;
            if (text.Length == 0) return;
            if (!text.Contains("|")) return;
            control.Content = key + " " + text.Substring(text.IndexOf("|"));
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (captureKey == 0) return;

            KeyInfo key = new KeyInfo(e.Key);

            if (captureKey == 1) key_Start = key;           
            if (captureKey == 2) key_Reset = key;
        
            UnregisterHotKey();
            RegisterHotKey();

            captureKey = 0;
        }

        private void ControlPane_StartButtonClick(object sender, EventArgs e)
        {
            OnStartKeyPressed();
        }

        private void ControlPane_ResetButtonClick(object sender, EventArgs e)
        {
            OnResetKeyPressed();
        }

        private void SettingsPane_StartKeyChange(object sender, EventArgs e)
        {
            if (captureKey == 1)
            {
                captureKey = 0;
                return;
            }
            if (captureKey != 0) return;
            captureKey = 1;
            UpdateKeyBtn(SettingsPane.StartButton, "\u2423"); // OpenBox char
        }

        private void SettingsPane_ResetKeyChange(object sender, EventArgs e)
        {
            if (captureKey == 2)
            {
                captureKey = 0;
                return;
            }
            if (captureKey != 0) return;
            captureKey = 2;
            UpdateKeyBtn(SettingsPane.ResetButton , "\u2423"); // OpenBox char
        }

        private void ControlPane_PaneToggle(object sender, Controls.PaneToggleEventArgs e)
        {
            if (e.Visibility == Visibility.Hidden)
            {
                SettingsPane.Show();
            }
        }

        private void SettingsPane_PaneToggle(object sender, Controls.PaneToggleEventArgs e)
        {
            if (e.Visibility == Visibility.Hidden)
            {
                ControlPane.Show();
            }
        }
    }
}
