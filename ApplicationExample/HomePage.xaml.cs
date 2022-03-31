using System;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json;

using GeregeSampleApp;

namespace ApplicationExample
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public struct Welcome
        {
            public static int GeregeMessage() => 101;

            [JsonProperty("title", Required = Required.Always)]
            public string Title { get; set; }
        }

        public HomePage()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadModuleBtn.Visibility = Visibility.Hidden;

            new Thread(GetTitle).Start();
        }

        private void GetTitle()
        {
            string title = "";
            try
            {
                Welcome t = this.UserCacheRequest<Welcome>();
                title = t.Title;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Welcome-ийг авах үед алдаа гарлаа -> " + ex.Message);
                title = ex.Message;
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    TitleBox.Text = title;

                    LoadModuleBtn.Visibility = Visibility.Visible;
                });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SampleApp MyApp = this.App();
                string dllName = "GeregeSampleModule.dll";
                object? partners = MyApp.ModuleStart(
                    MyApp.CurrentDirectory + dllName,
                    new { conclusion = "Loading module is easy and peasy" });

                if (partners is not Page)
                    throw new Exception(dllName + ": Module.Start функц нь Page буцаасангүй!");

                MyApp.RaiseEvent("load-page", partners);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GeregeSampleModule.dll-ийг ачаалах үед алдаа гарлаа -> " + ex.Message);
                ((Button)sender).Content = ex.Message;
            }
        }
    }
}
