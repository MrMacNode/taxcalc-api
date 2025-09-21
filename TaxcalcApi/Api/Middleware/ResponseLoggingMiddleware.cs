using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TaxcalcApi.Api.Middleware;

public class ResponseLoggingMiddleware(RequestDelegate _next, ILogger<ResponseLoggingMiddleware> _logger)
{
    public async Task Invoke(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        if (!string.IsNullOrWhiteSpace(text) && context.Response.ContentType?.Contains("application/json") == true)
        {
            _logger.LogInformation("Taxcalc API responded: {ResponseBody}", text);
        }

        await responseBody.CopyToAsync(originalBodyStream);
    }
}