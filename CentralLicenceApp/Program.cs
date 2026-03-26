using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Add services to the container.
builder.Services.AddControllersWithViews();

// Repositories
builder.Services.AddScoped<IClientLicenseRepository>(_ => new ClientLicenseRepository(connStr));
builder.Services.AddScoped<ILicenseHistoryRepository>(_ => new LicenseHistoryRepository(connStr));
builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connStr));
builder.Services.AddScoped<IRoleRepository>(_ => new RoleRepository(connStr));
builder.Services.AddScoped<IEmployeeDepartmentRepository>(_ => new EmployeeDepartmentRepository(connStr));
builder.Services.AddScoped<IEmployeeDesignationRepository>(_ => new EmployeeDesignationRepository(connStr));
builder.Services.AddScoped<IEmployeeTypeRepository>(_ => new EmployeeTypeRepository(connStr));
builder.Services.AddScoped<ICompanySettingsRepository>(_ => new CompanySettingsRepository(connStr));
builder.Services.AddScoped<ILocationRepository>(_ => new LocationRepository(connStr));
builder.Services.AddScoped<IMailConfigRepository>(_ => new MailConfigRepository(connStr));
builder.Services.AddScoped<IEmailTemplateRepository>(_ => new EmailTemplateRepository(connStr));
builder.Services.AddScoped<IReminderRepository>(_ => new ReminderRepository(connStr));
builder.Services.AddScoped<IClientDetailsRepository>(_ => new ClientDetailsRepository(connStr));

// Email service
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

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
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("AllUsers",  policy => policy.RequireRole("Administrator", "Staff"));
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

app.Run();
