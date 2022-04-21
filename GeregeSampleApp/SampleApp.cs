using System;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

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
            UserClient = new SampleUserClient(pipeline);
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
            Task task = Task.Run(() =>
            {
                try
                {
                    ClearCacheFolder();
                }
                catch { }
            });
            task.Wait();
        }

        /// <summary>
        /// Gerege үзэгдэл хүлээн авагч.
        /// </summary>
        public dynamic BaseEventHandler(string @event, dynamic param = null)
        {
            switch (@event)
            {
                case "get-server-address":
                    return "http://mock-server/api";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Гэрэгэ үйлчилгээний DLL ачаалж Module классын Start функцыг дуудсанаар үр дүнг авах.
        /// </summary>
        /// <param name="filePath">Module DLL файлын зам/нэр.</param>
        /// <param name="param">Module.Start функцэд дамжуулах параметр.</param>
        /// <returns>
        /// Амжилттай үр дүн.
        /// </returns>
        public object ModuleStart(string filePath, dynamic param)
        {
            if (string.IsNullOrEmpty(filePath)
                    || !File.Exists(filePath))
                throw new Exception(filePath + ": Модул зам олдсонгүй!");

            string dllName = Path.GetFileName(filePath);

            Assembly assembly = Assembly.LoadFrom(filePath);
            Type type = assembly.GetType("Module");
            if (type is null) throw new Exception(dllName + ": Module class олдсонгүй!");

            object instanceOfMyType = Activator.CreateInstance(type);
            if (instanceOfMyType is null) throw new Exception(dllName + ": Module обьект үүсгэж чадсангүй!");

            MethodInfo methodInfo = type.GetMethod("Start", new Type[] { typeof(object) });
            if (methodInfo is null) throw new Exception(dllName + ": Module.Start функц олдоогүй эсвэл буруу тодорхойлсон байна!");

            try
            {
                object[] parameters = new object[1] { param };
                return methodInfo.Invoke(instanceOfMyType, parameters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                throw ex.InnerException ?? ex;
            }
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
            var tempCache = new GeregeCache(0, new { tmp = 0 });
            if (string.IsNullOrEmpty(tempCache.FilePath)) return;

            var cacheFI = new FileInfo(tempCache.FilePath);
            if (cacheFI.Directory == null
                || !cacheFI.Directory.Exists
                || cacheFI.DirectoryName == null) return;

            var dir = new DirectoryInfo(cacheFI.DirectoryName);
            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }
            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                subdir.Delete(true);
            }
        }
    }
}
