namespace Payroll.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(string Token, string UserId, string TenantId)> LoginAsync(string email, string password);
}
