using System.Windows.Controls;

namespace SystemAukcyjny.Wpf.Views
{
    public partial class RegisterView
    {
        public RegisterView()
        {
            InitializeComponent();
            PasswordBox.PasswordChanged += (s, e) => {
                if (DataContext is ViewModels.RegisterViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                }
            };
        }
    }
}
