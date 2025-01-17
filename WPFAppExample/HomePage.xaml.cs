﻿using System;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json.Serialization;

using GeregeSampleApp;

/////// date: 2022.02.09 //////////
///// author: Narankhuu ///////////
//// contact: codesaur@gmail.com //

namespace WPFAppExample;

/// <summary>
/// Interaction logic for HomePage.xaml
/// </summary>
public partial class HomePage : Page
{
    private static readonly log4net.ILog Log4Net = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public struct Welcome
    {
        [JsonPropertyName("title")]
        [JsonRequired]
        public string Title { get; set; }
    }

    public HomePage()
    {
        InitializeComponent();
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        new Thread(GetTitle).Start();

        LoadModuleBtn.Visibility = Visibility.Hidden;
    }

    private void GetTitle()
    {
        if (!CheckAccess())
        {
            Dispatcher.Invoke(() => GetTitle());
            return;
        }

        try
        {
            Welcome t = this.UserCacheRequest<Welcome>("http://mock-server/get/title", HttpMethod.Get);
            TitleBox.Text = t.Title;
        }
        catch (Exception ex)
        {
            TitleBox.Text = ex.Message;

            string logMsg = "Welcome-ийг авах үед алдаа гарлаа";
            Log4Net.Error(logMsg, ex);
            Debug.WriteLine($"{logMsg} -> {ex.Message}");
        }
        finally
        {
            LoadModuleBtn.Visibility = Visibility.Visible;
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
            MyApp.RaiseEvent("load-page", partners);
        }
        catch (Exception ex)
        {
            ((Button)sender).Content = ex.Message;

            string logMsg = "Sample Module-ийг ачаалах үед алдаа гарлаа";
            Log4Net.Error(logMsg, ex);
            Debug.WriteLine($"{logMsg} -> {ex.Message}");
        }
    }
}
