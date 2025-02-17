using System.Text.Json.Serialization;

namespace BankAPI.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public required string Nom { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<CompteBancaire> ComptesBancaires { get; set; } = new List<CompteBancaire>(); // Correction du nom
    }
}
