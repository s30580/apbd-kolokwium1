using kolos1.Models;

namespace kolos1.Services;

public interface IBook
{
    Task<bool> BookingExists(int id);
    Task<BookingDTO> GetBooking(int id);
    
    Task<bool> GuestExists(int id);
    Task<bool> EmployeeExists(string str);
    Task<bool> AttractionExists(string str);
    
    Task PostBooking(POSTBookingDTO booking);
}