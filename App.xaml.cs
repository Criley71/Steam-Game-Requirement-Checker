using SteamReqDesktop.Services;
using SteamReqDesktop.ViewModels;
using SteamReqDesktopWPF.views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SteamReqDesktopWPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            // 1. Create your tools (Note: using your exact file names from the screenshot!)
            var hardwareService = new HardwareInfoService();
            var benchmarkService = new BenchmarkScoreService();
            var scraperService = new SteamScraper();

            // 2. Hand them to the ViewModel
            var mainViewModel = new MainViewModel(hardwareService, benchmarkService, scraperService);

            // 3. Create the window, connect the data, and FORCE it to open
            var mainWindow = new MainWindow();
            mainWindow.DataContext = mainViewModel;

            mainWindow.Show(); // <-- IF THIS IS MISSING, NO WINDOW APPEARS!
        }
    }

}
