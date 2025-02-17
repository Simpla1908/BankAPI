namespace BankAPI.Models
{
    public class VirementRequest
    {

        public required string CompteSource { get; set; }
        public required string CompteDestination { get; set; }
        public decimal Montant { get; set; }
    }
}
