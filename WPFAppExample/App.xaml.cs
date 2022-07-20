using GeregeSampleApp;

/////// date: 2022.02.09 //////////
///// author: Narankhuu ///////////
//// contact: codesaur@gmail.com //

namespace WPFAppExample;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : SampleApp
{
    public App()
    {
        this.InitializeComponent();

        log4net.Config.XmlConfigurator.Configure();
    }
}
