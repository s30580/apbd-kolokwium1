using System.ComponentModel.DataAnnotations;

namespace kolos1.Models;

public class POSTBookingDTO
{
    public int bookingId { get; set; }
    public int guestId { get; set; }
    public string employeeNumber { get; set; }
    public List<POSTAttractionsDTO> attractions { get; set; }
}

public class POSTAttractionsDTO
{
    public string name { get; set; }
    public int amount { get; set; }
}