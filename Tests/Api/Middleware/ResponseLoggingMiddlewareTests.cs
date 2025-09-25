using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using TaxcalcApi.Api.Middleware;

namespace TaxcalcApi.Tests.Api.Middleware;

public class ResponseLoggingMiddlewareTests
{
    [Fact]
    public async Task TestMiddlewareResponseLogging()
    {
        // Arrange
        var responseBody = """
            { "grossAnnualSalary": 60000, "netAnnualSalary": 45000}
        """;
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Response.ContentType = "application/json";

        // Write a response in the pipeline
        var next = new RequestDelegate(async ctx =>
        {
            var bytes = Encoding.UTF8.GetBytes(responseBody);
            await ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        });

        var loggerMock = new Mock<ILogger<ResponseLoggingMiddleware>>();

        var middleware = new ResponseLoggingMiddleware(next, loggerMock.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(responseBody)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}