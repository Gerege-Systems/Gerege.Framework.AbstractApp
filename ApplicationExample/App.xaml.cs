namespace ApplicationExample;

/////// date: 2022.02.09 //////////
///// author: Narankhuu ///////////
//// contact: codesaur@gmail.com //

using GeregeSampleApp;

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
