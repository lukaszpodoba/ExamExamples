using Example3.DTOs;
using FluentValidation;

namespace Example3.Validators;

public class CreatePrescriptionValidator : AbstractValidator<PrescriptionDTO.CreatePrescription>
{
    public CreatePrescriptionValidator()
    {
        RuleFor(e => e.Date).NotEmpty();
        RuleFor(e => e.DueDate).NotEmpty().GreaterThanOrEqualTo(e => e.Date);
        RuleFor(e => e.IdDoctor).NotEmpty();
        RuleFor(e => e.IdPatient).NotEmpty();
    }
}