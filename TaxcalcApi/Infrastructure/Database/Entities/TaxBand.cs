namespace TaxcalcApi.Infrastructure.Database.Entities
{
    public record TaxBand
    {
        public Guid Id { get; set; }
        public int LowerLimit { get; set; }
        public int? UpperLimit { get; set; }
        public int Rate { get; set; }
    }
}
