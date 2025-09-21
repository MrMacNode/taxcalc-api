

using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TaxcalcApi.Api.Dtos;
using TaxcalcApi.Api.Endpoints;
using TaxcalcApi.Api.Validators;
using TaxcalcApi.Core.Services;
using TaxcalcApi.Infrastructure.Database.Repositories;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.Configure<RouteOptions>(options =>
{
    options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

builder.Services.Configure<JsonOptions>(static options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Add(
        new DefaultJsonTypeInfoResolver());
});

//TODO: Add logging

 


builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.PropertyNameCaseInsensitive = true;
    opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<ITaxBandRepository, TaxBandRepository>();
builder.Services.AddScoped<IIncomeTaxCalculator, IncomeTaxCalculator>();
builder.Services.AddScoped<IValidator<IncomeTaxQueryModel>, IncomeTaxQueryModelValidator>();
//TODO: Add validators

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//TODO: Add endpoints
app.MapIncomeTaxEndpoints();

await app.RunAsync();
