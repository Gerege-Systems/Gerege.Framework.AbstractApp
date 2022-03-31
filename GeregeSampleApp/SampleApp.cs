using System.IO;
using System.Windows;

using Gerege.Framework.WPFApp;
using Gerege.Framework.HttpClient;

namespace GeregeSampleApp
{
    /// <summary>
    /// GeregeWPFApp-аас удамшсан SampleApp апп объект.
    /// </summary>
    public class SampleApp : GeregeWPFApp
    {
        /// <summary>
        /// Хэрэглэгчийн HTTP клиент обьект.
        /// </summary>
        public SampleUserClient UserClient;

        /// <inheritdoc />
        protected override void CreateComponents()
        {
            // Gerege үзэгдэл хүлээн авагч тохируулъя
            EventHandler += BaseEventHandler;

            // Хэрэглэгчийн HTTP клиентэд зориулж удирдлагууд үүсгэж байна
            var pipeline = new LoggingHandler(new ConsoleLogger())
            {
                InnerHandler = new RetryHandler()
                {
                    // Бодит серверлүү хандах бол энэ удирдлага ашиглаарай
                    //InnerHandler = new System.Net.Http.HttpClientHandler()

                    // Туршилтын зорилгоор хуурамч сервер хандалтын удирдлага ашиглаж байна
                    InnerHandler = new MockServerHandler()
                }
            };

            // Хэрэглэгчийн клиентийг үүсгэж байна
            UserClient = new(pipeline);
        }

        /// <summary>
        /// Апп анх ачаалагдах үед биелэх үзэгдлийн виртуал функц.
        /// </summary>
        /// <param name="e">Ачаалагдсан процессын утгууд.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // аппыг цор ганц хувилбараар ажиллуулах гол зорилготой эх ангийн үзэгдлийг дуудаж байна
            base.OnStartup(e);

            // Апп процесс амжилттай ачаалсан тул өмнө нь Гэрэгэ HTTP клиентийн
            // хүсэлт боловсруулах үед үүссэн байж болох Cache хавтас байвал цэвэрлэе
            ClearCacheFolder();
        }

        /// <summary>
        /// Gerege үзэгдэл хүлээн авагч.
        /// </summary>
        public dynamic? BaseEventHandler(string @event, dynamic? param = null)
        {
            return @event switch
            {
                "get-server-address" => "http://mock-server/api",

                _ => null,
            };
        }

        /// <summary>
        /// Гэрэгэ HTTP клиент хүсэлт боловсруулах үед үүссэн Cache хавтас байвал цэвэрлэх.
        /// <para>
        /// Гэрэгэ HTTP клиент нь хүсэлт боловсруулахдаа Cache үүсгэх боломжтой.
        /// Тийм учраас Апп шинээр ачаалах үед өмнөх процессийн үүсгэсэн Cache хавтас байгаа бол цэвэрлэх хэрэгтэй.
        /// </para>
        /// </summary>
        private void ClearCacheFolder()
        {
            try
            {
                GeregeCache tempCache = new(0, new { tmp = 0 });
                if (string.IsNullOrEmpty(tempCache.FilePath)) return;

                FileInfo cacheFI = new(tempCache.FilePath);
                if (cacheFI.Directory == null
                    || !cacheFI.Directory.Exists
                    || cacheFI.DirectoryName == null) return;

                DirectoryInfo dir = new(cacheFI.DirectoryName);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }
                foreach (DirectoryInfo subdir in dir.GetDirectories())
                {
                    subdir.Delete(true);
                }
            }
            catch { }
        }
    }
}
