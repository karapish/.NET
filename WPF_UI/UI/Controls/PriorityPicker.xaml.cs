
namespace TellusResourceAllocatorManagement.UI
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for PriorityPicker.xaml
    /// </summary>
    public partial class PriorityPicker : Window
    {
        public PriorityPicker()
        {
            InitializeComponent();
        }

        private void PrioritySelectorButtonOK_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Why the IsDefault=True in XAML is not working and I have to do this?
            this.DialogResult = true;
            this.Close();
        }

        private void PrioritySelectorButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Why the IsDefault=True in XAML is not working and I have to do this?
            this.DialogResult = false;
            this.Close();
        }
    }
}
