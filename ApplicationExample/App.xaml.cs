using GeregeSampleApp;

namespace ApplicationExample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : SampleApp
    {
        public App()
        {
            InitializeComponent();

            log4net.Config.XmlConfigurator.Configure();
        }
    }
}
