using System.Windows;
using Wpf.Ui.Appearance;

namespace Modeling_and_Simulation_Clinic.Services;

/// <summary>
/// Switches the app between dark and light mode: the app's own colours
/// (Resources/Themes) and the WPF-UI control theme.
/// </summary>
public static class ThemeService
{
    private const string DarkThemePath =
        "pack://application:,,,/Modeling_and_Simulation_Clinic;component/Resources/Themes/DarkTheme.xaml";
    private const string LightThemePath =
        "pack://application:,,,/Modeling_and_Simulation_Clinic;component/Resources/Themes/LightTheme.xaml";

    /// <summary>True while the dark theme is applied.</summary>
    public static bool IsDarkMode { get; private set; } = true;

    /// <summary>Applies the dark or the light theme to the whole app.</summary>
    /// <param name="isDarkMode">True for dark mode, false for light mode.</param>
    public static void ApplyTheme(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;

        ApplyCustomThemeResources(isDarkMode);

        ApplicationThemeManager.Apply(isDarkMode ? ApplicationTheme.Dark : ApplicationTheme.Light);
    }

    /// <summary>Swaps the app's colour dictionary (DarkTheme.xaml / LightTheme.xaml).</summary>
    private static void ApplyCustomThemeResources(bool isDarkMode)
    {
        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

        ResourceDictionary? existing = mergedDictionaries.FirstOrDefault(d =>
            d.Source?.OriginalString is { } source &&
            (source.EndsWith("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
             source.EndsWith("LightTheme.xaml", StringComparison.OrdinalIgnoreCase)));

        if (existing != null)
            mergedDictionaries.Remove(existing);

        mergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(isDarkMode ? DarkThemePath : LightThemePath)
        });
    }
}
