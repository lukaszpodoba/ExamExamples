using System.Data;
using System.Data.SqlClient;
using Dapper;
using Example2.DTOs;
using Example2.Models;

namespace Example2.Services;

public interface IDbServiceDapper
{
    Task<Owner?> GetOwnerByAnimalId(int id);
    Task<List<Procedure>> GetProcedureByAnimalId(int id);
    Task<AnimalDTOs.GetAnimalById> GetAnimalById(int id);
    Task<Animal?> AnimalExists(int id);
    //----------------------
    Task<Owner?>? GetOwnerById(int id);
    Task<Procedure?> GetProcedureById(int id);
    Task<int?> CreateNewAnimalWithProcedures(AnimalDTOs.CreateAnimal createAnimal);
};

public class DbServiceDapper(IConfiguration configuration) : IDbServiceDapper
{
    // Helper method for creating and opening connection
    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }

    public async Task<Owner?> GetOwnerByAnimalId(int id)
    {
        await using var connection = await GetConnection(); //Connecting
        var result = await connection.QueryFirstOrDefaultAsync<Owner>(
            "SELECT * FROM Owner WHERE ID = @OwnerId", new
            {
                OwnerId = id
            });
        return result;
    }

    public async Task<List<Procedure>> GetProcedureByAnimalId(int id)
    {
        await using var connection = await GetConnection(); //Connecting
        var result = await connection.QueryAsync<Procedure>(
            "SELECT Name, Description, Date FROM [Procedure] INNER JOIN Procedure_Animal ON [Procedure].Id = Procedure_ID WHERE Animal_ID = @AnimalId", new
            {
                AnimalId = id
            });
        return result.ToList();
    }

    public async Task<AnimalDTOs.GetAnimalById> GetAnimalById(int id)
    {
        await using var connection = await GetConnection(); //Connecting
        
        var owner = GetOwnerByAnimalId(id);
        var procedures = GetProcedureByAnimalId(id);

        var animal = await connection.QueryFirstOrDefaultAsync<Animal>(
            "SELECT * FROM Animal WHERE ID = @AnimalId", new
            {
                AnimalId = id
            });

        var result = new AnimalDTOs.GetAnimalById(
            id,
            animal.Name,
            animal.Type,
            animal.AdmissionDate,
            owner.Result,
            procedures.Result);

        return result;
    }

    public async Task<Animal?> AnimalExists(int id)
    {
        await using var connection = await GetConnection(); //Connecting

        var result = await connection.QueryFirstOrDefaultAsync<Animal>(
            "SELECT * FROM Animal Where ID = @AnimalId", new
            {
                AnimalId = id
            });
        return result;
    }

    public async Task<Owner?>? GetOwnerById(int id)
    {
        await using var connection = await GetConnection(); //Connecting

        var result = await connection.QueryFirstOrDefaultAsync<Owner>(
            "SELECT * FROM Owner WHERE ID = @OwnerId", new
            {
                OwnerId = id
            });
        return result;
    }

    public async Task<Procedure?> GetProcedureById(int id)
    {
        await using var connection = await GetConnection(); //Connecting
        
        var result = await connection.QueryFirstOrDefaultAsync<Procedure>(
            "SELECT * FROM [Procedure] WHERE ID = @ProcedureId", new
            {
                ProcedureId = id
            });
        return result;
    }

    public async Task<int?> CreateNewAnimalWithProcedures(AnimalDTOs.CreateAnimal createAnimal)
    {
        await using var connection = await GetConnection(); //Connecting
        await using var transaction = await connection.BeginTransactionAsync(); //Series of operations

        var owner = GetOwnerById(createAnimal.OwnerId);

        List<Procedure_AnimalDTO.CreateProcedureAnimal> procedureAnimalDtos = [];
        foreach (var procedure in createAnimal.ProcedureAnimals)
        {
            procedureAnimalDtos.Add(new Procedure_AnimalDTO.CreateProcedureAnimal(
                procedure.ProcedureId,
                procedure.Date
                ));
        }

        try
        {
            await connection.ExecuteAsync(
                "INSERT INTO Animal (Name, Type, AdmissionDate, Owner_ID) VALUES (@AName, @AType, @AAdmissionDate, @AOwner_ID)", new
                {
                    @AName = createAnimal.Name,
                    @AType = createAnimal.Type,
                    @AAdmissionDate = createAnimal.AdmissionDate,
                    @AOwner_ID = owner.Id
                }, transaction);

            var idAnimal = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT MAX(ID) FROM ANIMAL", transaction: transaction);
            
            if (procedureAnimalDtos.Count > 0)
            {
                foreach (var procedure in procedureAnimalDtos)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO Procedure_Animal (Procedure_ID, Animal_ID, Date) VALUES (@PID, @AID, @ADATE)", new
                        {
                            PID = procedure.ProcedureId,
                            AID = idAnimal,
                            ADATE = procedure.Date
                        }, transaction);   
                }
            }
            
            await transaction.CommitAsync();

            return idAnimal;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}