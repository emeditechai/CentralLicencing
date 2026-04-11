using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using CentralLicenceApp.Hubs;
using CentralLicenceApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<PushNotificationSettings>(builder.Configuration.GetSection("PushNotifications"));

// Repositories
builder.Services.AddScoped<IClientLicenseRepository>(_ => new ClientLicenseRepository(connStr));
builder.Services.AddScoped<IClientLicenseAuditLogRepository>(_ => new ClientLicenseAuditLogRepository(connStr));
builder.Services.AddScoped<ILicenseHistoryRepository>(_ => new LicenseHistoryRepository(connStr));
builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connStr));
builder.Services.AddScoped<IRoleRepository>(_ => new RoleRepository(connStr));
builder.Services.AddScoped<IEmployeeDepartmentRepository>(_ => new EmployeeDepartmentRepository(connStr));
builder.Services.AddScoped<IEmployeeDesignationRepository>(_ => new EmployeeDesignationRepository(connStr));
builder.Services.AddScoped<IEmployeeTypeRepository>(_ => new EmployeeTypeRepository(connStr));
builder.Services.AddScoped<IPricingModelRepository>(_ => new PricingModelRepository(connStr));
builder.Services.AddScoped<IExpenseCategoryRepository>(_ => new ExpenseCategoryRepository(connStr));
builder.Services.AddScoped<ITermsConditionTemplateRepository>(_ => new TermsConditionTemplateRepository(connStr));
builder.Services.AddScoped<IProductMasterRepository>(_ => new ProductMasterRepository(connStr));
builder.Services.AddScoped<IProductRateRepository>(_ => new ProductRateRepository(connStr));
builder.Services.AddScoped<IProductRateDiscountRepository>(_ => new ProductRateDiscountRepository(connStr));
builder.Services.AddScoped<IExpenseRequestRepository>(_ => new ExpenseRequestRepository(connStr));
builder.Services.AddScoped<ICompanySettingsRepository>(_ => new CompanySettingsRepository(connStr));
builder.Services.AddScoped<ILocationRepository>(_ => new LocationRepository(connStr));
builder.Services.AddScoped<IMailConfigRepository>(_ => new MailConfigRepository(connStr));
builder.Services.AddScoped<IEmailTemplateRepository>(_ => new EmailTemplateRepository(connStr));
builder.Services.AddScoped<IEmailLogRepository>(_ => new EmailLogRepository(connStr));
builder.Services.AddScoped<IReminderRepository>(_ => new ReminderRepository(connStr));
builder.Services.AddScoped<IClientDetailsRepository>(_ => new ClientDetailsRepository(connStr));
builder.Services.AddScoped<IReportRepository>(_ => new ReportRepository(connStr));
builder.Services.AddScoped<IPartyMasterRepository>(_ => new PartyMasterRepository(connStr));
builder.Services.AddScoped<IQuotationRepository>(_ => new QuotationRepository(connStr));
builder.Services.AddScoped<IInvoiceRepository>(_ => new InvoiceRepository(connStr));
builder.Services.AddScoped<IBankMasterRepository>(_ => new BankMasterRepository(connStr));
builder.Services.AddScoped<IPaymentModeRepository>(_ => new PaymentModeRepository(connStr));
builder.Services.AddScoped<IInvoicePaymentRepository>(_ => new InvoicePaymentRepository(connStr));
builder.Services.AddScoped<IInvoiceRefundRepository>(_ => new InvoiceRefundRepository(connStr));
builder.Services.AddScoped<ICreditNoteRepository>(_ => new CreditNoteRepository(connStr));
builder.Services.AddScoped<ITicketCategoryRepository>(_ => new TicketCategoryRepository(connStr));
builder.Services.AddScoped<ITicketSubCategoryRepository>(_ => new TicketSubCategoryRepository(connStr));
builder.Services.AddScoped<ITicketPriorityRepository>(_ => new TicketPriorityRepository(connStr));
builder.Services.AddScoped<IHelpDeskTicketRepository>(_ => new HelpDeskTicketRepository(connStr));
builder.Services.AddScoped<ITicketReportRepository>(_ => new TicketReportRepository(connStr));
builder.Services.AddScoped<IClientDetailsReportExportService, ClientDetailsReportExportService>();
builder.Services.AddScoped<IExpenseReportExportService, ExpenseReportExportService>();
builder.Services.AddScoped<ISettlementReportExportService, SettlementReportExportService>();
builder.Services.AddScoped<ITicketReportExportService, TicketReportExportService>();
builder.Services.AddScoped<IUserPushSubscriptionRepository>(_ => new UserPushSubscriptionRepository(connStr));

// Email service
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<ITicketEmailService>(sp =>
    new TicketEmailService(
        sp.GetRequiredService<IEmailService>(),
        sp.GetRequiredService<ICompanySettingsRepository>(),
        connStr,
        sp.GetRequiredService<ILogger<TicketEmailService>>()));
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddSingleton<IBrowserProvider, BrowserProvider>();
builder.Services.AddScoped<IDocumentPdfService, DocumentPdfService>();
builder.Services.AddScoped<IExpenseBrowserNotificationService, ExpenseBrowserNotificationService>();
builder.Services.AddScoped<ITicketBrowserNotificationService>(sp =>
    new TicketBrowserNotificationService(
        sp.GetRequiredService<IUserPushSubscriptionRepository>(),
        sp.GetRequiredService<IHubContext<TicketNotificationHub>>(),
        sp.GetRequiredService<IOptions<PushNotificationSettings>>(),
        connStr,
        sp.GetRequiredService<ILogger<TicketBrowserNotificationService>>()));
builder.Services.AddTransient<IClaimsTransformation, AdminClaimsTransformation>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AdminAuthorizationMiddlewareResultHandler>();
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

// Background reminder service
builder.Services.AddHostedService<ExpiryReminderService>();

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
app.MapHub<TicketNotificationHub>("/hubs/ticket-notifications");

// Warm up Chromium in the background so the first PDF request is fast.
_ = app.Services.GetRequiredService<IBrowserProvider>().WarmUpAsync();

app.Run();
