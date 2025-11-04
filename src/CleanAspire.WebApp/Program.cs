// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using CleanAspire.ClientApp;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using CleanAspire.Application;
using CleanAspire.Infrastructure;
using CleanAspire.Infrastructure.Persistence;
using CleanAspire.Domain.Identities;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();


builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddHttpClients(builder.Configuration);
builder.Services.AddAuthenticationAndLocalization(builder.Configuration);

// Add Infrastructure and Application services (including reminder services)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Add ASP.NET Core Identity with Authentication
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

// Add Authorization
builder.Services.AddAuthorizationBuilder();

// Add localization support
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Configure request localization to support Portuguese (Brazil)
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("pt-BR"),
        new CultureInfo("en-US"),
        new CultureInfo("es-ES"),
        new CultureInfo("fr-FR"),
        new CultureInfo("de-DE"),
        new CultureInfo("ja-JP"),
        new CultureInfo("zh-CN"),
        new CultureInfo("ko-KR")
    };

    options.DefaultRequestCulture = new RequestCulture("pt-BR", "pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Configure culture providers
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider
    {
        Options = options
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Add localization middleware
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(localizationOptions.Value);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<CleanAspire.WebApp.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CleanAspire.ClientApp._Imports).Assembly);

app.Run();
