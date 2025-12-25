using Payroll.Application.Common;

namespace Payroll.Infrastructure.Identity;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
