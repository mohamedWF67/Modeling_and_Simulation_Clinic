using Modeling_and_Simulation_Clinic.Entities;

namespace Modeling_and_Simulation_Clinic.ViewModels;

/// <summary>
/// Each row in the patient table.
/// </summary>
public sealed record PatientRow(
    int Id,
    string Type,
    int RnType,
    int? RnIat,
    int Iat,
    int Arrival,
    int NurseStart,
    int NurseTime,
    int NurseEnd,
    int NurseWait,
    int RnExam,
    int ExamTime,
    string Doctor,
    int? DoctorStart,
    int? DoctorEnd,
    int DoctorWait,
    int TotalWait,
    int TimeInSystem,
    int? RnLeave,
    bool Left)
{
    /// <summary>Gets the values from a patient.</summary>
    public static PatientRow From(Patient p) => new(
        p.id,
        p.type == Patient.patientType.Elderly ? "Elderly" : "Middle-age",
        p.R_patient_type,
        p.R_patient_iat == Patient.Empty ? null : p.R_patient_iat,
        p.iat,
        p.arrivalTime,
        p.n_start,
        p.n_time,
        p.n_end,
        p.n_wait,
        p.R_patient_exam_time,
        p.exam_time,
        p.d_name ?? "—",
        p.leftClinic ? null : p.d_start,
        p.leftClinic ? null : p.d_end,
        p.d_wait,
        p.totalWait,
        p.timeInSystem,
        p.R_patient_leave == Patient.Empty ? null : p.R_patient_leave,
        p.leftClinic);
}
