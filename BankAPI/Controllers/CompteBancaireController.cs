using BankAPI.Entities;
using BankAPI.Models;
using BankAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Controllers
{
    [Route("api/comptes")]
    [ApiController]
    [Authorize]
    public class CompteBancaireController : ControllerBase
    {
        private readonly CompteBancaireService _compteService;

        public CompteBancaireController(CompteBancaireService compteService)
        {
            _compteService = compteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllComptes()
        {
            return Ok(await _compteService.GetAllComptes());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompte(int id)
        {
            var compte = await _compteService.GetCompteById(id);
            if (compte == null) return NotFound("Compte introuvable");
            return Ok(compte);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompte([FromBody] CompteBancaire compte)
        {
            try
            {
                var newCompte = await _compteService.CreateCompte(compte);
                //return CreatedAtAction(nameof(GetCompte), new { id = newCompte.Id }, newCompte);
                return Ok("Compte créée avec succès");

            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Vous n'avez pas les permissions pour créer des comptes.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompte(int id)
        {
            try
            {
                if (await _compteService.DeleteCompte(id))
                    return NoContent();
                return NotFound("Compte introuvable");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Vous n'avez pas les permissions pour supprimer ce compte.");
            }
        }

        [HttpPost("depot")]
        public async Task<IActionResult> Depot([FromBody] TransactionRequest request)
        {
            if (await _compteService.EffectuerTransaction(request.NumeroCompte, request.Montant, true))
                return Ok("Dépôt effectué avec succès");
            return BadRequest("Échec du dépôt");
        }

        [HttpPost("retrait")]
        public async Task<IActionResult> Retrait([FromBody] TransactionRequest request)
        {
            if (await _compteService.EffectuerTransaction(request.NumeroCompte, request.Montant, false))
                return Ok("Retrait effectué avec succès");
            return BadRequest("Solde insuffisant ou compte introuvable");
        }

        [HttpPost("virement")]
        public async Task<IActionResult> Virement([FromBody] VirementRequest request)
        {
            if (await _compteService.EffectuerVirement(request.CompteSource, request.CompteDestination, request.Montant))
                return Ok("Virement effectué avec succès");
            return BadRequest("Virement échoué : solde insuffisant ou comptes introuvables");
        }
    }
}
