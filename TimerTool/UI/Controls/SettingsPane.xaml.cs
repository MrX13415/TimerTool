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
    /// Interaktionslogik für SettingsPane.xaml
    /// </summary>
    public partial class SettingsPane : UserControl
    {

        public event EventHandler<EventArgs> StartKeyChange;
        public event EventHandler<EventArgs> ResetKeyChange;
        public event EventHandler<PaneToggleEventArgs> PaneToggle;

        public SettingsPane()
        {
            InitializeComponent();
        }

        public bool MouseTriggerEnabled { get { return (bool)(MouseTriggerCheckBox.IsChecked); } }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Hidden;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Hide();
            PaneToggle?.Invoke(this, new PaneToggleEventArgs(Visibility));
        }
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Show();
            PaneToggle?.Invoke(this, new PaneToggleEventArgs(Visibility));
        }

        private void StartButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            StartKeyChange?.Invoke(sender, e);
        }

        private void ResetButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ResetKeyChange?.Invoke(sender, e);
        }

    }
}
