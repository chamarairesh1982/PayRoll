using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Payroll.Application.Common;
using Payroll.Domain.Common;

namespace Payroll.Infrastructure.Persistence.Interceptors;

public class TenantScopingInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;

    public TenantScopingInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetTenantId(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SetTenantId(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetTenantId(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // Ensure TenantId is set from the context, preventing cross-tenant data entry
                entry.Entity.TenantId = _tenantContext.TenantId;
            }
        }
    }
}
