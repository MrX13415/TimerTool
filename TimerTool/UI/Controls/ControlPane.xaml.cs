using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimerTool.UI.Controls
{
    /// <summary>
    /// Interaktionslogik für ControlPane.xaml
    /// </summary>
    public partial class ControlPane : UserControl
    {
        public event EventHandler<EventArgs> StartButtonClick;
        public event EventHandler<EventArgs> ResetButtonClick;
        public event EventHandler<PaneToggleEventArgs> PaneToggle;

        public ControlPane()
        {
            InitializeComponent();
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Hidden;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Hide();
            PaneToggle?.Invoke(this, new PaneToggleEventArgs(Visibility));
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Show();
            PaneToggle?.Invoke(this, new PaneToggleEventArgs(Visibility));
        }

        private void StartBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StartButtonClick.Invoke(sender, e);
        }

        private void ResetBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetButtonClick.Invoke(sender, e);
        }

    }

}
