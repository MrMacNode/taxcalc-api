using FluentValidation;
using TaxcalcApi.Api.Dtos;

namespace TaxcalcApi.Api.Validators
{
    /// <summary>
    /// A validator for income tax query parameters.
    /// </summary>
    /// <remarks>
    ///     - Guarantees that the annual salary parameter is provided, is a valid number, and is non-negative.
    ///     - Controls bad request error messages for invalid input.
    /// </remarks>
    public class IncomeTaxQueryModelValidator : AbstractValidator<IncomeTaxQueryModel>
    {
        public IncomeTaxQueryModelValidator()
        {
            RuleFor(x => x.AnnualSalaryString)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Annual salary is required.")
                .Must(value => decimal.TryParse(value, out _))
                .WithMessage("Annual salary must be a valid number.")
                .WithName("AnnualSalary");
            RuleFor(x => x.AnnualSalary)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Annual salary must be non-negative.");
        }
    }
}
