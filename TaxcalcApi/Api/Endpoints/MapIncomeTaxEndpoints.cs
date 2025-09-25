using TaxcalcApi.Api.Handlers;
using TaxcalcApi.Core.Models;

namespace TaxcalcApi.Api.Endpoints
{
    public static class IncomeTaxEndpoints
    {
        /// <summary>
        /// Mappings for income tax endpoints.
        /// </summary>
        /// <remarks>
        ///     - This method sets up the routing for income tax calculation endpoints. (Currently only one)
        ///     - /uk endpoint calculates UK income tax based on an annual salary provided as a query parameter.
        ///     - We could add /us, /ca, /au etc. endpoints in the future if needed.
        /// </remarks>
        public static IEndpointRouteBuilder MapIncomeTaxEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/incometax/uk", IncomeTaxHandlers.GetUkIncomeTax)
                .WithName("GetUkIncomeTax")
                .WithSummary("Calculates UK Income Tax")
                .WithDescription("Calculates the UK income tax based on the provided annual salary, with monthly figures.")
                .WithTags("IncomeTaxUk")
                .Produces<IncomeTaxResult>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            return app;
        }
    }
}
