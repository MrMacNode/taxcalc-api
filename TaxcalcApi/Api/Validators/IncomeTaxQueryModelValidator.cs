using FluentValidation;
using TaxcalcApi.Api.Dtos;

namespace TaxcalcApi.Api.Validators
{
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
