using System.Windows;
using Modeling_and_Simulation_Clinic.Pages;
using Modeling_and_Simulation_Clinic.ViewModels;

namespace Modeling_and_Simulation_Clinic;

/// <summary>
/// Main window: navigation on the left, the selected page in the middle and the
/// event calendar of the last run on the right.
/// </summary>
public partial class MainWindow
{
    /// <summary>Width of the calendar before it was hidden, so showing it again restores it.</summary>
    private GridLength _calendarWidth;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = SimulationViewModel.Instance;
        FitToWorkArea();

        Loaded += (_, _) => NavigationView.Navigate(typeof(DistributedPage));
    }

    /// <summary>Keeps the window inside the screen on small or scaled displays.</summary>
    private void FitToWorkArea()
    {
        Rect workArea = SystemParameters.WorkArea;
        Width = Math.Max(MinWidth, Math.Min(Width, workArea.Width * 0.95));
        Height = Math.Max(MinHeight, Math.Min(Height, workArea.Height * 0.95));
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationView.GoBack();
    }

    private void PanelButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationView.IsPaneOpen = !NavigationView.IsPaneOpen;
    }

    /// <summary>Shows or hides the event calendar on the right.</summary>
    private void CalendarButton_Click(object sender, RoutedEventArgs e)
    {
        bool show = CalendarPanel.Visibility != Visibility.Visible;

        if (show)
        {
            CalendarColumn.MinWidth = 300;
            CalendarColumn.Width = _calendarWidth;
        }
        else
        {
            _calendarWidth = CalendarColumn.Width;
            CalendarColumn.MinWidth = 0;
            CalendarColumn.Width = new GridLength(0);
        }

        CalendarPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        CalendarSplitter.Visibility = CalendarPanel.Visibility;
    }

    private void CloseBtn_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Minimize_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow.WindowState = WindowState.Minimized;
    }

    private void Maximize_OnClick(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        else 
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
    }
}
