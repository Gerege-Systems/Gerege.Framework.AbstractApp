﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/////// date: 2022.01.22 //////////
///// author: Narankhuu ///////////
//// contact: codesaur@gmail.com //

namespace Gerege.Framework.WPFApp;

/// <summary>
/// Гэрэгэ логикоор ажиллах програм хангамжын үндсэн суурь апп хийсвэр класс.
/// </summary>
public abstract partial class GeregeWPFApp : Application
{
    /// <summary>Апп процесс оршиж/ажиллаж буй хавтас зам.</summary>
    public string CurrentDirectory;

    /// <summary>Гэрэгэ үзэгдэл хүлээн авагч төрөл зарлах.</summary>
    /// <param name="name">Гэрэгэ үзэгдэл нэр.</param>
    /// <param name="args">Үзэгдэлд дамжуулах өгөгдөл параметр.</param>
    /// <returns>Үзэгдэл хүлээн авагчаас үр дүн эсвэл null буцаана.</returns>
    public delegate object? GeregeEventHandler(string name, object? args = null);

    /// <summary>Gerege үзэгдэл хүлээн авагч.</summary>
    public event GeregeEventHandler EventHandler;

    /// <summary>Гэрэгэ апп үүсгэгч.</summary>
    public GeregeWPFApp()
    {        
        // App идэвхитэй ажиллаж байгаа хавтас олоx
        DirectoryInfo? currentDir = null;
        if (Environment.ProcessPath != null)
            currentDir = Directory.GetParent(Environment.ProcessPath);
        currentDir ??= new DirectoryInfo(Environment.CurrentDirectory);
        CurrentDirectory = currentDir.FullName + Path.DirectorySeparatorChar;
    }

    /// <summary>Гэрэгэ үзэгдлийг идэвхжүүлэх.</summary>
    /// <param name="name">Идэвхжүүлэх үзэгдэл нэр.</param>
    /// <param name="args">Үзэгдэлд дамжуулагдах өгөгдөл.</param>
    /// <returns>
    /// Үзэгдэл хүлээн авагчаас үр дүн ирвэл object төрлөөр буцаана.
    /// Ямар нэгэн алдаа гарч Exception үүссэн бол үзэгдлийн үр дүнд алдааны мэдээллийг олгоно.
    /// <para>Үзэгдэл хүлээн авагчаас үр дүн null байх боломжтой.</para>
    /// </returns>
    public virtual object? RaiseEvent(string name, object? args = null)
    {
        // боломжит үзэгдэл хүлээн авагчдийг цуглуулж байна
        Delegate[]? delegates = EventHandler?.GetInvocationList();

        // боломжит үзэгдэл хүлээн авагчид байхгүй бол null утга буцаая
        if (delegates is null) return null;

        // хүлээн авагчидаас боловсруулсан үр дүнг энэ жагсаалтад бүртгэе
        List<object?> results = [];

        // үзэгдэл хүлээн авагч бүрийг ажиллуулж байна
        foreach (Delegate d in delegates)
        {
            try
            {
                results.Add(d.DynamicInvoke(name, args));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // амжилттай гүйцэтгэсэн эхний үр дүн эсвэл null буцаая
        return results.FirstOrDefault(r => r != null);
    }

    /// <summary>
    /// Гэрэгэ клиент апп нь Гэрэгэ хэрэглэгч эрхийн дагуу нэвтэрч ажиллах ерөнхий зохион байгуулалттай.
    /// <para>
    /// Үүнд GeregeWPFApp нь анх амжилттай ачаалагдах үедээ энэ хийсвэр функцыг дуудан
    /// HTTP клиент обьект болон лог бичих обьект гэх мэт шаардлагатай бүрэлдэхүүн хэсгээ үүсгэдэг байх ёстой.
    /// </para>
    /// <para>
    /// Хөгжүүлэгч нь заавал энэ функцийг удамшуулсан класс дээрээ override түлхүүр үгээр
    /// дахин функц болгон тодорхойлж тухайн aппд шаардлагатай бүрэлдэхүүн обьектуудыг үүсгэнэ.
    /// </para>
    /// </summary>
    protected abstract void CreateComponents();

    private Mutex _instanceMutex;

    /// <summary>
    /// Апп анх ачаалагдах үед биелэх үзэгдлийн виртуал функц.
    /// Анхны код нь аппыг цор ганц хувилбараар ажиллуулах гол зорилготой.
    /// <para>
    /// Хөгжүүлэгч нь шаардлагатай бол энэ функцийг удамшуулсан класс дээрээ
    /// override түлхүүр үгээр дахин функц болгон тодорхойлж үйлдэл логик оруулж/нэмж гүйцэтгэж болно.
    /// </para>
    /// </summary>
    /// <param name="e">Ачаалагдсан процессын утгууд.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Mutex үүссэн эсэхийг шалгая
        _instanceMutex = new(true, @"Global\ControlPanel", out bool createdNew);

        if (createdNew)
        {
            // Mutex шинээр үүсэж байгаа гэдэг нь Апп процесс өмнө нь үүсээгүй гэсэн үг.
            // Тийм учраас анх ачаалж байна гэж үзээд эхлэлийн үйлдлүүдийг энэ блок дотор хийж болно.

            // Шаардлагатай бүрэлдэхүүн хэсэг үүсгэх хийсвэр функцыг ажиллууллая
            CreateComponents();

            return;
        }

        // Апп өмнө ачаалагдсан байгаа учир процессийг олж идэвхжүүлэн
        // аппын үндсэн цонхыг олж сэргээгээд хамгийн дээр харуулъяа
        try
        {
            // Апп үндсэн процессыг авна
            Process currentProcess = Process.GetCurrentProcess();
            Process? runningProcess = (from process in Process.GetProcesses()
                                  where
                                    process.Id != currentProcess.Id &&
                                    process.ProcessName.Equals(
                                      currentProcess.ProcessName,
                                      StringComparison.Ordinal)
                                  select process).FirstOrDefault();
            if (runningProcess is not null)
            {
                // Процесс нь үндсэн цонхтой байна уу?
                if (runningProcess.MainWindowHandle != IntPtr.Zero)
                {
                    // Үндсэн цонх нь харагдахгүй далд байвал сэргээе
                    if (IsIconic(runningProcess.MainWindowHandle))
                        _ = ShowWindow(runningProcess.MainWindowHandle, SW_SHOWNOACTIVATE);

                    // Үндсэн цонхыг хамгийн дээр идэвхжүүлж харуулъяа
                    SetForegroundWindow(runningProcess.MainWindowHandle);
                }
                else
                {
                    // TODO: Үндсэн цонхгүй процесс байх үед ер яах билээ
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            // Аппын шинээр ачаалахыг хүссэн нэмэлт процессыг унтраая
            Current.Shutdown();
        }
    }

    /// <summary>
    /// Апп унтрах үед биелэх үзэгдлийн виртуал функц.
    /// </summary>
    /// <param name="e">Унтрах процессын утгууд.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        _instanceMutex.ReleaseMutex();

        base.OnExit(e);
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsIconic(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    private static partial int ShowWindow(IntPtr hWnd, uint Msg);

    private const int SW_SHOWNOACTIVATE = 4;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);
}
