using System.Collections.Generic;
using System.Windows;

namespace Dash_boardsBIDA
{
    public partial class LoginWindow : Window
    {
        // 5 tài khoản demo (email -> password)
        private readonly Dictionary<string, string> _accounts = new Dictionary<string, string>()
        {
            {"admin1@gmail.com","123"},
            {"admin2@gmail.com","123"},
            {"staff1@gmail.com","123"},
            {"staff2@gmail.com","123"},
            {"cashier@gmail.com","123"},
        };

        public LoginWindow()
        {
            InitializeComponent();
            TxtEmail.Text = "admin1@gmail.com"; // gợi ý cho nhanh
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim().ToLower();
            string pass = TxtPass.Password;

            if (_accounts.ContainsKey(email) && _accounts[email] == pass)
            {
                MainWindow mw = new MainWindow(email);
                mw.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Sai email hoặc mật khẩu! (password demo: 123)");
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}