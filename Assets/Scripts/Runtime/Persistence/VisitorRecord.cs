using SQLite;

namespace Museum.Persistence
{
    [Table("Visitor")]
    public class VisitorRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, MaxLength(120)]
        public string Name { get; set; }

        public int Age { get; set; }

        [MaxLength(2)]
        public string CountryCode { get; set; }

        [MaxLength(80)]
        public string CountryName { get; set; }

        public long StartedAtUnixSeconds { get; set; }
        public long EndedAtUnixSeconds { get; set; }

        public int ArtifactsIdentified { get; set; }
        public float OpenAiMinutesUsed { get; set; }
    }
}
