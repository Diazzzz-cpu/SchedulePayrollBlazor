using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchedulePayrollBlazor.Data;

#nullable disable

namespace SchedulePayrollBlazor.Data.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.User", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            b.Property<DateTime>("CreatedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<string>("PasswordHash")
                .IsRequired()
                .HasColumnType("longtext")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<string>("Role")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("varchar(32)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.HasKey("Id");

            b.HasIndex("Email")
                .IsUnique();

            b.ToTable("Users");
        });

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.Employee", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            b.Property<string>("Department")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("varchar(120)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<string>("EmploymentType")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasMaxLength(60)
                .HasColumnType("varchar(60)")
                .HasDefaultValue("FullTime")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<string>("FirstName")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("varchar(120)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("tinyint(1)")
                .HasDefaultValue(true);

            b.Property<string>("JobTitle")
                .IsRequired()
                .HasMaxLength(160)
                .HasColumnType("varchar(160)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<string>("LastName")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("varchar(120)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<string>("Location")
                .IsRequired()
                .HasMaxLength(160)
                .HasColumnType("varchar(160)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<DateTime>("StartDate")
                .HasColumnType("datetime(6)");

            b.Property<int>("UserId")
                .HasColumnType("int");

            b.HasKey("Id");

            b.HasIndex("UserId")
                .IsUnique();

            b.ToTable("Employees");
        });

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.PayPeriod", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            b.Property<DateTime>("EndDate")
                .HasColumnType("date");

            b.Property<string>("PeriodName")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("varchar(120)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<string>("Status")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasMaxLength(40)
                .HasColumnType("varchar(40)")
                .HasDefaultValue("Open")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<DateTime>("StartDate")
                .HasColumnType("date");

            b.HasKey("Id");

            b.ToTable("PayPeriods");
        });

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.PayrollRun", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            b.Property<DateTime>("CreatedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            b.Property<int>("EmployeeId")
                .HasColumnType("int");

            b.Property<decimal>("GrossPay")
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0m);

            b.Property<decimal>("NetPay")
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0m);

            b.Property<int?>("PayPeriodId")
                .HasColumnType("int");

            b.Property<string>("Status")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasMaxLength(40)
                .HasColumnType("varchar(40)")
                .HasDefaultValue("Pending")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<decimal>("TotalDeductions")
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0m);

            b.HasKey("Id");

            b.HasIndex("EmployeeId");

            b.HasIndex("PayPeriodId");

            b.ToTable("PayrollRuns");
        });

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.Schedule", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            b.Property<int>("EmployeeId")
                .HasColumnType("int");

            b.Property<TimeSpan>("EndTime")
                .HasColumnType("time(6)");

            b.Property<string>("Source")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasMaxLength(60)
                .HasColumnType("varchar(60)")
                .HasDefaultValue("Manual")
                .HasAnnotation("MySql:CharSet", "utf8mb4");

            b.Property<DateTime>("ShiftDate")
                .HasColumnType("date");

            b.Property<TimeSpan>("StartTime")
                .HasColumnType("time(6)");

            b.HasKey("Id");

            b.HasIndex("EmployeeId");

            b.ToTable("Schedules");
        });

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.Employee", b =>
        {
            b.HasOne("SchedulePayrollBlazor.Data.Models.User", "User")
                .WithOne("Employee")
                .HasForeignKey("SchedulePayrollBlazor.Data.Models.Employee", "UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("User");
        });

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.PayrollRun", b =>
        {
            b.HasOne("SchedulePayrollBlazor.Data.Models.Employee", "Employee")
                .WithMany("PayrollRuns")
                .HasForeignKey("EmployeeId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("SchedulePayrollBlazor.Data.Models.PayPeriod", "PayPeriod")
                .WithMany("PayrollRuns")
                .HasForeignKey("PayPeriodId")
                .OnDelete(DeleteBehavior.SetNull);

            b.Navigation("Employee");

            b.Navigation("PayPeriod");
        });

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.Schedule", b =>
        {
            b.HasOne("SchedulePayrollBlazor.Data.Models.Employee", "Employee")
                .WithMany("Schedules")
                .HasForeignKey("EmployeeId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("Employee");
        });

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.Employee", b =>
        {
            b.Navigation("PayrollRuns");

            b.Navigation("Schedules");
        });

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.PayPeriod", b =>
        {
            b.Navigation("PayrollRuns");
        });

        modelBuilder.Entity("SchedulePayrollBlazor.Data.Models.User", b =>
        {
            b.Navigation("Employee");
        });
#pragma warning restore 612, 618
    }
}
