using Payroll.Application.Common;

namespace Payroll.Application.Tests.Common;

public class TestTenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
}
