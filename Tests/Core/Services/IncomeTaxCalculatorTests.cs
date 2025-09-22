using Moq;
using TaxcalcApi.Core.Services;
using TaxcalcApi.Infrastructure.Database.Entities;
using TaxcalcApi.Infrastructure.Database.Repositories;

namespace Tests.Core.Services;

public class IncomeTaxCalculatorTests
{
    [Theory]
    [InlineData(
        10000, // annualSalary
        10000, // expectedGrossAnnualSalary
        833.33, // expectedGrossMonthlySalary
        9000, // expectedNetAnnualSalary
        750, // expectedNetMonthlySalary
        1000, // expectedAnnualTaxPaid
        83.33 // expectedMonthlyTaxPaid
    )]
    [InlineData(
        40000,
        40000,
        3333.33,
        29000,
        2416.67,
        11000,
        916.67
    )]
    public async Task CalculateUkAnnual_ReturnsExpectedResult(
        decimal annualSalary,
        decimal expectedGrossAnnualSalary,
        decimal expectedGrossMonthlySalary,
        decimal expectedNetAnnualSalary,
        decimal expectedNetMonthlySalary,
        decimal expectedAnnualTaxPaid,
        decimal expectedMonthlyTaxPaid)
    {
        // Arrange: Setup mock tax bands for a simple progressive tax system
        var taxBands = new List<TaxBand>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LowerLimit = 0,
                UpperLimit = 5000,
                Rate = 0
            },
            new()
            {
                Id = Guid.NewGuid(),
                LowerLimit = 5000,
                UpperLimit = 20000,
                Rate = 20
            },
            new()
            {
                Id = Guid.NewGuid(),
                LowerLimit = 20000,
                Rate = 40
            },
        };

        var cancellationToken = new CancellationToken();

        var mockGetTaxBands = new Mock<ITaxBandRepository>();
        mockGetTaxBands
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxBands);

        var calculator = new IncomeTaxCalculator(mockGetTaxBands.Object);

        // Act
        var result = await calculator.CalculateUkAnnual(annualSalary, cancellationToken);

        // Assert
        Assert.Equal(expectedGrossAnnualSalary, result.GrossAnnualSalary);
        Assert.Equal(expectedGrossMonthlySalary, result.GrossMonthlySalary);
        Assert.Equal(expectedNetAnnualSalary, result.NetAnnualSalary);
        Assert.Equal(expectedNetMonthlySalary, result.NetMonthlySalary);
        Assert.Equal(expectedAnnualTaxPaid, result.AnnualTaxPaid);
        Assert.Equal(expectedMonthlyTaxPaid, result.MonthlyTaxPaid);
    }
}