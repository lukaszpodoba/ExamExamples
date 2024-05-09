using Example2.Models;

namespace Example2.DTOs;

public class AnimalDTOs
{
    public record GetAnimalById(
        int Id,
        string Name,
        string Type,
        DateTime AdmissionDate,
        Owner Owner,
        List<Procedure> Procedures);

    public record CreateAnimal(
        string Name,
        string Type,
        DateTime AdmissionDate,
        int OwnerId,
        List<Procedure_AnimalDTO.CreateProcedureAnimal> ProcedureAnimals
        );
}