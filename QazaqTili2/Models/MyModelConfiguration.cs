using System.Data.Entity.ModelConfiguration;

namespace QazaqTili2.Models
{
    public class MyModelConfiguration : EntityTypeConfiguration<AnalytByLetters>
    {
        public MyModelConfiguration()
        {
            this.ToTable("analyt_by_letters");
        }
    }
}
