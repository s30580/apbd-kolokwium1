
using System.Data.Common;
using kolos1.Models;
using Microsoft.Data.SqlClient;

namespace kolos1.Services;

public class Book: IBook
{
    private readonly IConfiguration _configuration;

    public Book(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    private async Task<bool> IfExists(string select,int id)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        command.CommandText = select;
        command.Parameters.Add(new SqlParameter("@id", id));
        var result = await command.ExecuteScalarAsync();
        return result != null;
    }
    private async Task<bool> IfExists(string select,string str)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        command.CommandText = select;
        command.Parameters.Add(new SqlParameter("@string", str));
        var result = await command.ExecuteScalarAsync();
        return result != null;
    }

    public async Task<bool> BookingExists(int id)
    {
        string select = "SELECT 1 FROM Booking WHERE booking_id = @id";
        return await IfExists(select, id);
    }

    public async Task<BookingDTO> GetBooking(int id)
    {
        BookingDTO booking = new BookingDTO();
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        command.CommandText = "select b.date,g.first_name,g.last_name,g.date_of_birth,e.first_name as firstname,e.last_name as lastname,e.employee_number as employeenumber from Booking b join Guest g on b.guest_id = g.guest_id join Employee e on b.employee_id = e.employee_id WHERE b.booking_id = @id";
        command.Parameters.Add(new SqlParameter("@id", id));
        using (var result = await command.ExecuteReaderAsync())
        {
            result.ReadAsync();
            booking.date = result.GetDateTime(result.GetOrdinal("date"));
            booking.guest = new GuestDTO()
            {
                firstName = result.GetString(result.GetOrdinal("first_name")),
                lastName = result.GetString(result.GetOrdinal("last_name")),
                dateOfBirth = result.GetDateTime(result.GetOrdinal("date_of_birth")),
            };
            booking.employee = new EmployeeDTO()
            {
                firstName = result.GetString(result.GetOrdinal("firstname")),
                lastName = result.GetString(result.GetOrdinal("lastname")),
                employeeNumber = result.GetString(result.GetOrdinal("employeenumber")),
            };
            booking.attractions = new List<Attractions>();
        }
        command.Parameters.Clear();
        command.CommandText =
            "select a.name,a.price,b.amount from Booking_Attraction b join Attraction a on b.attraction_id = a.attraction_id where booking_id = @id";
        command.Parameters.Add(new SqlParameter("@id", id));
        using (var result = await command.ExecuteReaderAsync())
        {
            while (await result.ReadAsync())
            {
                booking.attractions.Add(new Attractions
                {
                    name = result.GetString(result.GetOrdinal("name")),
                    price = result.GetDecimal(result.GetOrdinal("price")),
                    amount = result.GetInt32(result.GetOrdinal("amount")),
                });
            }
            
        }

        return booking;
    }

    public async Task<bool> GuestExists(int id)
    {
        string select = "SELECT 1 FROM Guest WHERE guest_id = @id";
        return await IfExists(select, id);
    }

    public async Task<bool> EmployeeExists(string number)
    {
        string select = "SELECT 1 FROM Employee WHERE employee_number = @string";
        return await IfExists(select, number);
    }

    public async Task<bool> AttractionExists(string number)
    {
        string select = "SELECT 1 FROM Attraction WHERE name = @string";
        return await IfExists(select, number);
    }

    public async Task PostBooking(POSTBookingDTO booking)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        DbTransaction transaction = connection.BeginTransaction();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText = "select employee_id from Employee where employee_number = @number";
            command.Parameters.AddWithValue("@number", booking.employeeNumber);
            var employeeID = await command.ExecuteScalarAsync();
            command.Parameters.Clear();
            
            command.CommandText = "Insert Into Booking values (@bookingid, @guestid,@employeeid,@date)";
            command.Parameters.AddWithValue("@bookingid", booking.bookingId);
            command.Parameters.AddWithValue("@guestid", booking.guestId);
            command.Parameters.AddWithValue("@employeeid", employeeID);
            command.Parameters.AddWithValue("@date", DateTime.Now);
            if(await command.ExecuteNonQueryAsync()==0)throw new Exception("Could not insert new booking");
            
            
            foreach (var attraction in booking.attractions)
            {
                command.Parameters.Clear();
                command.CommandText = "select attraction_id from Attraction WHERE name = @name";
                command.Parameters.AddWithValue("@name", attraction.name);
                var attractionID = await command.ExecuteScalarAsync();
                
                command.Parameters.Clear();
                command.CommandText = "Insert Into Booking_Attraction values (@bookingid,@attractionid, @amount)";
                command.Parameters.AddWithValue("@bookingid", booking.bookingId);
                command.Parameters.AddWithValue("@attractionid", attractionID);
                command.Parameters.AddWithValue("@amount", attraction.amount);
                if(await command.ExecuteNonQueryAsync()==0)throw new Exception("Could not insert into Booking_Attraction");
            }
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw e;
        }

    }
}