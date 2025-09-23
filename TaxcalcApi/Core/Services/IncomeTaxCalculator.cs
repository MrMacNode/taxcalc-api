using System.Threading;
using TaxcalcApi.Core.Models;
using TaxcalcApi.Infrastructure.Database.Entities;
using TaxcalcApi.Infrastructure.Database.Repositories;

namespace TaxcalcApi.Core.Services
{
    /// <summary>
    /// Provides functionality to calculate income tax based on tax bands.
    /// </summary>
    /// <remarks>This is the core business functionality for income tax calculations, calling backend boilerplate for tax bands and implementing defined business rules.</remarks>
    public class IncomeTaxCalculator(ITaxBandRepository _repository) : IIncomeTaxCalculator
    {
        /// <summary>
        /// Calculates the UK income tax based on an annual salary.
        /// </summary>
        /// <param name="annualSalary"></param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        ///     - Note that the ordering of tax bands is irrelevant to the calculation. Each band is applied independently to the salary, and the when doesn't matter as long as they all add up.
        ///     - What is NOT irrelevant is the completeness and correctness of the tax bands in the database. 
        ///         - If they are wrong or incomplete, the calculation will be wrong.
        ///         - This calculation is naive of whether the bands overlap or have gaps.
        ///             - No business rules are defined for such a situation; 
        ///             - It is a data integrity issue;
        ///             - The bands dictate the rules for a range of money. If there are overlaps, we're assuming the government wants to double-tax the overlap. (AND WHO WOULD BE SURPRISED?)
        /// </remarks>
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

        /// <summary>
        /// This method calculates the portion of the annual salary that falls within a specific tax band.
        /// </summary>
        /// <param name="annualSalary"></param>
        /// <param name="taxBand"></param>
        /// <returns>The amount which is to be applied to the band's tax rate.</returns>
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
