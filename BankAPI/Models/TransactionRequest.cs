namespace BankAPI.Models
{
    public class TransactionRequest
    {
        public required string NumeroCompte { get; set; }
        public decimal Montant { get; set; }
    }
}
