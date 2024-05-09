using Example1.Services;

namespace Example1.Endpoints;

public static class GroupDapperEndpoint
{
    public static void RegisterGroupDapperEndpoint(this WebApplication app)
    {
        app.MapGet("api/groups/{id:int}", GetGroup);
        app.MapDelete("/api/students/{id:int}", DeleteStudent);
    }

    private static async Task<IResult> GetGroup(
        int id,
        IDbServiceDapper db
    )
    {
        if (db.GetGroupId(id).Result == null)
        {
            return Results.NotFound("Group with given id does not exist");
        }
        
        var result = await db.GetGroupWithStudentsById(id);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteStudent(
        int id,
        IDbServiceDapper db
    )
    {
        return await db.DeleteStudentById(id) ? Results.NoContent() : Results.NotFound($"Student with given id does not exist");
    }
}