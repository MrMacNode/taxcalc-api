using FluentValidation.TestHelper;
using TaxcalcApi.Api.Dtos;
using TaxcalcApi.Api.Validators;

namespace Tests.Api.Validators;
public class IncomeTaxQueryModelValidatorTests
{
    private readonly IncomeTaxQueryModelValidator validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void NullOrEmptyAnnualSalaryStringIsInvalid(string? salaryString)
    {
        var model = new IncomeTaxQueryModel { AnnualSalaryString = salaryString };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.AnnualSalaryString)
            .WithErrorMessage("Annual salary is required.");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12.34.56")]
    [InlineData("1,000,000.00.00")]
    [InlineData("1e309")]
    public void InvalidNumberAnnualSalaryStringIsInvalid(string salaryString)
    {
        var model = new IncomeTaxQueryModel { AnnualSalaryString = salaryString };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.AnnualSalaryString)
            .WithErrorMessage("Annual salary must be a valid number.");
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("-0.01")]
    [InlineData("-100000")]
    public void NegativeAnnualSalaryIsInvalid(string negativeSalaryString)
    {
        var model = new IncomeTaxQueryModel { AnnualSalaryString = negativeSalaryString };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.AnnualSalary)
            .WithErrorMessage("Annual salary must be non-negative.");
    }

    [Theory]
    [InlineData("4000000000000")]
    public void RidiculousNumberIsInvalid(string bigNumberString)
    {
        var model = new IncomeTaxQueryModel { AnnualSalaryString = bigNumberString };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.AnnualSalary)
            .WithErrorMessage("Annual salary must be less than three trillion. Are you a country?");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1000")]
    [InlineData("123456.78")]
    [InlineData("999999999")]
    public void ValidAnnualSalaryHasNoValidationErrors(string salaryString)
    {
        var model = new IncomeTaxQueryModel { AnnualSalaryString = salaryString };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
