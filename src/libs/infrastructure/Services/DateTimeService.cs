using Payroll.Application.Common.Interfaces;

namespace Payroll.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}
