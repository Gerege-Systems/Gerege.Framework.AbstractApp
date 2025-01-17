﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Reflection;
using GeregeSampleApp;

/////// date: 2022.02.09 //////////
///// author: Narankhuu ///////////
//// contact: codesaur@gmail.com //

namespace WPFAppExample;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly log4net.ILog Log4Net = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public MainWindow()
    {
        InitializeComponent();

        this.App().EventHandler += GeregEventHandler;

        this.AppRaiseEvent("trigger-client-login");
    }

    /// <summary>
    /// Апп дээр идэвхижсэн үзэгдлүүдийг хүлээн авч ажиллуулах.
    /// </summary>
    /// <param name="event">Идэвхжсэн үзэгдэл.</param>
    /// <param name="param">Үзэгдэлд дамжуулагдсан өгөгдөл.</param>
    /// <returns>
    /// Үзэгдэл хүлээн авагчтай бол боловсруулсан үр дүнг object төрлөөр буцаана, үгүй бол null утга буцна.
    /// </returns>
    public object? GeregEventHandler(string @event, object? param = null)
    {
        Debug.WriteLine($"Gerege үзэгдэл дуудагдаж байна => {@event}");

        Log4Net.Info(@event);

        return @event switch
        {
            "trigger-client-login" => OnTriggerClientLogin(),
            "client-login" => OnClientLogin(),
            "load-home" => OnLoadHome(),
            "load-page" => OnLoadPage(param),

            _ => null
        };
    }

    /// <summary>
    /// Апп дээр үндсэн хэрэглэгч нэвтрэхийг шаардах үед энэ функц ажиллана.
    /// </summary>
    public object? OnTriggerClientLogin()
    {
        Dispatcher.Invoke(() =>
        {
            return MainFrame.Navigate(new("clientlogin.xaml", UriKind.Relative));
        });

        return null;
    }

    /// <summary>
    /// Апп дээр үндсэн хэрэглэгч амжилттай нэвтрэх үед энэ функц ажиллана.
    /// </summary>
    public object? OnClientLogin()
    {
        return this.AppRaiseEvent("load-home");
    }

    /// <summary>
    /// Нүүр хуудасруу шилжихийг хүсэх үед энэ функц ажиллана.
    /// </summary>
    public object? OnLoadHome()
    {
        Dispatcher.Invoke(() =>
        {
            return MainFrame.Navigate(new("homepage.xaml", UriKind.Relative));
        });

        return null;
    }

    /// <summary>
    /// Модулиас уншсан Page рүү шилжих.
    /// </summary>
    /// <param name="param">Page обьект.</param>
    public object? OnLoadPage(object? param)
    {
        try
        {
            Dispatcher.Invoke(() =>
            {
                return MainFrame.Navigate(param);
            });
        }
        catch (Exception ex)
        {
            Log4Net.Error(ex);
        }

        return null;
    }
}
