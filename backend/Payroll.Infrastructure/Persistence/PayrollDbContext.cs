using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Leave;
using Payroll.Domain.Loans;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence;

public class PayrollDbContext : DbContext
{
    public PayrollDbContext(DbContextOptions<PayrollDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayRun> PayRuns => Set<PayRun>();
    public DbSet<PaySlip> PaySlips => Set<PaySlip>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // TODO: apply configurations
    }
}
