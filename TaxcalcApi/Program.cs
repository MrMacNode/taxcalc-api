using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TaxcalcApi.Api.Dtos;
using TaxcalcApi.Api.Endpoints;
using TaxcalcApi.Api.Middleware;
using TaxcalcApi.Api.Validators;
using TaxcalcApi.Core.Services;
using TaxcalcApi.Infrastructure.Configuration;
using TaxcalcApi.Infrastructure.Database.Repositories;

var builder = WebApplication.CreateSlimBuilder(args);

Serilog.Debugging.SelfLog.Enable(Console.WriteLine);


// Setting CORS to allow all origins. 
// - This is a public read-only API
// - Although we have a UI in mind, other clients may wish to use it.
// - No business rules restricting usage have been provided.
// - If any of this changes, we can add a more restrictive CORS policy.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
          .WithMethods("GET")
          .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication();

builder.Services.Configure<RouteOptions>(options =>
{
    options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

builder.Services.Configure<JsonOptions>(static options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Add(
        new DefaultJsonTypeInfoResolver());
});

builder.ConfigureLogging();

builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.PropertyNameCaseInsensitive = true;
    opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.ConfigureDatabase();
builder.Services.AddScoped<IIncomeTaxCalculator, IncomeTaxCalculator>();
builder.Services.AddSingleton<IValidator<IncomeTaxQueryModel>, IncomeTaxQueryModelValidator>();

builder.WebHost.UseKestrel(options => options.ListenAnyIP(7149));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseCors();

app.UseMiddleware<ResponseLoggingMiddleware>();

app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapIncomeTaxEndpoints();

await app.RunAsync();
