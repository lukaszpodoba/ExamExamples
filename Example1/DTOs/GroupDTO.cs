namespace Example1.DTOs;

public class GroupDTO
{
    public record GetGroupById(int Id, string Name, List<int> StudentsId);
}