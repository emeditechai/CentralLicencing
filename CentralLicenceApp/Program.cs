using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using CentralLicenceApp.Hubs;
using CentralLicenceApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.Configure<PushNotificationSettings>(builder.Configuration.GetSection("PushNotifications"));

// Repositories
builder.Services.AddScoped<IClientLicenseRepository>(_ => new ClientLicenseRepository(connStr));
builder.Services.AddScoped<ILicenseHistoryRepository>(_ => new LicenseHistoryRepository(connStr));
builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connStr));
builder.Services.AddScoped<IRoleRepository>(_ => new RoleRepository(connStr));
builder.Services.AddScoped<IEmployeeDepartmentRepository>(_ => new EmployeeDepartmentRepository(connStr));
builder.Services.AddScoped<IEmployeeDesignationRepository>(_ => new EmployeeDesignationRepository(connStr));
builder.Services.AddScoped<IEmployeeTypeRepository>(_ => new EmployeeTypeRepository(connStr));
builder.Services.AddScoped<IExpenseCategoryRepository>(_ => new ExpenseCategoryRepository(connStr));
builder.Services.AddScoped<IExpenseRequestRepository>(_ => new ExpenseRequestRepository(connStr));
builder.Services.AddScoped<ICompanySettingsRepository>(_ => new CompanySettingsRepository(connStr));
builder.Services.AddScoped<ILocationRepository>(_ => new LocationRepository(connStr));
builder.Services.AddScoped<IMailConfigRepository>(_ => new MailConfigRepository(connStr));
builder.Services.AddScoped<IEmailTemplateRepository>(_ => new EmailTemplateRepository(connStr));
builder.Services.AddScoped<IReminderRepository>(_ => new ReminderRepository(connStr));
builder.Services.AddScoped<IClientDetailsRepository>(_ => new ClientDetailsRepository(connStr));
builder.Services.AddScoped<IUserPushSubscriptionRepository>(_ => new UserPushSubscriptionRepository(connStr));

// Email service
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IExpenseBrowserNotificationService, ExpenseBrowserNotificationService>();
builder.Services.AddTransient<IClaimsTransformation, AdminClaimsTransformation>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AdminAuthorizationMiddlewareResultHandler>();
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

// Background reminder service
builder.Services.AddHostedService<ExpiryReminderService>();

// Seeder
builder.Services.AddTransient<DatabaseSeeder>();

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath  = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdValue, out var userId))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var user = await userRepository.GetByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var activeRoleIdValue = context.Principal?.FindFirstValue("ActiveRoleId");
                if (!int.TryParse(activeRoleIdValue, out var activeRoleId)
                    || !user.AssignedRoleIds.Contains(activeRoleId))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("AllUsers",  policy => policy.RequireRole("Administrator", "Staff", "Finance"));
});

var app = builder.Build();

// Run DB seeder at startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapHub<ExpenseNotificationHub>("/hubs/expense-notifications");

app.Run();
