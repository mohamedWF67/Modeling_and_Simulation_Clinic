using System.Windows.Controls;
using Modeling_and_Simulation_Clinic.ViewModels;

namespace Modeling_and_Simulation_Clinic.Pages;

/// <summary>
/// The simulation table of the last run: nurse columns, doctor columns, or both combined.
/// </summary>
public partial class SimulationTablePage : Page
{
    public SimulationTablePage()
    {
        InitializeComponent();
        DataContext = SimulationViewModel.Instance;
    }
}
