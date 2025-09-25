using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace TaxcalcApi.Api.Dtos
{
    /// <summary>
    /// Parameter model for income tax calculation queries.
    /// </summary>
    public record IncomeTaxQueryModel
    {
        private string? _annualSalaryString = string.Empty;
        private decimal _annualSalary = 0;


        /// <summary>
        /// Front-end parameter for annual salary. This is a string to facilitate validation and control messaging.
        /// </summary>
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

        /// <summary>
        /// Annual salary as a parsed, validated decimal value.
        /// </summary>
        public decimal AnnualSalary
        {
            get => _annualSalary;
        }
    }
}
