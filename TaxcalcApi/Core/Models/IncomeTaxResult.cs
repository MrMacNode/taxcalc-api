namespace TaxcalcApi.Core.Models
{
    /// <summary>
    /// Represents the result of an income tax calculation, including gross and net salaries as well as taxes paid.
    /// </summary>
    /// <remarks>
    ///     - This model could be used for various income tax calculations for different countries.
    ///     - Extensions could include additional fields for other deductions or benefits, region specific fields, etc.
    ///     - While this model is also a response object, it makes sense to keep it here in the Core project as a business domain model.
    ///         - If this application ceased to be an API or if this core logic were to be reused, we could still return this model to whatever consumer needs it.
    ///         - If it were to be reused in different .NET solutions, a separate library would be appropriate. But for now, we only have 1 small model.
    /// </remarks>
    public class IncomeTaxResult
    {
        public decimal GrossAnnualSalary { get; set; }
        public decimal GrossMonthlySalary { get; set; }
        public decimal NetAnnualSalary { get; set; }
        public decimal NetMonthlySalary { get; set; }
        public decimal AnnualTaxPaid { get; set; }
        public decimal MonthlyTaxPaid { get; set; }
    }
}
