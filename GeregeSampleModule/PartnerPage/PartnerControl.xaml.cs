﻿using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.Json.Serialization;

using GeregeSampleApp;

/////// date: 2022.02.09 //////////
///// author: Narankhuu ///////////
//// contact: codesaur@gmail.com //

namespace GeregeSampleModule.PartnerPage;

public class Partner
{
    [JsonPropertyName("name")]
    [JsonRequired]
    public string Name { get; set; }

    [JsonPropertyName("logo")]
    [JsonRequired]
    public string Logo { get; set; }

    [JsonPropertyName("href")]
    [JsonRequired]
    public string WebAddress { get; set; }
}

public class PartnerList
{
    [JsonPropertyName("partners")]
    [JsonRequired]
    public List<Partner> Data { get; set; }
}

/// <summary>
/// Interaction logic for PartnerControl.xaml
/// </summary>
public partial class PartnerControl : UserControl
{
    public PartnerControl()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        this.AppRaiseEvent("load-home");
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        PartnersPanel.Children.Clear();

        new Thread(GetPartners).Start();
    }

    private void GetPartners()
    {
        if (!CheckAccess())
        {
            Dispatcher.Invoke(() => GetPartners());
            return;
        }

        try
        {
            var list = this.UserCacheRequest<PartnerList>("http://mock-server/get/partners", HttpMethod.Get);
            TitleBox.Text = "Successfully retrieved partners list.";

            foreach (Partner partner in list.Data)
            {
                var border = new Border
                {
                    Tag = partner.WebAddress,
                    Cursor = Cursors.Hand,
                    Margin = new(20, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };
                border.MouseDown += MenuItemClick;

                BitmapImage? logoImg = this.ReadBitmapImage("PartnerPage" + Path.DirectorySeparatorChar + partner.Logo);
                if (logoImg is not null)
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
                    Margin = new(0, 0, 0, -45),
                    FontFamily = new("Montserrat"),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                PartnersPanel.Children.Add(border);
            }
        }
        catch (Exception ex)
        {
            TitleBox.Text = ex.Message;

            Debug.WriteLine($"Error on fetching data -> {ex.Message}");
        }
    }

    private void MenuItemClick(object sender, MouseButtonEventArgs e)
    {
        string? href = Convert.ToString(((FrameworkElement)sender).Tag);

        if (string.IsNullOrEmpty(href)) return;

        Process.Start(new ProcessStartInfo("cmd", $"/c start {href}"));
    }
}
