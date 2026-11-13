using System.Configuration;
using System.Data;
using System.Windows;
using Modeling_and_Simulation_Clinic.Entities;
using Modeling_and_Simulation_Clinic.Random;

namespace Modeling_and_Simulation_Clinic;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        Clinic c = new Clinic("El Dorado");
        c.BuildDistributionTable();
        c.InitializeDoctors();
        c.createPatients(20);
        c.computePatientsOutcomes(99);
    }
}