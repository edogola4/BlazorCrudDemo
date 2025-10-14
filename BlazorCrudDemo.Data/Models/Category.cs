using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorCrudDemo.Data.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        [MaxLength(250)]
        public string Description { get; set; }
        
        public ICollection<Product> Products { get; set; } = new List<Product>();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
