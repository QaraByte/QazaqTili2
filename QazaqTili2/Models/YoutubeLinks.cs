using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QazaqTili2.Models
{
    public class YoutubeLinks
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public int Id { get; set; }
        public string Url { get; set; }
        public int WordId { get; set; }
        public DateTime CreateTime { get; set; }
        public Word? Words { get; set; }
        public string? WordTime { get; set; }
        public string? Name { get; set; }
    }
}
