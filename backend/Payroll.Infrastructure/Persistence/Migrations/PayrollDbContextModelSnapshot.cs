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
#pragma warning restore 612, 618
    }
}
