using TaxcalcApi.Api.Handlers;
using TaxcalcApi.Core.Models;

namespace TaxcalcApi.Api.Endpoints
{
    public static class IncomeTaxEndpoints
    {
        public static IEndpointRouteBuilder MapIncomeTaxEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/incometax/uk", IncomeTaxHandlers.GetUkIncomeTax)
            .WithName("GetUkIncomeTax")
            .WithSummary("Calculates UK Income Tax")
            .WithDescription("Calculates the UK income tax based on the provided annual salary, with monthly figures.")
            .WithTags("IncomeTaxUk")
            .Produces<IncomeTaxResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
            //.WithOpenApi(op =>
            //{
            //    op.Summary = "Calculate UK Income Tax";
            //    op.Description = "Calculates the UK income tax based on the provided annual salary.";
            //    return op;
            //});
            return app;
        }
    }
}
