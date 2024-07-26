using System.ComponentModel.DataAnnotations;

namespace OnlinePazar.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public int? Stock { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
