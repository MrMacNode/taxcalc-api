using TaxcalcApi.Infrastructure.Database.Entities;

namespace TaxcalcApi.Infrastructure.Database.Queries
{
    public class GetUkTaxBands : IGetUkTaxBands
    {
        public async Task<IEnumerable<TaxBand>> Execute()
        {
            return new List<TaxBand>
            {
                new TaxBand
                {
                    Id = Guid.NewGuid(),
                    LowerLimit = 0,
                    UpperLimit = 5000,
                    Rate = 0
                },
                new TaxBand
                {
                    Id = Guid.NewGuid(),
                    LowerLimit = 5000,
                    UpperLimit = 20000,
                    Rate = 20
                },
                new TaxBand
                {
                    Id = Guid.NewGuid(),
                    LowerLimit = 20000,
                    Rate = 40
                },
            };
        }
    }
}
