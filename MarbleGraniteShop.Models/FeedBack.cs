using System.ComponentModel.DataAnnotations.Schema;

namespace MarbleGraniteShop.Models
{
    public class FeedBack
    {
        public int Id { get; set; }
        public string Rating { get; set; }
        public string Review { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
