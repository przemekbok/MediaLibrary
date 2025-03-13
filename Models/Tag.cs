using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediaLibrary.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Navigation property for media
        public virtual ICollection<MediaTag> MediaTags { get; set; } = new List<MediaTag>();
    }
}