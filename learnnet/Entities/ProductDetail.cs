using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace learnnet.Entities
{
    public class ProductDetail
    {
        [Key]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Manufacturer { get; set; } = string.Empty;

        public int WarrantyPeriodMonths { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual Product? Product { get; set; }
    }
}
