namespace QazaqTili2.Models
{
    public class WordTypes
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateTime { get; set; }
        public IEnumerable<Word>? Words { get; set; }
    }
}
