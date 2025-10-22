namespace StringAnalyzer.Models;

// Main response for a single string
public class StringResponse
{
    public string id { get; set; } = string.Empty;
    public string value { get; set; } = string.Empty;
    public StringProperties properties { get; set; } = new();
    public DateTime created_at { get; set; }
}

// Nested properties object
public class StringProperties
{
    public int length { get; set; }
    public bool is_palindrome { get; set; }
    public int unique_characters { get; set; }
    public int word_count { get; set; }
    public string sha256_hash { get; set; } = string.Empty;
    public Dictionary<string, int> character_frequency_map { get; set; } = new();
}

// Response for GET /strings (list with filters)
public class StringListResponse
{
    public List<StringResponse> data { get; set; } = new();
    public int count { get; set; }
    public object? filters_applied { get; set; }
}

// Response for natural language filtering
public class NaturalLanguageResponse
{
    public List<StringResponse> data { get; set; } = new();
    public int count { get; set; }
    public InterpretedQuery interpreted_query { get; set; } = new();
}

public class InterpretedQuery
{
    public string original { get; set; } = string.Empty;
    public Dictionary<string, object> parsed_filters { get; set; } = new();
}