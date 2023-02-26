using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QazaqTili2.Models
{
    public class ImageLinks
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public int Id { get; set; }
        public int ParentWordId { get; set; }
        public int WordId { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
