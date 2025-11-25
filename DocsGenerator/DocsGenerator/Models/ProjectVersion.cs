using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DocsGenerator.Models
{
    [Table("ProjectVersions")]
    public class ProjectVersion
    {
        [Key]
        public int Id { get; set; }
        public string CommitHash { get; set; } = null!;
        public string CommitName { get; set; } = null!;
        public string Branch { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? DocsPath { get; set; }
    }
}
