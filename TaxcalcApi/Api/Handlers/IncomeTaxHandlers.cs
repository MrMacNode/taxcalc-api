using FluentValidation;
using Serilog;
using TaxcalcApi.Api.Dtos;
using TaxcalcApi.Core.Services;

namespace TaxcalcApi.Api.Handlers
{
    public static class IncomeTaxHandlers
    {
        /// <summary>
        /// Manages requests to calculate UK income tax based on an annual salary.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="incomeTaxCalculator">Service layer component to perform the calculations.</param>
        /// <param name="validator">A validator for parameters.</param>
        /// <param name="parameters">Packaged parameters against which to perform the calculations.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<IResult> GetUkIncomeTax(
            ILogger<IncomeTaxQueryModel> logger,
            IIncomeTaxCalculator incomeTaxCalculator,
            IValidator<IncomeTaxQueryModel> validator,
            [AsParameters] IncomeTaxQueryModel parameters, 
            CancellationToken cancellationToken
            )
        {
            try
            {
                logger.LogInformation("Received request to calculate UK income tax for annual salary: {AnnualSalary}", parameters.AnnualSalaryString);

                var validationResult = await validator.ValidateAsync(parameters, cancellationToken);
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

                var result = await incomeTaxCalculator.CalculateUkAnnual(parameters.AnnualSalary, cancellationToken);
                logger.LogInformation("Successfully calculated UK income tax for annual salary.");
                return Results.Ok(result);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Request was cancelled by the client.");
                return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
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
