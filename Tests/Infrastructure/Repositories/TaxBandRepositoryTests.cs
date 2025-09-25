using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Dapper;
using Polly;
using Xunit;
using TaxcalcApi.Infrastructure.Database.Entities;
using TaxcalcApi.Infrastructure.Database.Repositories;
using System.Data;

namespace TaxcalcApi.Tests.Infrastructure.Repositories;

public class TaxBandRepositoryTests
{
    private static TaxBand CreateTaxBand(Guid? id = null) =>
        new TaxBand { Id = id ?? Guid.NewGuid(), LowerLimit = 0, UpperLimit = 10000, Rate = 10 };

    [Fact]
    public async Task ReturnsFromCacheWhenCacheIsValid()
    {
        // Arrange
        var cachedBands = new List<TaxBand> { CreateTaxBand() };
        var cacheMock = new Mock<IMemoryCache>();
        var loggerMock = new Mock<ILogger<TaxBandRepository>>();
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();

        object outValue = cachedBands;
        cacheMock.Setup(m => m.TryGetValue("TaxBandsCacheKey", out outValue)).Returns(true);

        var repo = new TaxBandRepository(loggerMock.Object, cacheMock.Object, connectionFactoryMock.Object);

        // Act
        var result = await repo.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(cachedBands, result);
        connectionFactoryMock.Verify(f => f.GetDbConnection(), Times.Never);
    }

    //TODO: Testing for pulling from DB when cache is expired
    //TODO: Testing for Polly retry
}