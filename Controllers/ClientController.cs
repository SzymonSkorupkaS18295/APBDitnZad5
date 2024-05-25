using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using APBDcw5.Models;

namespace APBDcw5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly Apbd5Context _context;

        public ClientsController(Apbd5Context context)
        {
            _context = context;
        }
        
        [HttpDelete("{idClient}")]
        public async Task<IActionResult> DeleteClient(int idClient)
        {
            var client = await _context.Clients
                .Include(c => c.ClientTrips)
                .FirstOrDefaultAsync(c => c.IdClient == idClient);

            if (client == null)
            {
                return NotFound();
            }

            if (client.ClientTrips.Any())
            {
                return BadRequest("Client has assigned trips and cannot be deleted.");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}