using System.Data;
using System.Data.SqlClient;
using Dapper;
using Example1.DTOs;
using Example1.Models;

namespace Example1.Services;

public interface IDbServiceDapper
{ 
    Task<Group?> GetGroupId(int id);
    Task<List<int>> GetStudentsIdByGroupId(int id); 
    Task<GroupDTO.GetGroupById?> GetGroupWithStudentsById(int id);
    //-----------------------
    Task<bool> DeleteStudentById(int id);
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

    public async Task<Group?> GetGroupId(int id)
    {
        await using var connection = await GetConnection(); //Connecting
        var result = await connection.QueryFirstOrDefaultAsync<Group>(
            "SELECT * FROM Groups WHERE ID = @GroupId", new
            {
                GroupId = id
            });
        return result;
    }

    public async Task<List<int>> GetStudentsIdByGroupId(int id)
    {
        await using var connection = await GetConnection(); //Connecting
        var result = await connection.QueryAsync<int>(
            "SELECT * FROM GroupAssignments WHERE Group_ID = @GroupId", new
            {
                GroupId = id
            });
        return result.ToList();
    }

    public async Task<GroupDTO.GetGroupById?> GetGroupWithStudentsById(int id)
    {
        await using var connection = await GetConnection(); //Connecting
        var group = GetGroupId(id);
        var studentsList = GetStudentsIdByGroupId(id);
        var result = new GroupDTO.GetGroupById(id, group.Result.Name, studentsList.Result);

        return result;
    }

    public async Task<bool> DeleteStudentById(int id)
    {
        await using var connection = await GetConnection(); //Connecting
        await using var transaction = await connection.BeginTransactionAsync(); //Series of operations
        try
        {
            var firstResult = await connection.ExecuteAsync(
                "DELETE FROM GroupAssignments WHERE Student_ID = @StudentId", new
                {
                    StudentId = id
                }, transaction);

            var secondResult = await connection.ExecuteAsync(
                "DELETE FROM Students WHERE ID = @StudentId", new
                {
                    StudentId = id
                }, transaction);

            await transaction.CommitAsync();

            return secondResult != 0;

        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}