namespace BankAPI.Models
{
    public class RegisterModel
    {
        public required string Nom { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

        public required string Role { get; set; }

    }
}
