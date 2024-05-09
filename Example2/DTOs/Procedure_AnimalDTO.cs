using Example2.Models;

namespace Example2.DTOs;

public class Procedure_AnimalDTO
{
    public record CreateProcedureAnimal(int ProcedureId, DateTime Date);
}