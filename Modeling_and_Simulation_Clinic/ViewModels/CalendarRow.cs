using Modeling_and_Simulation_Clinic.Entities;

namespace Modeling_and_Simulation_Clinic.ViewModels;

/// <summary>Who an event in the calendar belongs to; used for its icon and colour.</summary>
public enum CalendarEventKind
{
    Arrival,
    Nurse,
    Doctor,
    Leave
}

/// <summary>
/// One row of the event calendar shown on the right of the window.
/// </summary>
/// <param name="IsNewTime">True for the first event at this minute (the time is only highlighted once).</param>
public sealed record CalendarRow(int Time, string Event, int PatientId, string Details, CalendarEventKind Kind, bool IsNewTime)
{
    /// <summary>Copies a calendar entry from the simulation.</summary>
    /// <param name="entry">The calendar entry.</param>
    /// <param name="previous">The entry before it, or null for the first one.</param>
    public static CalendarRow From(CalendarEntry entry, CalendarEntry? previous) => new(
        entry.time,
        entry.eventName,
        entry.patientId,
        entry.details,
        KindOf(entry.eventName),
        previous == null || previous.time != entry.time);

    private static CalendarEventKind KindOf(string eventName)
    {
        if (eventName.StartsWith("Nurse"))
            return CalendarEventKind.Nurse;
        if (eventName.StartsWith("Doctor"))
            return CalendarEventKind.Doctor;
        if (eventName == "Leave")
            return CalendarEventKind.Leave;
        return CalendarEventKind.Arrival;
    }
}
