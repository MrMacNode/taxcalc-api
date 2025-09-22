using FluentValidation;
using Serilog;
using TaxcalcApi.Api.Dtos;
using TaxcalcApi.Core.Services;

namespace TaxcalcApi.Api.Handlers
{
    public static class IncomeTaxHandlers
    {
        public static async Task<IResult> GetUkIncomeTax(
            ILogger<IncomeTaxQueryModel> logger,
            IIncomeTaxCalculator incomeTaxCalculator,
            IValidator<IncomeTaxQueryModel> validator,
            [AsParameters] IncomeTaxQueryModel parameters
            )
        {
            try
            {
                logger.LogInformation("Received request to calculate UK income tax for annual salary: {AnnualSalary}", parameters.AnnualSalaryString);
                var validationResult = await validator.ValidateAsync(parameters);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Validation failed for annual salary: {AnnualSalary}. Errors: {Errors}",
                        parameters.AnnualSalaryString,
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                    var errors = validationResult.Errors
                        .Select(e => new { e.PropertyName, e.ErrorMessage })
                        .ToList();
                    return Results.BadRequest(new { Errors = errors });
                }
                var result = await incomeTaxCalculator.CalculateUkAnnual(parameters.AnnualSalary);
                logger.LogInformation("Successfully calculated UK income tax for annual salary.");
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while calculating UK income tax for annual salary: {AnnualSalary}", parameters.AnnualSalaryString);
                return Results.Problem("An internal server error occurred while processing your request.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
