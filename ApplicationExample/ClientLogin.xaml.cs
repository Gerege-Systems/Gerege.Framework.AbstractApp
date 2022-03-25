using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;

using Gerege.Framework.Control;

using GeregeSampleApp;

namespace ApplicationExample
{
    /// <summary>
    /// Interaction logic for TerminalLogin.xaml
    /// </summary>
    public partial class UserLogin : Page
    {
        PleaseWait? Loading;

        /// <summary>Хэрэглэгч нэвтрэх</summary>
        public UserLogin()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string user = Username.Text;
            string pass = Password.Password;

            Loading = new("");
            Worker.Children.Add(Loading);

            LoginBtn.IsEnabled = false;

            new Thread(() => ProceedUserLogin(user, pass)).Start();
        }

        private void ProceedUserLogin(string username, string password)
        {
            string? status = null;
            try
            {
                this.App().UserClient.Login(new { username, password });
                status = "Login success!";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                status = ex.Message;
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    errorTextBlock.Text = status;
                    Worker.Children.Remove(Loading);

                    LoginBtn.IsEnabled = true;
                });
            }
        }
    }
}
