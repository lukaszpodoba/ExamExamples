using Example2.DTOs;
using Example2.Services;
using FluentValidation;

namespace Example2.Endpoints;

public static class GroupDapperEndpoints
{
    
    public static void RegisterGroupDapperEndpoint(this WebApplication app)
    {
        app.MapGet("api/groups/{id:int}", GetAnimal);
        app.MapPost("api/animal/", CreateAnimal);
    }
    
    private static async Task<IResult> GetAnimal(
        int id,
        IDbServiceDapper db
    )
    {
        if (db.AnimalExists(id).Result == null)
        {
            return Results.NotFound("Animal with given id does not exist");
        }
        
        var result = await db.GetAnimalById(id);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateAnimal(
        AnimalDTOs.CreateAnimal request,
        IDbServiceDapper db,
        IValidator<AnimalDTOs.CreateAnimal> validator)
    {
        // Validate request
        var validate = await validator.ValidateAsync(request);
        if (!validate.IsValid)
        {
            return Results.ValidationProblem(validate.ToDictionary());
        }
        
        //Check if owner exists
        var owner = await db.GetOwnerById(request.OwnerId);
        if (owner == null)
        {
            return Results.NotFound("Owner with given id does not exist");
        }
        
        //Checking if procedures exists
        foreach (var procedure in request.ProcedureAnimals)
        {
            var procedureIsNull = await db.GetProcedureById(procedure.ProcedureId);
            if (procedureIsNull == null)
            {
                return Results.NotFound($"Procedure with id {procedure.ProcedureId} does not exist");
            }
        }
        
        var result = await db.CreateNewAnimalWithProcedures(request);
        return Results.Created($"api/animal/{result}", result);
    }
}