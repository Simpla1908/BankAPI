using BankAPI.Data;
using BankAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankAPI.Services
{
    public class CompteBancaireService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompteBancaireService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private Utilisateur? GetCurrentUtilisateur()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId != null ? _context.Utilisateurs.AsNoTracking().FirstOrDefault(u => u.Id.ToString() == userId) : null;
        }

        public async Task<IEnumerable<CompteBancaire>> GetAllComptes()
        {
            return await _context.ComptesBancaires.Include(c => c.Client).AsNoTracking().ToListAsync();
        }

        public async Task<CompteBancaire?> GetCompteById(int id)
        {
            return await _context.ComptesBancaires.Include(c => c.Client).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CompteBancaire> CreateCompte(CompteBancaire compte)
        {
            var currentUtilisateur = GetCurrentUtilisateur();
            if (currentUtilisateur?.Role.Contains("Agent") != true)
            {
                throw new UnauthorizedAccessException("Vous n'avez pas la permission de créer des comptes.");
            }

            // Vérifier si le client existe
            var client = await _context.Clients.FindAsync(compte.ClientId);
            if (client == null)
            {
                throw new ArgumentException("Le client spécifié n'existe pas.");
            }

            compte.Client = client; // Associer le client trouvé
            compte.NumeroCompte = await GenererNumeroCompte();

            _context.ComptesBancaires.Add(compte);
            await _context.SaveChangesAsync();
            return compte;
        }


        public async Task<bool> DeleteCompte(int id)
        {
            var compte = await _context.ComptesBancaires.FindAsync(id);
            if (compte == null) return false;

            var currentUtilisateur = GetCurrentUtilisateur();
            if (currentUtilisateur?.Role.Contains("Admin") != true)
            {
                throw new UnauthorizedAccessException("Vous n'avez pas la permission de supprimer ce compte.");
            }

            _context.ComptesBancaires.Remove(compte);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EffectuerTransaction(string numeroCompte, decimal montant, bool isDepot)
        {
            var compte = await _context.ComptesBancaires.FirstOrDefaultAsync(c => c.NumeroCompte == numeroCompte);
            if (compte == null) return false;

            if (!isDepot && compte.Solde < montant) return false; // Vérifier solde pour retrait

            compte.Solde += isDepot ? montant : -montant;

            var currentUtilisateur = GetCurrentUtilisateur() ?? throw new UnauthorizedAccessException("Utilisateur non trouvé.");

            var transaction = new TransactionBancaire
            {
                CompteSource = numeroCompte,
                Montant = montant,
                DateTransaction = DateTime.UtcNow,
                UtilisateurId = currentUtilisateur.Id,
                TypeTransaction = isDepot ? "depot" : "retrait" // Assignation du type de transaction
            };

            _context.TransactionsBancaires.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EffectuerVirement(string compteSource, string compteDestination, decimal montant)
        {
            var source = await _context.ComptesBancaires.FirstOrDefaultAsync(c => c.NumeroCompte == compteSource);
            var destination = await _context.ComptesBancaires.FirstOrDefaultAsync(c => c.NumeroCompte == compteDestination);

            if (source == null || destination == null || source.Solde < montant) return false;

            source.Solde -= montant;
            destination.Solde += montant;

            var currentUtilisateur = GetCurrentUtilisateur() ?? throw new UnauthorizedAccessException("Utilisateur non trouvé.");

            var transaction = new TransactionBancaire
            {
                CompteSource = compteSource,
                CompteDestination = compteDestination,
                Montant = montant,
                DateTransaction = DateTime.UtcNow,
                UtilisateurId = currentUtilisateur.Id,
                TypeTransaction = "virement"

            };

            _context.TransactionsBancaires.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenererNumeroCompte()
        {
            string annee = DateTime.UtcNow.Year.ToString(); // Année actuelle
            string prefixe = "CB"; // Préfixe fixe pour les comptes bancaires

            // Compter le nombre total de comptes existants pour générer un identifiant unique
            int nombreComptes = await _context.ComptesBancaires.CountAsync() + 1;

            // Générer un numéro unique sous la forme CB-2024-0001
            return $"{prefixe}-{annee}-{nombreComptes:D4}";
        }





    }
}
