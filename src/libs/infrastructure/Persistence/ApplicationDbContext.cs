using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Common;
using Payroll.Domain.Common;
using Payroll.Domain.Employees;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayRun> PayRuns => Set<PayRun>();
    public DbSet<PayRunLineItem> PayRunLineItems => Set<PayRunLineItem>();
    public DbSet<Payslip> Payslips => Set<Payslip>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);

        // Apply global query filter for multi-tenancy
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType).HasQueryFilter(CreateTenantQueryFilter(entityType.ClrType));
            }
        }
    }

    private LambdaExpression CreateTenantQueryFilter(Type type)
    {
        // Equivalent to: x => x.TenantId == this.TenantId
        var param = Expression.Parameter(type, "x");
        var tenantIdProp = Expression.Property(param, nameof(ITenantEntity.TenantId));
        
        // This captures the 'this' context instance correctly for EF Core
        var contextTenantIdProp = Expression.Property(Expression.Constant(this), nameof(TenantId));
        
        var body = Expression.Equal(tenantIdProp, contextTenantIdProp);
        
        return Expression.Lambda(body, param);
    }

    // TenantId property accessed by the query filter
    public Guid TenantId => _tenantContext.TenantId;
}
