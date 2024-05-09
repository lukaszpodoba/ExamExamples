namespace Example3.DTOs;

public class PrescriptionDTO
{
    public record GetPrescription(
        int IdPrescription,
        DateTime Date,
        DateTime DueDate,
        string PatientLastName,
        string DoctorLastName
    );

    public record CreatePrescription(
        DateTime Date,
        DateTime DueDate,
        int IdPatient,
        int IdDoctor
    );
}