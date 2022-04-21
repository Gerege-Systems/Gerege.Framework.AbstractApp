using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;

#pragma warning disable IDE0060
namespace GeregeSampleApp
{
    /// <author>
    /// codesaur - 2022.02.09
    /// </author>
    /// <project>
    /// Gerege Application Development Framework V5
    /// </project>

    /// <summary>
    /// Гэрэгэ логикоор ажиллах програм хангамжын үндсэн суурь аппын өргөтгөл.
    /// <para>
    /// .NET - ийн хамгийн эхний суурь object классыг өргөтгөн дурын объект код дотор
    /// одоо идэвхитэй ачаалагдсан Гэрэгэ апп объектыг дуудан ашиглах боломжтой болгоно.
    /// </para>
    /// </summary>
    public static class SampleAppExtension
    {
        /// <summary>
        /// Одоо идэвхитэй ачаалагдсан Гэрэгэ апп объектыг дуудан ашиглах боломжтой болгоно.
        /// <remarks>
        /// <code>
        /// // code sample
        /// App MyApp = this.App();
        /// Page partners = (Page)MyApp.LoadModule(
        ///     MyApp.CurrentDirectory + "GeregeSampleModule.dll",
        ///     new { conclusion = "Loading module is easy and peasy" });
        /// MyApp.RaiseEvent("load-page", partners);
        /// 
        /// // or one-liner
        /// this.App().UserClient.Login(new { username, password });
        /// </code>
        /// </remarks>
        /// </summary>
        /// <param name="a">Өргөтгөлийг ашиглаж буй объект.</param>
        /// <returns>
        /// GeregeWPFApp-аас удамшсан ачаалагдсан апп SampleApp объектыг буцаана.
        /// </returns>
        public static SampleApp App(this object a)
        {
            return Application.Current as SampleApp;
        }

        /// <summary>
        /// Ачаалагдсан Гэрэгэ апп дээр Гэрэгэ үзэгдлийг идэвхжүүлэх.
        /// <remarks>
        /// <code>
        /// this.AppRaiseEvent("some-event", new { param = "value" });
        /// </code>
        /// </remarks>
        /// </summary>
        /// <param name="a">Өргөтгөлийг ашиглаж буй объект.</param>
        /// <param name="name">Идэвхжүүлэх үзэгдэл нэр.</param>
        /// <param name="args">Үзэгдэлд дамжуулагдах өгөгдөл.</param>
        /// <returns>
        /// Үзэгдэл хүлээн авагчаас үр дүн ирвэл dynamic төрлөөр буцаана, үгүй бол null утга буцаана.
        /// <para>Үзэгдэл хүлээн авагчаас үр дүн null буцаасан байх боломжтой.</para>
        /// </returns>
        public static dynamic AppRaiseEvent(this object a, string name, dynamic args = null)
        {
            return a.App().RaiseEvent(name, args);
        }

        /// <summary>
        /// Ачаалагдсан Гэрэгэ аппын хэрэглэгчийн клиентээр HTTP хүсэлт үүсгэж илгээн мэдээлэл хүлээж авах.
        /// Зөв зарлагдсан T темплейт класс/бүтцээс Гэрэгэ мессеж дугаарыг авч хүсэлтийн толгойн message_code талбарт онооно.
        /// <para>
        /// T темплейт бүтэц/класс буруу зарлагдсан, хүсэлтийн параметрууд буруу өгөгдсөн, холболт тасарсан, серверээс хариу ирээгүй, ирсэн хариуны формат зөрсөн
        /// гэх мэтчилэн болон өөр бусад шалтгаануудын улмаас Exception алдаа үүсэх боломжтой тул заавал try {} catch (Exception) {} код блок дунд ашиглана.
        /// </para>
        /// </summary>
        /// <param name="a">Өргөтгөлийг ашиглаж буй объект.</param>
        /// <param name="payload">Хүсэлтийн бие.</param>
        /// <param name="method">Хүсэлтийн дүрэм. Анхны утга null үед POST дүрэм гэж үзнэ.</param>
        /// <param name="requestUri">Хүсэлт илгээх хаяг.</param>
        /// <exception cref="Exception">
        /// T темплейт бүтэц/класс буруу зарлагдсан, хүсэлтийн параметрууд буруу өгөгдсөн, холболт тасарсан, серверээс хариу ирээгүй,
        /// ирсэн хариуны формат зөрсөн гэх мэтчилэн алдаануудын улмаас Exception үүсгэж шалтгааныг мэдэгдэнэ.
        /// </exception>
        /// <returns>
        /// Серверээс ирсэн хариуг амжилттай авч тухайн зарласан T темплейт класс обьектэд хөрвүүлсэн утгыг буцаана.
        /// </returns>
        public static T UserRequest<T>(this object a, dynamic payload = null, HttpMethod method = null, string requestUri = null)
        {
            return a.App().UserClient.Request<T>(payload, method, requestUri);
        }

        /// <summary>
        /// Ачаалагдсан Гэрэгэ аппын хэрэглэгчийн клиентээр HTTP хүсэлт үүсгэж илгээн мэдээлэл хүлээж авах.
        /// Амжилттай биелсэн хүсэлтийн хариуг cache-д хадгална. 
        /// <para>
        /// Зөв зарлагдсан T темплейт класс/бүтцээс Гэрэгэ мессеж дугаарыг авч хүсэлтийн толгойн message_code талбарт онооно.
        /// T темплейт бүтэц/класс буруу зарлагдсан, хүсэлтийн параметрууд буруу өгөгдсөн, холболт тасарсан, серверээс хариу ирээгүй, ирсэн хариуны формат зөрсөн
        /// гэх мэтчилэн болон өөр бусад шалтгаануудын улмаас Exception алдаа үүсэх боломжтой тул заавал try {} catch (Exception) {} код блок дунд ашиглана.
        /// </para>
        /// </summary>
        /// <param name="a">Өргөтгөлийг ашиглаж буй объект.</param>
        /// <param name="payload">Хүсэлтийн бие.</param>
        /// <param name="method">Хүсэлтийн дүрэм. Анхны утга null үед POST дүрэм гэж үзнэ.</param>
        /// <param name="requestUri">Хүсэлт илгээх хаяг.</param>
        /// <exception cref="Exception">
        /// T темплейт бүтэц/класс буруу зарлагдсан, хүсэлтийн параметрууд буруу өгөгдсөн, холболт тасарсан, серверээс хариу ирээгүй,
        /// ирсэн хариуны формат зөрсөн гэх мэтчилэн алдаануудын улмаас Exception үүсгэж шалтгааныг мэдэгдэнэ.
        /// </exception>
        /// <returns>
        /// Серверээс ирсэн хариуг амжилттай авсан эсвэл Cache дээрээс амжилттай уншсан мэдээллийг тухайн зарласан T темплейт класс обьектэд хөрвүүлсэн утгыг буцаана
        /// </returns>
        public static T UserCacheRequest<T>(this object a, dynamic payload = null, HttpMethod method = null, string requestUri = null)
        {
            return a.App().UserClient.CacheRequest<T>(payload, method, requestUri);
        }

        /// <summary>
        /// Аппын хавтаснаас зургийн төрлийн файлаас мэдээлэл унших.
        /// </summary>
        /// <param name="a">Өргөтгөлийг ашиглаж буй объект.</param>
        /// <param name="filePath">Файл зам нэр.</param>
        /// <returns>
        /// Амжилттай уншигдсан бол BitmapImage төрлийн утга бусад тохиолдолд null утга буцаана.
        /// </returns>
        public static BitmapImage ReadBitmapImage(this object a, string filePath)
        {
            try
            {
                string completePath = a.App().CurrentDirectory + filePath;

                if (!File.Exists(completePath))
                    throw new FileNotFoundException(completePath + " гэсэн файл байхгүй л байна даа!");

                return new BitmapImage(new Uri(completePath));
            }
            catch
            {
                return null;
            }
        }
    }
}
#pragma warning restore IDE0060
