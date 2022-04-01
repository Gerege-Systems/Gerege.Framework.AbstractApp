using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;

using GeregeSampleApp;

namespace ApplicationExample
{
    /// <summary>
    /// Interaction logic for TerminalLogin.xaml
    /// </summary>
    public partial class UserLogin : Page
    {
        /// <summary>Хэрэглэгч нэвтрэх</summary>
        public UserLogin()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string user = Username.Text;
            string pass = Password.Password;

            Username.IsEnabled = false;
            Password.IsEnabled = false;
            LoginBtn.IsEnabled = false;

            new Thread(() => ProceedUserLogin(user, pass)).Start();
        }

        private void ProceedUserLogin(string username, string password)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => ProceedUserLogin(username, password));
                return;
            }

            try
            {
                this.App().UserClient.Login(new { username, password });
                errorTextBlock.Text = "Login success!";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                errorTextBlock.Text = ex.Message;
            }
            finally
            {
                Username.IsEnabled = true;
                Password.IsEnabled = true;
                LoginBtn.IsEnabled = true;
            }
        }
    }
}
