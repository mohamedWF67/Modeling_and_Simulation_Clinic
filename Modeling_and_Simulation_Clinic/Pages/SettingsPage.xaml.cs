using System.Windows.Controls;
using Modeling_and_Simulation_Clinic.Services;

namespace Modeling_and_Simulation_Clinic.Pages;

/// <summary>App settings (dark / light mode).</summary>
public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();

        // Set the switch before listening to it, so opening the page does not re-apply the theme
        DarkModeSwitch.IsChecked = ThemeService.IsDarkMode;
        DarkModeSwitch.Checked += (_, _) => ThemeService.ApplyTheme(isDarkMode: true);
        DarkModeSwitch.Unchecked += (_, _) => ThemeService.ApplyTheme(isDarkMode: false);
    }
}
