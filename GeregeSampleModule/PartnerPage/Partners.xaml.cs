using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

using Newtonsoft.Json;

using GeregeSampleApp;

namespace GeregeSampleModule.PartnerPage
{
    public class Partner
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("logo", Required = Required.Always)]
        public string Logo { get; set; }

        [JsonProperty("href", Required = Required.Always)]
        public string WebAddress { get; set; }
    }

    public class PartnerList
    {
        public virtual int GeregeMessage() => 102;

        [JsonProperty("partners", Required = Required.Always)]
        public List<Partner> Data { get; set; }
    }

    /// <summary>
    /// Interaction logic for Partners.xaml
    /// </summary>
    public partial class Partners : Page
    {
        public Partners()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.AppRaiseEvent("load-home");
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            new Thread(GetPartners).Start();
        }

        private void GetPartners()
        {
            string title = "";
            PartnerList partnerList = new();
            try
            {
                partnerList = this.UserRequest<PartnerList>();
                title = "Successfully retrieved partners list.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error on fetching data -> " + ex);
                title = ex.Message;
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    TitleBox.Text = title;

                    PartnersPanel.Children.Clear();

                    if (partnerList == null || partnerList.Data == null)
                        return;

                    foreach (Partner partner in partnerList.Data)
                    {
                        Border border = new Border
                        {
                            Tag = partner.WebAddress,
                            Cursor = Cursors.Hand,
                            Margin = new Thickness(20, 0, 0, 0),
                            VerticalAlignment = VerticalAlignment.Top
                        };
                        border.MouseDown += MenuItemClick;

                        BitmapImage? logoImg = this.ReadBitmapImage("PartnerPage" + Path.DirectorySeparatorChar + partner.Logo);
                        if (logoImg != null)
                            border.Background = new ImageBrush { ImageSource = logoImg };
                        else
                            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0072fe"));

                        border.Width = 127;
                        border.Height = 127;

                        border.Child = new TextBlock
                        {
                            Height = 40,
                            FontSize = 14,
                            Text = partner.Name,
                            Foreground = Brushes.Black,
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, 0, -45),
                            FontFamily = new FontFamily("Montserrat"),
                            VerticalAlignment = VerticalAlignment.Bottom,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };

                        PartnersPanel.Children.Add(border);
                    }
                });
            }
        }

        private void MenuItemClick(object sender, MouseButtonEventArgs e)
        {
            string? href = Convert.ToString(((FrameworkElement)sender).Tag);

            if (string.IsNullOrEmpty(href)) return;

            Process.Start(new ProcessStartInfo("cmd", $"/c start {href}"));
        }
    }
}
