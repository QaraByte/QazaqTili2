namespace QazaqTili2.Models
{
    public class Word
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public int Id { get; set; }
        public string Name { get; set; }
        [DefaultValue("getdate()")]
        public DateTime CreateTime { get; set; }
    }
}
