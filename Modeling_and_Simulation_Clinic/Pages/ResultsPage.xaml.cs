using System.Windows.Controls;
using Modeling_and_Simulation_Clinic.ViewModels;

namespace Modeling_and_Simulation_Clinic.Pages;

public partial class ResultsPage : Page
{
    public ResultsPage()
    {
        InitializeComponent();
        DataContext = statisticsViewModel.Instance;
    }
}