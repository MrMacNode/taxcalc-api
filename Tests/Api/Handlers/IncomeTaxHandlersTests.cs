using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxcalcApi.Api.Dtos;
using TaxcalcApi.Api.Handlers;
using TaxcalcApi.Core.Models;
using TaxcalcApi.Core.Services;
using Xunit;

namespace TaxcalcApi.Tests.Api.Handlers;

public class IncomeTaxHandlersTests
{
    private readonly Mock<ILogger<IncomeTaxQueryModel>> loggerMock = new();
    private readonly Mock<IIncomeTaxCalculator> calculatorMock = new();
    private readonly Mock<IValidator<IncomeTaxQueryModel>> validatorMock = new();

    private static IncomeTaxQueryModel CreateValidQueryModel() => new() { AnnualSalaryString = "50000" };

    [Fact]
    public async Task ReturnsOkOnValidInput()
    {
        // Arrange
        var parameters = CreateValidQueryModel();
        var validationResult = new ValidationResult();
        var expectedResult = new IncomeTaxResult { GrossAnnualSalary = 50000m };

        validatorMock.Setup(v => v.ValidateAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        calculatorMock.Setup(c => c.CalculateUkAnnual(parameters.AnnualSalary, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await IncomeTaxHandlers.GetUkIncomeTax(
            loggerMock.Object,
            calculatorMock.Object,
            validatorMock.Object,
            parameters,
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IncomeTaxResult>>(result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task ReturnsBadRequestOnValidationError()
    {
        // Arrange
        var parameters = CreateValidQueryModel();
        var errors = new List<ValidationFailure>
        {
            new("AnnualSalary", "Annual salary is required")
        };
        var validationResult = new ValidationResult(errors);

        validatorMock.Setup(v => v.ValidateAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await IncomeTaxHandlers.GetUkIncomeTax(
            loggerMock.Object,
            calculatorMock.Object,
            validatorMock.Object,
            parameters,
            CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<IEnumerable<string>>>(result);
        var responseErrors = badRequest.Value;
        Assert.NotNull(responseErrors);
        Assert.Single(responseErrors);
        Assert.Equal("Annual salary is required", responseErrors.First());
    }

    [Fact]
    public async Task Returns499OnCancellation()
    {
        // Arrange
        var parameters = CreateValidQueryModel();
        var validationResult = new ValidationResult();

        validatorMock.Setup(v => v.ValidateAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        calculatorMock.Setup(c => c.CalculateUkAnnual(parameters.AnnualSalary, It.IsAny<CancellationToken>()))
            .Returns(async (decimal _, CancellationToken ct) =>
            {
                await Task.Delay(1000, ct);
                return new IncomeTaxResult();
            });

        using var cts = new CancellationTokenSource();
        var task = IncomeTaxHandlers.GetUkIncomeTax(
            loggerMock.Object,
            calculatorMock.Object,
            validatorMock.Object,
            parameters,
            cts.Token);

        cts.Cancel();

        var result = await task;

        // Assert
        var statusResult = Assert.IsType<StatusCodeHttpResult>(result);
        Assert.Equal(499, statusResult.StatusCode);
    }

    [Fact]
    public async Task Returns500OnException()
    {
        // Arrange
        var parameters = CreateValidQueryModel();
        var validationResult = new ValidationResult();

        validatorMock.Setup(v => v.ValidateAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        calculatorMock.Setup(c => c.CalculateUkAnnual(parameters.AnnualSalary, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Calculation failed"));

        // Act
        var result = await IncomeTaxHandlers.GetUkIncomeTax(
            loggerMock.Object,
            calculatorMock.Object,
            validatorMock.Object,
            parameters,
            CancellationToken.None);

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal("An internal server error occurred while processing your request.", problemResult.ProblemDetails.Detail?.ToString());
        Assert.Equal(StatusCodes.Status500InternalServerError, problemResult.StatusCode);
    }
}