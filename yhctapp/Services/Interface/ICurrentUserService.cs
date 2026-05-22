public interface ICurrentUserService
{
   public string? DepartmentId { get; }
   public bool IsAdmin { get; }
}