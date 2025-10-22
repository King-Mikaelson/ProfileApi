using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StringAnalyzer.Models;

[Table("AnalyzedStrings")]
public class AnalyzedString
{
    // Primary Key - this is the SHA-256 hash
    [Key]
    [MaxLength(64)]
    public string Id { get; set; } = string.Empty;

    // The actual string value
    [Required]
    public string Value { get; set; } = string.Empty;

    // Computed Properties
    public int Length { get; set; }

    public bool IsPalindrome { get; set; }

    public int UniqueCharacters { get; set; }

    public int WordCount { get; set; }

    [MaxLength(64)]
    public string Sha256Hash { get; set; } = string.Empty;

    // Character frequency stored as JSON string
    [Column(TypeName = "nvarchar(max)")]
    public string CharacterFrequencyMapJson { get; set; } = string.Empty;

    // Timestamp
    public DateTime CreatedAt { get; set; }
}
