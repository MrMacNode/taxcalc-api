using TaxcalcApi.Core.Models;
using TaxcalcApi.Infrastructure.Database.Entities;
using TaxcalcApi.Infrastructure.Database.Queries;

namespace TaxcalcApi.Core.Services
{
    public class IncomeTaxCalculator(IGetUkTaxBands GetTaxBands) : IIncomeTaxCalculator
    {
        public async Task<IncomeTaxResult> CalculateUkAnnual(decimal annualSalary)
        {
            var taxBands = (await GetTaxBands.Execute()).OrderBy(tb => tb.LowerLimit).ToList();
            decimal annualTax = 0;
            foreach(var taxBand in taxBands)
            {
                annualTax += CalculateTaxableIncome(annualSalary, taxBand) * taxBand.Rate / 100;
            }
            return new()
            {
                GrossAnnualSalary = annualSalary,
                GrossMonthlySalary = annualSalary / 12,
                NetAnnualSalary = annualSalary - annualTax,
                NetMonthlySalary = (annualSalary - annualTax) / 12,
                AnnualTaxPaid = annualTax,
                MonthlyTaxPaid = annualTax / 12
            };
        }

        private static decimal CalculateTaxableIncome(decimal annualSalary, TaxBand taxBand)
        {
            if (annualSalary > taxBand.LowerLimit)
            {
                if(taxBand.UpperLimit is not null && annualSalary > taxBand.UpperLimit)
                {
                    return taxBand.UpperLimit.Value - taxBand.LowerLimit;
                }
                return Math.Max(0, annualSalary - taxBand.LowerLimit);
            }
            return 0;
        }
    }
}
