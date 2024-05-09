using System.Data;
using System.Data.SqlClient;
using Dapper;
using Example3.DTOs;
using Example3.Models;

namespace Example3.Services;


public interface IDbServiceDapper
{
    //GET
    Task<bool> DoctorExistByName(string name);
    Task<List<PrescriptionDTO.GetPrescription>> GetPrescriptionByDoctorName(string name);
    //POST
    Task<Prescription> CreateNewPrescription(PrescriptionDTO.CreatePrescription createPrescription);
    Task<bool> PatientExistById(int id);
    Task<bool> DoctorExistsById(int id);

}

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

    public async Task<bool> DoctorExistByName(string name)
    {
        await using var connection = await GetConnection(); //Connecting
        var result = await connection.QueryFirstOrDefaultAsync<int>(
            "SELECT 1 FROM Doctor WHERE LastName = @LN", new
            {
                LN = name
            });
        return result == 0;
    }

    public async Task<List<PrescriptionDTO.GetPrescription>> GetPrescriptionByDoctorName(string? name)
    {
        await using var connection = await GetConnection(); //Connecting
        
        if (string.IsNullOrEmpty(name) )
        {
            var firstResult = await connection.QueryAsync<PrescriptionDTO.GetPrescription>(
                "SELECT IdPrescription, Date, DueDate, P2.LastName AS 'PatientLastName', DOCTOR.LastName AS 'DoctorLastName' FROM DOCTOR INNER JOIN Prescription P on DOCTOR.IdDoctor = P.IdDoctor INNER JOIN Patient P2 on P2.IdPatient = P.IdPatient");
            return firstResult.ToList();
        }
        
        var result = await connection.QueryAsync<PrescriptionDTO.GetPrescription>(
            "SELECT IdPrescription, Date, DueDate, P2.LastName AS 'PatientLastName', DOCTOR.LastName AS 'DoctorLastName' FROM DOCTOR INNER JOIN Prescription P on DOCTOR.IdDoctor = P.IdDoctor INNER JOIN Patient P2 on P2.IdPatient = P.IdPatient WHERE DOCTOR.LastName = @LN",
            new
            {
                LN = name
            });
        return result.ToList();
    }

    public async Task<Prescription> CreateNewPrescription(PrescriptionDTO.CreatePrescription createPrescription)
    {
        await using var connection = await GetConnection(); //Connecting
        await using var transaction = await connection.BeginTransactionAsync(); //Series of operations

        try
        {
            await connection.ExecuteAsync(
                "INSERT INTO Prescription (Date, DueDate, IdPatient, IdDoctor)" +
                "VALUES (@NewDate, @NewDueDate, @NewIdPatient, @NewIdDoctor)", new
                {
                    @NewDate = createPrescription.Date,
                    @NewDueDate = createPrescription.DueDate,
                    @NewIdPatient = createPrescription.IdPatient,
                    @NewIdDoctor = createPrescription.IdDoctor
                }, transaction: transaction);
            
            var idPrescription = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT MAX(IdPrescription) FROM Prescription", transaction: transaction);

            await transaction.CommitAsync();
            
            return new Prescription()
            {
                IdPrescription = idPrescription,
                Date = createPrescription.Date,
                DueDate = createPrescription.DueDate,
                IdDoctor = createPrescription.IdDoctor,
                IdPatient = createPrescription.IdPatient
            };
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> PatientExistById(int id)
    {
        await using var connection = await GetConnection(); //Connecting
        var result = await connection.QueryFirstOrDefaultAsync<int>(
            "SELECT 1 FROM Patient WHERE IdPatient = @NewIdPatient", new
            {
                NewIdPatient = id
            });
        return result == 0;
    }

    public async Task<bool> DoctorExistsById(int id)
    {
        await using var connection = await GetConnection(); //Connecting
        var result = await connection.QueryFirstOrDefaultAsync<int>(
            "SELECT 1 FROM Doctor WHERE IdDoctor = @NewIdDoctor", new
            {
                NewIdDoctor = id
            });
        return result == 0;
    }
}