namespace Seatpicker.Domain;

public class Floorplan 
{
    public int Id { get; set; }

    public ICollection<Seat> Seats { get; }
 
    public Backdrop Backdrop { get; set; }

    public void AddSeat(Seat seat) => Seats.Add(seat); 
}

public class Seat
{
    public int Id { get; }
    
    public Seat? PositionRelativeTo { get; set; }
    
    public User? UserId { get; set; }

    public double X { get; set; }
    
    public double Y { get; set; }
    
    public double Width { get; set; }
    
    public double Height { get; set; }
}

public record Backdrop(byte[] Pdf);