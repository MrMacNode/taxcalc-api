using TaxcalcApi.Core.Models;

namespace TaxcalcApi.Core.Services
{
    public interface IIncomeTaxCalculator
    {
        Task<IncomeTaxResult> CalculateUkAnnual(decimal annualSalary);
    }
}
