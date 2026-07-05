using System.Windows;
using System.Windows.Controls;

namespace SystemAukcyjny.Wpf.Views
{
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();
            PasswordBox.PasswordChanged += (s, e) => {
                if (DataContext is ViewModels.LoginViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                }
            };
        }
    }
}
