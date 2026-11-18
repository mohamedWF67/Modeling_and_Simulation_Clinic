namespace Modeling_and_Simulation_Clinic.Entities;

/// <summary>
/// One row of the calendar (event) table: what happened, when, and to which patient.
/// </summary>
public class CalendarEntry
{
    public int time;
    public string eventName;
    public int patientId;
    public string details;

    public CalendarEntry(int time, string eventName, int patientId, string details)
    {
        this.time = time;
        this.eventName = eventName;
        this.patientId = patientId;
        this.details = details;
    }

    /// <summary>Prints the row to the console.</summary>
    public void print()
    {
        Console.WriteLine($"{time}\t{eventName}\tPatient {patientId}\t{details}");
    }
}
