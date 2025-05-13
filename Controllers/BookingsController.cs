using kolos1.Models;
using kolos1.Services;
using Microsoft.AspNetCore.Mvc;

namespace kolos1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBook _book;

    public BookingsController(IBook book)
    {
        _book = book;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBooking(int id)
    {
        if (!await _book.BookingExists(id))
        {
            return NotFound($"Booking with ID:{id}, not found");
        }
        return Ok(await _book.GetBooking(id));
    }

    [HttpPost]
    public async Task<IActionResult> PostBooking([FromBody] POSTBookingDTO booking)
    {
        if (await _book.BookingExists(booking.bookingId))
        {
            return BadRequest($"Booking with ID:{booking.bookingId} already exists");
        }
        if(!await _book.GuestExists(booking.guestId))
            return NotFound($"Guest with ID:{booking.guestId} does not exist");
        
        if(!await _book.EmployeeExists(booking.employeeNumber))
            return NotFound($"Employee with Number:{booking.employeeNumber} does not exist");

        foreach (var attraction in booking.attractions)
        {
            if(!await _book.AttractionExists(attraction.name))
                return NotFound($"Attraction with Name:{attraction.name} does not exist");
        }

        await _book.PostBooking(booking);
        return CreatedAtAction(nameof(PostBooking), booking.bookingId, booking);
    }
}