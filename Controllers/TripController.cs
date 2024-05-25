using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APBDcw5.Models;
using APBDcw5.DTO;
namespace APBDcw5.Controller;

[Route("api/[controller]")]
[ApiController]
public class TripController : ControllerBase
{
    private readonly Apbd5Context _context;

    public TripController(Apbd5Context context)
    {
        _context = context;
    }

    [HttpGet]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TripDto>>> GetTrips()
    {
        var trips = await _context.Trips
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .Include(t => t.IdCountries)
            .OrderByDescending(t => t.DateFrom)
            .Select(t => new TripDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new CountryDto
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(ct => new ClientDto
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                }).ToList()
            })
            .ToListAsync();

        return Ok(trips);
    }
    
    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] ClientRequest clientRequest)
    {
        var trip = await _context.Trips
            .Include(t => t.ClientTrips)
            .FirstOrDefaultAsync(t => t.IdTrip == idTrip);

        if (trip == null)
        {
            return NotFound("Trip not found.");
        }
        
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == clientRequest.Pesel);
        
        if (client == null)
        {
            client = new Client
            {
                FirstName = clientRequest.FirstName,
                LastName = clientRequest.LastName,
                Email = clientRequest.Email,
                Telephone = clientRequest.Telephone,
                Pesel = clientRequest.Pesel
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }
        
        if (trip.ClientTrips.Any(ct => ct.IdClient == client.IdClient))
        {
            return BadRequest("Client is already assigned to this trip.");
        }

        var clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = clientRequest.PaymentDate
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return Ok();
    }

    public class ClientRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Pesel { get; set; } = string.Empty;
        public int IdTrip { get; set; }
        public string TripName { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
    }

}