using TaxcalcApi.Infrastructure.Database.Entities;

namespace TaxcalcApi.Infrastructure.Database.Queries
{
    public interface IGetUkTaxBands
    {
        Task<IEnumerable<TaxBand>> Execute();
    }
}
