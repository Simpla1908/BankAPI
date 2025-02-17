namespace BankAPI.Entities
{
    public class CompteBancaire
    {
        public int Id { get; set; }
        public string? NumeroCompte { get; set; }
        public decimal Solde { get; set; } = 0; // Valeur par défaut 0
        public required string TypeCompte { get; set; } // "courant" ou "crédit"
        public required int ClientId { get; set; }
        public Client? Client { get; set; } // Permet d'éviter l'erreur de conversion
    }
}
