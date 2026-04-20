using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<Salon_LeHoang.Models.SalonLeHoangContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("UserAuth")
    .AddCookie("UserAuth", config =>
    {
        config.Cookie.Name = "Salon.Auth";
        config.LoginPath = "/Auth/Login";
        config.AccessDeniedPath = "/Auth/AccessDenied";
        config.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

// Tự động tạo tài khoản Admin mặc định nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Salon_LeHoang.Models.SalonLeHoangContext>();
    try 
    {
        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            context.Users.Add(new Salon_LeHoang.Models.User
            {
                FullName = "Administrator",
                PhoneNumber = "admin", // Tên đăng nhập
                PasswordHash = "admin123", // Mật khẩu
                Role = "Admin",
                Points = 0,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }
    }
    catch 
    { 
        // Bỏ qua lỗi nếu database chưa được tạo
    }
}

app.Run();
