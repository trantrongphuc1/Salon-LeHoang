using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Salon_LeHoang.Models;

public partial class SalonLeHoangContext : DbContext
{
    public SalonLeHoangContext()
    {
    }

    public SalonLeHoangContext(DbContextOptions<SalonLeHoangContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }
    public virtual DbSet<AppointmentDetail> AppointmentDetails { get; set; }
    public virtual DbSet<Attendance> Attendances { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Invoice> Invoices { get; set; }
    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }
    public virtual DbSet<PointHistory> PointHistories { get; set; }
    public virtual DbSet<Service> Services { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId);
            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.User).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AppointmentDetail>(entity =>
        {
            entity.HasKey(e => e.AppointmentDetailId);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentDetails)
                .HasForeignKey(d => d.AppointmentId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Service).WithMany(p => p.AppointmentDetails)
                .HasForeignKey(d => d.ServiceId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId);
            entity.HasIndex(e => new { e.EmployeeId, e.AttendanceMonth, e.AttendanceYear }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmployeeId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.BaseSalary).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.CommissionRate).HasColumnType("decimal(5, 2)").HasDefaultValue(3m);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.FinalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EarnedPoints).HasDefaultValue(0);
            entity.Property(e => e.PointsUsed).HasDefaultValue(0);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50).HasDefaultValue("Tiền mặt");
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.InvoiceDetailId);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Service).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.ServiceId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Employee).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.EmployeeId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PointHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.User).WithMany(p => p.PointHistories)
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Invoice).WithMany(p => p.PointHistories)
                .HasForeignKey(d => d.InvoiceId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.DurationMinutes).HasDefaultValue(30);
            entity.Property(e => e.ImageUrl).HasMaxLength(500).IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Role).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Customer");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
