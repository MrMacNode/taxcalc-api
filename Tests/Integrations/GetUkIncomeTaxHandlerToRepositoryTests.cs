using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxcalcApi.Api.Dtos;
using TaxcalcApi.Api.Handlers;
using TaxcalcApi.Api.Validators;
using TaxcalcApi.Core.Models;
using TaxcalcApi.Core.Services;
using TaxcalcApi.Infrastructure.Database.Entities;
using TaxcalcApi.Infrastructure.Database.Repositories;
using static Dapper.SqlMapper;

namespace Tests.Integrations
{
    /// <summary>
    /// Integration tests which mock the database connection factory, but otherwise use implemented objects from handler to repository.
    /// </summary>
    
    public class GetUkIncomeTaxHandlerToRepositoryTests
    {
        [Theory]
        [InlineData(
            "10000", // annualSalary
            10000, // expectedGrossAnnualSalary
            833.33, // expectedGrossMonthlySalary
            9000, // expectedNetAnnualSalary
            750, // expectedNetMonthlySalary
            1000, // expectedAnnualTaxPaid
            83.33 // expectedMonthlyTaxPaid
        )]
        [InlineData(
            "40000",
            40000,
            3333.33,
            29000,
            2416.67,
            11000,
            916.67
        )]
        public async Task GetUkIncomeTaxHandlerHappyPath(
            string annualSalary,
            decimal expectedGrossAnnualSalary,
            decimal expectedGrossMonthlySalary,
            decimal expectedNetAnnualSalary,
            decimal expectedNetMonthlySalary,
            decimal expectedAnnualTaxPaid,
            decimal expectedMonthlyTaxPaid)
        {
            // Arrange/Act
            var result = await RunGetUkIncomeTax(annualSalary);

            // Assert
            var okResult = Assert.IsType<Ok<IncomeTaxResult>>(result);
            Assert.NotNull(okResult.Value);
            Assert.Equal(expectedGrossAnnualSalary, okResult.Value.GrossAnnualSalary);
            Assert.Equal(expectedGrossMonthlySalary, okResult.Value.GrossMonthlySalary);
            Assert.Equal(expectedNetAnnualSalary, okResult.Value.NetAnnualSalary);
            Assert.Equal(expectedNetMonthlySalary, okResult.Value.NetMonthlySalary);
            Assert.Equal(expectedAnnualTaxPaid, okResult.Value.AnnualTaxPaid);
            Assert.Equal(expectedMonthlyTaxPaid, okResult.Value.MonthlyTaxPaid);
        }

        [Theory]
        [InlineData("", "Annual salary is required.")]
        [InlineData("notanumber", "Annual salary must be a valid number.")]
        [InlineData("-1000", "Annual salary must be non-negative.")]
        public async Task GetUkIncomeTaxHandlerInvalidSalary(
            string annualSalary,
            string expectedErrorMessage)
        {
            // Arrange/Act
            var result = await RunGetUkIncomeTax(annualSalary);

            // Assert
            var badRequest = Assert.IsType<BadRequest<IEnumerable<string>>>(result);
            Assert.NotNull(badRequest?.Value);
            Assert.Contains(expectedErrorMessage, badRequest.Value);
        }

        [Fact]
        public async Task Returns499OnCancellation()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            // Act
            var result = await RunGetUkIncomeTax("10000", cts.Token);
            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeHttpResult>(result);
            Assert.Equal(StatusCodes.Status499ClientClosedRequest, statusCodeResult.StatusCode);
        }

        //TODO: Test 500 via database error

        private static List<TaxBand> CreateTaxBands() =>
            [
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
            ];

        private static Mock<IDbConnection> CreateMockIDbConnection(List<TaxBand> taxBands)
        {
            var mockDbConnection = new Mock<IDbConnection>();

            // Setup mock to return taxBands when QueryAsync<TaxBand> is called
            mockDbConnection
                .SetupDapperAsync(c => c.QueryAsync<TaxBand>(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()
                ))
                .ReturnsAsync(taxBands);
            return mockDbConnection;
        }

        private static Mock<IDbConnectionFactory> CreateMockDbConnectionFactory(Mock<IDbConnection> mockDbConnection)
        {
            var mockFactory = new Mock<IDbConnectionFactory>();
            mockFactory
                .Setup(f => f.GetDbConnection())
                .Returns(mockDbConnection.Object);
            return mockFactory;
        }

        private static IMemoryCache CreateMockMemoryCache()
        {
            var mockCache = new Mock<IMemoryCache>();
            object dummy;
            mockCache
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out dummy))
                .Returns(false);
            mockCache
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object);
            return mockCache.Object;
        }

        private static IncomeTaxCalculator CreateIncomeTaxCalculator()
        {
            var taxBands = CreateTaxBands();
            var mockDbConnection = CreateMockIDbConnection(taxBands);
            var mockFactory = CreateMockDbConnectionFactory(mockDbConnection);
            var mockMemoryCache = CreateMockMemoryCache();

            var repository = new TaxBandRepository(
                Mock.Of<ILogger<TaxBandRepository>>(),
                mockMemoryCache,
                mockFactory.Object);

            return new IncomeTaxCalculator(repository);
        }

        private static async Task<IResult> RunGetUkIncomeTax(string annualSalary, CancellationToken cancellationToken = default) =>
            await IncomeTaxHandlers.GetUkIncomeTax(
                Mock.Of<ILogger<IncomeTaxQueryModel>>(),
                CreateIncomeTaxCalculator(),
                new IncomeTaxQueryModelValidator(),
                new IncomeTaxQueryModel { AnnualSalaryString = annualSalary },
                cancellationToken);

    }
}
