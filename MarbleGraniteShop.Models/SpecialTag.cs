using System.ComponentModel.DataAnnotations;

namespace MarbleGraniteShop.Models
{
    public class SpecialTag
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Special Tag")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
