using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace TaxcalcApi.Api.Dtos
{
    public record IncomeTaxQueryModel
    {
        private string? _annualSalaryString = string.Empty;
        private decimal _annualSalary = 0;

        [property: FromQuery(Name = "annualSalary")]
        public string? AnnualSalaryString
        { 
            get => _annualSalaryString;
            init
            {
                _annualSalaryString = value;
                if(decimal.TryParse(value, out decimal parseValue))
                {
                    _annualSalary = parseValue;
                }
            }
        }

        public decimal AnnualSalary
        {
            get => _annualSalary;
        }
    }
}
