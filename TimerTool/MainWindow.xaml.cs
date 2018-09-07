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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimerTool
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        int status = 0; // 0: nothing ; 1: engaged ; 2: running

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
            OnMouseClick();
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
            const uint VK_F10 = 0x79;
            const uint VK_F09 = 0x78;

            if (!RegisterHotKey(helper.Handle, HOTKEY_START_ID, 0, VK_F10))
            {
                // handle error
            }
            if (!RegisterHotKey(helper.Handle, HOTKEY_RESET_ID, 0, VK_F09))
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

        public bool UseMouse { get { return (bool)(UseMouseCheckBox.IsChecked); } }

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
                    if (!UseMouse)
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
            TimerLabel_Seconds.Content = String.Format("{0}", sec);
            TimerLabel_Millis.Content = String.Format("{0:000} sec", ms);

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

            StatusLabel.Content = str;
            InfoLabel_Mouse.Visibility = status == 1 ? Visibility.Visible : Visibility.Hidden;
        }



    }
}
