using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using BankAPI.Entities;
using BankAPI.Models;
using BankAPI.Data;

namespace BankAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(DataContext context, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur.
        /// </summary>
        [HttpPost("Register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Données invalides." });

            if (_context.Utilisateurs.Any(u => u.Email == model.Email))
                return Conflict(new { message = "L'utilisateur existe déjà." });

            // Hachage sécurisé du mot de passe
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var utilisateur = new Utilisateur
            {
                Nom = model.Nom,
                Email = model.Email,
                PasswordHash = passwordHash,
                Role = model.Role
            };

            _context.Utilisateurs.Add(utilisateur);
            _context.SaveChanges();

            return Created("", new { message = "Utilisateur enregistré avec succès." });
        }

        /// <summary>
        /// Connexion et génération d'un JWT.
        /// </summary>
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Données invalides." });

            var user = _context.Utilisateurs.SingleOrDefault(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized(new { message = "Identifiants incorrects." });

            var token = GenerateJwtToken(user);

            return Ok(new { token });
        }

        /// <summary>
        /// Génère un JWT sécurisé.
        /// </summary>
        private string GenerateJwtToken(Utilisateur user)
        {
            var tokenKey = _configuration["AppSettings:Token"];
            if (string.IsNullOrEmpty(tokenKey))
                throw new InvalidOperationException("Clé de sécurité JWT introuvable.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Nom),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID unique du token
                new Claim(ClaimTypes.Role, user.Role) // Ajout du rôle pour gestion des permissions
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["AppSettings:Issuer"],
                audience: _configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
