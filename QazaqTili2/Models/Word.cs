using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QazaqTili2.Models
{
    public class Word
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public int Id { get; set; }
        public string Name { get; set; }
        //[DefaultValue("getdate()")]
        public DateTime? CreateTime { get; set; }
        public WordTypes? WordTypes { get; set; }
        public int? WordTypeId { get; set; }
        public IEnumerable<YoutubeLinks>? YoutubeLinks { get; set; }
    }
}
