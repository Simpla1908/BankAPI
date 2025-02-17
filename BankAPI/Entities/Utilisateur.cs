
namespace BankAPI.Entities
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public required string Nom { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string Role { get; set; } = string.Empty;

        public ICollection<TransactionBancaire> Transactions { get; set; } = new List<TransactionBancaire>(); // Un utilisateur peut effectuer plusieurs transactions
    }

}
