namespace StringAnalyzer.Models;

// Main response for a single string
public class StringResponse
{
    public string Id { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public StringProperties Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

// Nested properties object
public class StringProperties
{
    public int Length { get; set; }
    public bool IsPalindrome { get; set; }
    public int UniqueCharacters { get; set; }
    public int WordCount { get; set; }
    public string Sha256Hash { get; set; } = string.Empty;
    public Dictionary<string, int> CharacterFrequencyMap { get; set; } = new();
}

// Response for GET /strings (list with filters)
public class StringListResponse
{
    public List<StringResponse> Data { get; set; } = new();
    public int Count { get; set; }
    public object? FiltersApplied { get; set; }
}

// Response for natural language filtering
public class NaturalLanguageResponse
{
    public List<StringResponse> Data { get; set; } = new();
    public int Count { get; set; }
    public InterpretedQuery InterpretedQuery { get; set; } = new();
}

public class InterpretedQuery
{
    public string Original { get; set; } = string.Empty;
    public Dictionary<string, object> ParsedFilters { get; set; } = new();
}