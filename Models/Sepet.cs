using System.ComponentModel.DataAnnotations;

namespace OnlinePazar.Models;

public class Sepet
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public string UrunAdi { get; set; }
    public int UrunFiyat { get; set; }
    public int UrunAdet {  get; set; }
}
