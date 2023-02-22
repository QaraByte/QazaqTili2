using System.ComponentModel.DataAnnotations.Schema;

namespace QazaqTili2.Models
{
    [Table("Files")]
    public class FileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public int WordId { get; set; }
        public DateTime UploadTime { get; set; }
    }
}
