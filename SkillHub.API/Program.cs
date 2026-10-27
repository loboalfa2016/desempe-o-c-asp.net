

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SkillHub.Application.Interfaces;
using SkillHub.Infrastructure.Identity;
using SkillHub.Infrastructure.Persistence;
using SkillHub.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "No se ha configurado Jwt:Key.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWorkshopService, WorkshopService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    await IdentitySeeder.SeedRolesAsync(roleManager);

    var employeeEmail =
        builder.Configuration["SKILLHUB_EMPLOYEE_EMAIL"];

    var employeePassword =
        builder.Configuration["SKILLHUB_EMPLOYEE_PASSWORD"];

    var employeeName =
        builder.Configuration["SKILLHUB_EMPLOYEE_NAME"];

    if (!string.IsNullOrWhiteSpace(employeeEmail) &&
        !string.IsNullOrWhiteSpace(employeePassword) &&
        !string.IsNullOrWhiteSpace(employeeName))
    {
        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        await IdentitySeeder.SeedEmployeeAsync(
            userManager,
            employeeEmail,
            employeePassword,
            employeeName);
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
