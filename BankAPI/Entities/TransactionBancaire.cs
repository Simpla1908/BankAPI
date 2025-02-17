namespace BankAPI.Entities
{
    public class TransactionBancaire
    {
        public int Id { get; set; }
        public required string CompteSource { get; set; }
        public string? CompteDestination { get; set; } // Nullable pour dépôt/retrait
        public decimal Montant { get; set; }
        public DateTime DateTransaction { get; set; } = DateTime.UtcNow;
        public required string TypeTransaction { get; set; } 

        public int? UtilisateurId { get; set; } // Nullable pour éviter la suppression des transactions si l'utilisateur est supprimé
        public Utilisateur? Utilisateur { get; set; }


    }
}
