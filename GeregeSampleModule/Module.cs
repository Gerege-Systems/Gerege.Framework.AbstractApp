using System;
using System.Windows;
using System.Diagnostics;
using GeregeSampleModule.PartnerPage;

/////// date: 2022.02.09 //////////
///// author: Narankhuu ///////////
//// contact: codesaur@gmail.com //

/// <summary>
/// Модуль үйлчилгээний эх класс.
/// <para>
/// </para>
/// Gerege application нь Модуль үйлчилгээг ажиллуулах зарчим нь бол
/// Module dll-ийг санах ойд ачаалаад Assembly дотроос ямар нэг namespace дотор агуулагдаагүй
/// ил байгаа Module классын Start функцыг дуудсанаар FrameworkElement аваад ашигладаг.
/// Тийм учраас Module классыг namespace-гүйгээр ил зарлаж байна.
/// </summary>
#pragma warning disable CA1050
public class Module
{
    /// <summary>
    /// Модулийн эхлэл функц.
    /// Хост апп дээрээс дуудагдана.
    /// </summary>
    /// <param name="param">Хост апп-аас илгээсэн параметр.</param>
    /// <returns>
    /// Гэрэгийн томоохон харилцагчдын мэдээлэл агуулах хуудас.
    /// </returns>
    public FrameworkElement Start(dynamic param)
    {
        string log = Convert.ToString(param);
        Debug.WriteLine("Module.Start param -> " + log);

        return new PartnerControl();
    }
}
#pragma warning restore CA1050
