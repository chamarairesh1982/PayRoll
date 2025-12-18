namespace Payroll.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    IReadOnlyCollection<string> Roles { get; }
}
