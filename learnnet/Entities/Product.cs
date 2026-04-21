using System.ComponentModel.DataAnnotations;

namespace learnnet.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for 1-to-1 relationship
        public virtual ProductDetail? Details { get; set; }
    }
}
