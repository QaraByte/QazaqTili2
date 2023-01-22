namespace QazaqTili2.Models
{
    public class YoutubeLinks
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int WordId { get; set; }
        public DateTime CreateTime { get; set; }
        public Word? Words { get; set; }
        public string WordTime { get; set; }
    }
}
