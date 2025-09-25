using TaxcalcApi.Infrastructure.Database.Entities;

namespace TaxcalcApi.Infrastructure.Database.Repositories;

public interface ITaxBandRepository
{
    Task<IEnumerable<TaxBand>> GetAllAsync(CancellationToken cancellationToken);
}

