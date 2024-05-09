using Example2.DTOs;
using FluentValidation;

namespace Example2.Validators;

public class CreateAnimalValidator : AbstractValidator<AnimalDTOs.CreateAnimal>
{
    public CreateAnimalValidator()
    {
        RuleFor(e => e.Name).MaximumLength(100).NotEmpty();
        RuleFor((e => e.Type)).MaximumLength(100).NotEmpty();
        RuleFor(e => e.AdmissionDate).NotEmpty().Must(date => date.Date <= DateTime.Now);
        RuleFor(e => e.ProcedureAnimals).NotNull();
        RuleFor(e => e.OwnerId).NotEmpty();
    }
}