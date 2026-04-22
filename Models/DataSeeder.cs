using Microsoft.EntityFrameworkCore;
using Salon_LeHoang.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Salon_LeHoang.Models
{
    public static class DataSeeder
    {
        public static void Seed(SalonLeHoangContext context)
        {
            // Seed Admin
            if (!context.Users.Any(u => u.Role == "Admin"))
            {
                context.Users.Add(new User
                {
                    FullName = "Administrator",
                    PhoneNumber = "admin",
                    PasswordHash = "admin123",
                    Role = "Admin",
                    Points = 0,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
            }

            // Seed Employees
            if (!context.Employees.Any())
            {
                context.Employees.AddRange(
                    new Employee { FullName = "Nguyễn Văn Thợ", PhoneNumber = "0988111222", Position = "Thợ chính", BaseSalary = 6000000, CommissionRate = 5, IsActive = true, CreatedAt = DateTime.Now },
                    new Employee { FullName = "Trần Thị Cắt", PhoneNumber = "0988111223", Position = "Thợ cắt", BaseSalary = 5000000, CommissionRate = 4, IsActive = true, CreatedAt = DateTime.Now },
                    new Employee { FullName = "Lê Văn Gội", PhoneNumber = "0988111224", Position = "Thợ gội/massage", BaseSalary = 4000000, CommissionRate = 3, IsActive = true, CreatedAt = DateTime.Now },
                    new Employee { FullName = "Phạm Uyển Nhuộm", PhoneNumber = "0988111225", Position = "Thợ hóa chất", BaseSalary = 7000000, CommissionRate = 5, IsActive = true, CreatedAt = DateTime.Now },
                    new Employee { FullName = "Hoàng Tạo Kiểu", PhoneNumber = "0988111226", Position = "Stylist", BaseSalary = 8000000, CommissionRate = 5, IsActive = true, CreatedAt = DateTime.Now }
                );
                context.SaveChanges();
            }

            // Seed Services
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service { ServiceCode = "CUT-NAM", ServiceName = "Cắt tóc nam", Description = "Cắt tóc nam theo yêu cầu", Price = 50000, DurationMinutes = 30, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "CUT-NU", ServiceName = "Cắt tóc nữ", Description = "Cắt tạo dáng tóc nữ", Price = 100000, DurationMinutes = 45, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "WASH", ServiceName = "Gội đầu thư giãn", Description = "Gội đầu massage mặt", Price = 80000, DurationMinutes = 40, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "COMBO-NAM", ServiceName = "Combo Cắt + Gội nam", Description = "Cắt, gội, sấy tạo kiểu nam", Price = 100000, DurationMinutes = 60, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "COLOR-NAM", ServiceName = "Nhuộm tóc nam", Description = "Nhuộm tóc thời trang nam", Price = 200000, DurationMinutes = 60, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "COLOR-NU", ServiceName = "Nhuộm tóc nữ", Description = "Nhuộm tóc thời trang nữ không tẩy", Price = 500000, DurationMinutes = 90, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "COLOR-BLEACH", ServiceName = "Tẩy & Nhuộm nữ", Description = "Tẩy tóc và nhuộm màu sáng", Price = 1000000, DurationMinutes = 180, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "PERM-DIGI", ServiceName = "Uốn tóc KTS", Description = "Uốn tóc kỹ thuật số chuẩn Hàn", Price = 600000, DurationMinutes = 150, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "PERM-COLD", ServiceName = "Uốn lạnh nam", Description = "Uốn phồng chân tóc", Price = 250000, DurationMinutes = 60, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "STRAIGHT", ServiceName = "Duỗi tóc", Description = "Duỗi tóc suôn mượt tự nhiên", Price = 450000, DurationMinutes = 120, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "RECOVER-1", ServiceName = "Phục hồi Keratin", Description = "Phục hồi tóc bằng Keratin cao cấp", Price = 500000, DurationMinutes = 60, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "RECOVER-2", ServiceName = "Hấp dầu mượt tóc", Description = "Hấp dầu lạnh", Price = 200000, DurationMinutes = 45, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "NANO", ServiceName = "Phủ bóng Nano", Description = "Phủ bóng giữ màu tóc nhuộm", Price = 400000, DurationMinutes = 60, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "STYLE-NU", ServiceName = "Sấy tạo kiểu nữ", Description = "Sấy tạo kiểu dự tiệc", Price = 150000, DurationMinutes = 45, IsActive = true, CreatedAt = DateTime.Now },
                    new Service { ServiceCode = "BEARD", ServiceName = "Cạo mặt lấy ráy tai", Description = "Chăm sóc trọn gói nam", Price = 60000, DurationMinutes = 30, IsActive = true, CreatedAt = DateTime.Now }
                );
                context.SaveChanges();
            }

            // Seed Customers
            if (context.Users.Count(u => u.Role == "Customer") < 10)
            {
                for (int i = 1; i <= 10; i++)
                {
                    string phone = "09000000" + i.ToString("D2");
                    if (!context.Users.Any(u => u.PhoneNumber == phone))
                    {
                        context.Users.Add(new User
                        {
                            FullName = "Khách Hàng " + i,
                            PhoneNumber = phone,
                            PasswordHash = "customer",
                            Role = "Customer",
                            Points = 0,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
                context.SaveChanges();
            }
        }
    }
}
