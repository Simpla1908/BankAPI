using BankAPI.Data;
using BankAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankAPI.Services
{
    public class ClientService
    {
        private readonly DataContext _context;

        public ClientService(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _context.Clients.Include(c => c.ComptesBancaires).ToListAsync();
        }

        public async Task<Client?> GetClientByIdAsync(int id)
        {
            return await _context.Clients.Include(c => c.ComptesBancaires)
                                         .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<Client?> UpdateClientAsync(int id, Client client)
        {
            var existingClient = await _context.Clients.FindAsync(id);
            if (existingClient == null) return null;

            existingClient.Nom = client.Nom;
            existingClient.Email = client.Email;
            existingClient.Telephone = client.Telephone;

            await _context.SaveChangesAsync();
            return existingClient;
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return false;

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
