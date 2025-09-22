using System.Threading;
using TaxcalcApi.Core.Models;
using TaxcalcApi.Infrastructure.Database.Entities;
using TaxcalcApi.Infrastructure.Database.Repositories;

namespace TaxcalcApi.Core.Services
{
    public class IncomeTaxCalculator(ITaxBandRepository _repository) : IIncomeTaxCalculator
    {
        public async Task<IncomeTaxResult> CalculateUkAnnual(decimal annualSalary, CancellationToken cancellationToken)
        {
            var taxBands = await _repository.GetAllAsync(cancellationToken) ?? [];
            decimal annualTax = 0;
            foreach(var taxBand in taxBands)
            {
                annualTax += CalculateTaxableIncome(annualSalary, taxBand) * taxBand.Rate / 100;
            }
            return new()
            {
                GrossAnnualSalary = ToCurrency(annualSalary),
                GrossMonthlySalary = ToCurrency(annualSalary / 12),
                NetAnnualSalary = ToCurrency(annualSalary - annualTax),
                NetMonthlySalary = ToCurrency((annualSalary - annualTax) / 12),
                AnnualTaxPaid = ToCurrency(annualTax),
                MonthlyTaxPaid = ToCurrency(annualTax / 12)
            };
        }

        protected static decimal ToCurrency(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

        protected static decimal CalculateTaxableIncome(decimal annualSalary, TaxBand taxBand)
        {
            if (annualSalary <= taxBand.LowerLimit)
            {
                return 0;
            }

            if(taxBand.UpperLimit is not null && annualSalary > taxBand.UpperLimit)
            {
                return taxBand.UpperLimit.Value - taxBand.LowerLimit;
            }

            return Math.Max(0, annualSalary - taxBand.LowerLimit);
        }
    }
}
