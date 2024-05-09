using Example3.DTOs;
using Example3.Services;
using FluentValidation;

namespace Example3.Endpoints;

public static class GroupDapperEndpoints
{
    public static void RegisterGroupDapperEndpoint(this WebApplication app)
    {
        app.MapGet("api/prescription/", GetPrescription);
        app.MapPost("api/prescription/", CreatePrescriptionEndpoint);
    }

    private static async Task<IResult> GetPrescription(
        string? name,
        IDbServiceDapper db
    )
    {
        if (name != null && db.DoctorExistByName(name).Result)
        {
            return Results.NotFound("Doctor with this surname does not exist");
        }

        var result = await db.GetPrescriptionByDoctorName(name);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreatePrescriptionEndpoint(
        PrescriptionDTO.CreatePrescription request,
        IDbServiceDapper db,
        IValidator<PrescriptionDTO.CreatePrescription> validator
    )
    {
        // Validate request
        var validate = await validator.ValidateAsync(request);
        if (!validate.IsValid)
        {
            return Results.ValidationProblem(validate.ToDictionary());
        }
        //Checking Patient
        if (db.PatientExistById(request.IdPatient).Result)
        {
            return Results.NotFound("Patient with given id does not exist");
        }
        //Checking Doctor
        if (db.DoctorExistsById(request.IdDoctor).Result)
        {
            return Results.NotFound("Doctor with given id does not exist");
        }
        
        var result = await db.CreateNewPrescription(request);
        return Results.Created($"api/prescription/{result}", result);
    }
}