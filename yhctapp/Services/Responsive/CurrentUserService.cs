using System.Linq;
using System.Security.Claims;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? DepartmentId
    {
        get
        {
            var deptClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("IdDepartmentRoom")?.Value;
            return deptClaim;
        }
    }

    public bool IsAdmin
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return false;
            return user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.Equals("ADMIN", System.StringComparison.OrdinalIgnoreCase));
        }
    }
}