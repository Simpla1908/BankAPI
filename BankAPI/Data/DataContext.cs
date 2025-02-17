using BankAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }


        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Client> Clients { get; set; }

        public DbSet<CompteBancaire> ComptesBancaires { get; set; }
        public DbSet<TransactionBancaire> TransactionsBancaires { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompteBancaire>()
                .HasOne(cb => cb.Client)
                .WithMany(c => c.ComptesBancaires)
                .HasForeignKey(cb => cb.ClientId)
                .OnDelete(DeleteBehavior.Cascade); // Supprime les comptes si le client est supprimé

            modelBuilder.Entity<TransactionBancaire>()
                .HasOne(t => t.Utilisateur)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UtilisateurId)
                .OnDelete(DeleteBehavior.SetNull); // Laisse la transaction même si l'utilisateur est supprimé




        }


    }


}