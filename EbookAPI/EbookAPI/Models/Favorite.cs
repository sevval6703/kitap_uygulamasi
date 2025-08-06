using System.ComponentModel.DataAnnotations;

namespace EBookAPI.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BookId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Book Book { get; set; }
    }
}