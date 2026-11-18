using System.Windows;
using Modeling_and_Simulation_Clinic.Services;

namespace Modeling_and_Simulation_Clinic;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeService.ApplyTheme(isDarkMode: true);
    }
}
