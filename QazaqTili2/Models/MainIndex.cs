using System;

namespace QazaqTili2.Models
{
    public class MainIndex
    {
        public Int64 Row { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreateTime { get; set; }
        public int? WordTypeId { get; set; }
        public string WordTypeName { get; set; }
        public int Count { get; set; }
    }
}
