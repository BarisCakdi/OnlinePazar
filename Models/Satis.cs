using System.ComponentModel.DataAnnotations;

namespace OnlinePazar.Models;

public class Satis
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public int SatisAdedi {  get; set; } 
    public int SatisFiyati { get; set; }

}
