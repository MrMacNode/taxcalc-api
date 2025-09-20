using FluentValidation;
using TaxcalcApi.Api.Dtos;
using TaxcalcApi.Core.Services;

namespace TaxcalcApi.Api.Handlers
{
    public static class IncomeTaxHandlers
    {
        public static async Task<IResult> GetUkIncomeTax(
            IIncomeTaxCalculator incomeTaxCalculator,
            IValidator<IncomeTaxQueryModel> validator,
            [AsParameters] IncomeTaxQueryModel parameters
            )
        {
            try
            {
                var validationResult = await validator.ValidateAsync(parameters);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .Select(e => new { e.PropertyName, e.ErrorMessage })
                        .ToList();
                    return Results.BadRequest(new { Errors = errors });
                }
                var result = await incomeTaxCalculator.CalculateUkAnnual(parameters.AnnualSalary);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem("An internal server error occurred while processing your request.");
            }
        }
    }
}
