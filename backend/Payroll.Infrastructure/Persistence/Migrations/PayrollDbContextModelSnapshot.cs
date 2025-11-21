using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Payroll.Infrastructure.Persistence;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PayrollDbContext))]
partial class PayrollDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.8")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("Payroll.Domain.Employees.Employee", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            b.Property<decimal>("BaseSalary")
                .HasColumnType("decimal(18,2)");

            b.Property<string>("CallingName")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<DateTime?>("ConfirmationDate")
                .HasColumnType("datetime2");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("datetime2");

            b.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<DateTime>("DateOfBirth")
                .HasColumnType("datetime2");

            b.Property<string>("EmployeeCode")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            b.Property<DateTime>("EmploymentStartDate")
                .HasColumnType("datetime2");

            b.Property<string>("FirstName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<int>("Gender")
                .HasColumnType("int");

            b.Property<string>("Initials")
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            b.Property<bool>("IsActive")
                .HasColumnType("bit");

            b.Property<string>("LastName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<int>("MaritalStatus")
                .HasColumnType("int");

            b.Property<DateTime?>("ModifiedAt")
                .HasColumnType("datetime2");

            b.Property<string>("ModifiedBy")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("NicNumber")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            b.Property<DateTime?>("ProbationEndDate")
                .HasColumnType("datetime2");

            b.HasKey("Id");

            b.HasIndex("EmployeeCode")
                .IsUnique();

            b.HasIndex("NicNumber")
                .IsUnique();

            b.ToTable("Employees", (string)null);
        });

        modelBuilder.Entity("Payroll.Domain.Leave.LeaveRequest", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            b.Property<Guid?>("ApprovedById")
                .HasColumnType("uniqueidentifier");

            b.Property<DateTimeOffset?>("ApprovedAt")
                .HasColumnType("datetimeoffset");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("datetime2");

            b.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<Guid>("EmployeeId")
                .HasColumnType("uniqueidentifier");

            b.Property<DateOnly>("EndDate")
                .HasColumnType("date");

            b.Property<string>("HalfDaySession")
                .HasMaxLength(2)
                .HasColumnType("nvarchar(2)");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);

            b.Property<bool?>("IsHalfDay")
                .HasColumnType("bit");

            b.Property<int>("LeaveType")
                .HasColumnType("int");

            b.Property<DateTime?>("ModifiedAt")
                .HasColumnType("datetime2");

            b.Property<string>("ModifiedBy")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("Reason")
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            b.Property<DateTimeOffset>("RequestedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("datetimeoffset")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            b.Property<DateOnly>("StartDate")
                .HasColumnType("date");

            b.Property<int>("Status")
                .HasColumnType("int");

            b.Property<double>("TotalDays")
                .HasColumnType("float");

            b.HasKey("Id");

            b.HasIndex("EmployeeId", "StartDate");

            b.ToTable("LeaveRequests", (string)null);
        });

        modelBuilder.Entity("Payroll.Domain.Overtime.OvertimeRecord", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            b.Property<DateTimeOffset?>("ApprovedAt")
                .HasColumnType("datetimeoffset");

            b.Property<Guid?>("ApprovedById")
                .HasColumnType("uniqueidentifier");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("datetime2");

            b.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<DateOnly>("Date")
                .HasColumnType("date");

            b.Property<Guid>("EmployeeId")
                .HasColumnType("uniqueidentifier");

            b.Property<double>("Hours")
                .HasColumnType("float");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);

            b.Property<bool>("IsLockedForPayroll")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(false);

            b.Property<DateTime?>("ModifiedAt")
                .HasColumnType("datetime2");

            b.Property<string>("ModifiedBy")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("Reason")
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            b.Property<int>("Status")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasDefaultValue(1);

            b.Property<int>("Type")
                .HasColumnType("int");

            b.HasKey("Id");

            b.HasIndex("EmployeeId", "Date");

            b.ToTable("OvertimeRecords", (string)null);
        });

        modelBuilder.Entity("Payroll.Domain.Payroll.PayRun", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            b.Property<string>("Code")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("datetime2");

            b.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);

            b.Property<bool>("IsLocked")
                .HasColumnType("bit");

            b.Property<DateTime?>("ModifiedAt")
                .HasColumnType("datetime2");

            b.Property<string>("ModifiedBy")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("nvarchar(150)");

            b.Property<DateOnly>("PayDate")
                .HasColumnType("date");

            b.Property<DateOnly>("PeriodEnd")
                .HasColumnType("date");

            b.Property<DateOnly>("PeriodStart")
                .HasColumnType("date");

            b.Property<int>("PeriodType")
                .HasColumnType("int");

            b.Property<int>("Status")
                .HasColumnType("int");

            b.HasKey("Id");

            b.HasIndex("Code")
                .IsUnique();

            b.ToTable("PayRuns", (string)null);
        });

        modelBuilder.Entity("Payroll.Domain.Payroll.PaySlip", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            b.Property<decimal>("BasicSalary")
                .HasColumnType("decimal(18,2)");

            b.Property<string>("Currency")
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnType("nvarchar(10)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("datetime2");

            b.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<Guid>("EmployeeId")
                .HasColumnType("uniqueidentifier");

            b.Property<decimal>("EmployeeEpf")
                .HasColumnType("decimal(18,2)");

            b.Property<decimal>("EmployerEpf")
                .HasColumnType("decimal(18,2)");

            b.Property<decimal>("EmployerEtf")
                .HasColumnType("decimal(18,2)");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);

            b.Property<DateTime?>("ModifiedAt")
                .HasColumnType("datetime2");

            b.Property<string>("ModifiedBy")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<decimal>("NetPay")
                .HasColumnType("decimal(18,2)");

            b.Property<decimal>("PayeTax")
                .HasColumnType("decimal(18,2)");

            b.Property<Guid>("PayRunId")
                .HasColumnType("uniqueidentifier");

            b.Property<decimal>("TotalDeductions")
                .HasColumnType("decimal(18,2)");

            b.Property<decimal>("TotalEarnings")
                .HasColumnType("decimal(18,2)");

            b.HasKey("Id");

            b.HasIndex("PayRunId", "EmployeeId");

            b.HasOne("Payroll.Domain.Payroll.PayRun", "PayRun")
                .WithMany("PaySlips")
                .HasForeignKey("PayRunId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("PayRun");

            b.ToTable("PaySlips", (string)null);
        });

        modelBuilder.Entity("Payroll.Domain.Payroll.PaySlipDeductionLine", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            b.Property<decimal>("Amount")
                .HasColumnType("decimal(18,2)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("datetime2");

            b.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("Description")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);

            b.Property<bool>("IsPostTax")
                .HasColumnType("bit");

            b.Property<bool>("IsPreTax")
                .HasColumnType("bit");

            b.Property<DateTime?>("ModifiedAt")
                .HasColumnType("datetime2");

            b.Property<string>("ModifiedBy")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<Guid>("PaySlipId")
                .HasColumnType("uniqueidentifier");

            b.Property<string>("Code")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            b.HasKey("Id");

            b.HasIndex("PaySlipId");

            b.HasOne("Payroll.Domain.Payroll.PaySlip", "PaySlip")
                .WithMany("Deductions")
                .HasForeignKey("PaySlipId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("PaySlip");

            b.ToTable("PaySlipDeductionLines", (string)null);
        });

        modelBuilder.Entity("Payroll.Domain.Payroll.PaySlipEarningLine", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            b.Property<decimal>("Amount")
                .HasColumnType("decimal(18,2)");

            b.Property<string>("Code")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("datetime2");

            b.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("Description")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);

            b.Property<bool>("IsEpfApplicable")
                .HasColumnType("bit");

            b.Property<bool>("IsEtfApplicable")
                .HasColumnType("bit");

            b.Property<bool>("IsTaxable")
                .HasColumnType("bit");

            b.Property<DateTime?>("ModifiedAt")
                .HasColumnType("datetime2");

            b.Property<string>("ModifiedBy")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<Guid>("PaySlipId")
                .HasColumnType("uniqueidentifier");

            b.HasKey("Id");

            b.HasIndex("PaySlipId");

            b.HasOne("Payroll.Domain.Payroll.PaySlip", "PaySlip")
                .WithMany("Earnings")
                .HasForeignKey("PaySlipId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("PaySlip");

            b.ToTable("PaySlipEarningLines", (string)null);
        });

        modelBuilder.Entity("Payroll.Domain.PayrollConfig.AllowanceType", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            b.Property<int>("Basis")
                .HasColumnType("int");

            b.Property<string>("Code")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("datetime2");

            b.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("Description")
                .HasColumnType("nvarchar(max)");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);

            b.Property<bool>("IsEpfApplicable")
                .HasColumnType("bit");

            b.Property<bool>("IsEtfApplicable")
                .HasColumnType("bit");

            b.Property<bool>("IsTaxable")
                .HasColumnType("bit");

            b.Property<DateTime?>("ModifiedAt")
                .HasColumnType("datetime2");

            b.Property<string>("ModifiedBy")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.HasKey("Id");

            b.HasIndex("Code")
                .IsUnique();

            b.ToTable("AllowanceTypes", (string)null);
        });

        modelBuilder.Entity("Payroll.Domain.PayrollConfig.DeductionType", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            b.Property<int>("Basis")
                .HasColumnType("int");

            b.Property<string>("Code")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("datetime2");

            b.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("Description")
                .HasColumnType("nvarchar(max)");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);

            b.Property<bool>("IsPostTax")
                .HasColumnType("bit");

            b.Property<bool>("IsPreTax")
                .HasColumnType("bit");

            b.Property<DateTime?>("ModifiedAt")
                .HasColumnType("datetime2");

            b.Property<string>("ModifiedBy")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.HasKey("Id");

            b.HasIndex("Code")
                .IsUnique();

            b.ToTable("DeductionTypes", (string)null);
        });

        modelBuilder.Entity("Payroll.Domain.Payroll.PayRun", b =>
        {
            b.Navigation("PaySlips");
        });

        modelBuilder.Entity("Payroll.Domain.Payroll.PaySlip", b =>
        {
            b.Navigation("Deductions");

            b.Navigation("Earnings");
        });
#pragma warning restore 612, 618
    }
}
