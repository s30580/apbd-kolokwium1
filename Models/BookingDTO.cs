namespace kolos1.Models;

public class BookingDTO
{
    public DateTime date { get; set; }
    public GuestDTO guest { get; set; }
    public EmployeeDTO employee { get; set; }
    public List<Attractions> attractions { get; set; }
}

public class GuestDTO
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public DateTime dateOfBirth { get; set; }
}

public class EmployeeDTO
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string employeeNumber { get; set; }
}

public class Attractions
{
    public string name { get; set; }
    public Decimal price { get; set; }
    public int amount { get; set; }
}