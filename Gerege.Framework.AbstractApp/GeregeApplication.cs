using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Gerege.Framework.AbstractApp
{
    /// <author>
    /// codesaur - 2022.01.22
    /// </author>
    /// <project>
    /// Gerege Application Development Framework V5
    /// </project>

    /// <summary>
    /// Гэрэгэ логикоор ажиллах програм хангамжын үндсэн суурь апп хийсвэр класс.
    /// </summary>
    public abstract class GeregeApplication : Application
    {
        /// <summary>Апп процесс идэвхтэй ажиллаж буй хавтас зам.</summary>
        public string CurrentDirectory;

        /// <summary>Гэрэгэ үзэгдэл хүлээн авагч төрөл зарлах.</summary>
        /// <param name="name">Гэрэгэ үзэгдэл нэр.</param>
        /// <param name="args">Үзэгдэлд дамжуулах өгөгдөл параметр.</param>
        /// <returns>Үзэгдэл хүлээн авагчаас үр дүн эсвэл null буцаана.</returns>
        public delegate dynamic? GeregeEventHandler(string name, dynamic? args = null);

        /// <summary>Gerege үзэгдэл хүлээн авагч.</summary>
        public event GeregeEventHandler? EventHandler;

        /// <summary>Гэрэгэ апп үүсгэгч.</summary>
        public GeregeApplication()
        {
            // App идэвхитэй ажиллаж байгаа хавтас олоx
            DirectoryInfo? currentDir = null;
            if (Environment.ProcessPath != null)
                currentDir = Directory.GetParent(Environment.ProcessPath);
            if (currentDir == null)
                currentDir = new DirectoryInfo(Environment.CurrentDirectory);
            CurrentDirectory = currentDir.FullName + Path.DirectorySeparatorChar;
        }

        /// <summary>Гэрэгэ үзэгдлийг идэвхжүүлэх.</summary>
        /// <param name="name">Идэвхжүүлэх үзэгдэл нэр.</param>
        /// <param name="args">Үзэгдэлд дамжуулагдах өгөгдөл.</param>
        /// <returns>
        /// Үзэгдэл хүлээн авагчаас үр дүн ирвэл dynamic төрлөөр буцаана.
        /// Ямар нэгэн алдаа гарч Exception үүссэн бол үзэгдлийн үр дүнд алдааны мэдээллийг олгоно.
        /// <para>Үзэгдэл хүлээн авагчаас үр дүн null байх боломжтой.</para>
        /// </returns>
        public virtual dynamic? RaiseEvent(string name, dynamic? args = null)
        {
            // боломжит үзэгдэл хүлээн авагчдийг цуглуулж байна
            Delegate[]? delegates = EventHandler?.GetInvocationList();

            // боломжит үзэгдэл хүлээн авагчид байхгүй тул null утга буцаая
            if (delegates == null) return null;

            // хүлээн авагчидаас боловсруулсан үр дүнг энэ жагсаалтад бүртгэе
            List<dynamic> results = new();

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
        /// Үүнд GeregeWindowsApp нь анх амжилттай ачаалагдах үедээ энэ хийсвэр функцыг дуудан
        /// HTTP клиент обьект болон лог бичих обьект гэх мэт шаардлагатай бүрэлдэхүүн хэсгээ үүсгэдэг байх ёстой.
        /// </para>
        /// <para>
        /// Хөгжүүлэгч нь заавал энэ функцийг удамшуулсан класс дээрээ override түлхүүр үгээр
        /// дахин функц болгон тодорхойлж  тухайн aппд шаардлагатай бүрэлдэхүүн обьектуудыг үүсгэнэ.
        /// </para>
        /// </summary>
        protected abstract void CreateComponents();

        /// <summary>
        /// Гэрэгэ үйлчилгээний DLL ачаалж Module классын Start функцыг дуудсанаар FrameworkElement авах.
        /// </summary>
        /// <param name="filePath">Module DLL файлын зам/нэр.</param>
        /// <param name="param">Module.Start функцэд дамжуулах параметр.</param>
        /// <returns>
        /// Амжилттай уншигдаж үүссэн FrameworkElement.
        /// </returns>
        public FrameworkElement LoadModule(string filePath, dynamic param)
        {
            if (string.IsNullOrEmpty(filePath)
                    || !File.Exists(filePath))
                throw new Exception(filePath + ": Модул зам олдсонгүй!");

            string dllName = Path.GetFileName(filePath);

            Assembly assembly = Assembly.LoadFrom(filePath);
            Type? type = assembly.GetType("Module");
            if (type == null) throw new Exception(dllName + ": Module class олдсонгүй!");

            object? instanceOfMyType = Activator.CreateInstance(type);
            if (instanceOfMyType == null) throw new Exception(dllName + ": Module обьект үүсгэж чадсангүй!");

            MethodInfo? methodInfo = type.GetMethod("Start", new Type[] { typeof(object) });
            if (methodInfo == null) throw new Exception(dllName + ": Module.Start функц олдоогүй эсвэл буруу тодорхойлсон байна!");

            try
            {
                object[] parameters = new object[1] { param };
                object? result = methodInfo.Invoke(instanceOfMyType, parameters);
                return result is FrameworkElement element ? element : throw new Exception(dllName + ": Module.Start функц нь FrameworkElement буцаасангүй!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                throw ex.InnerException ?? ex;
            }
        }

        private Mutex? _instanceMutex;

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
            _instanceMutex = new Mutex(true, @"Global\ControlPanel", out bool createdNew);

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
                var runningProcess = (from process in Process.GetProcesses()
                                      where
                                        process.Id != currentProcess.Id &&
                                        process.ProcessName.Equals(
                                          currentProcess.ProcessName,
                                          StringComparison.Ordinal)
                                      select process).FirstOrDefault();
                if (runningProcess != null)
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
            _instanceMutex?.ReleaseMutex();

            base.OnExit(e);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, uint Msg);

        private const int SW_SHOWNOACTIVATE = 4;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
