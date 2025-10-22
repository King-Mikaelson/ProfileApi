using StringAnalyzer.Models;

namespace StringAnalyzer.Services
{
    public interface IStringAnalyzerService
    {
        Task<StringResponse> AnalyzeStringAsync(string value);
        Task<StringResponse?> GetStringByValueAsync(string value);
        Task<List<StringResponse>> GetAllStringsAsync();
        Task<bool> DeleteStringAsync(string value);
        Task<StringListResponse> GetAllStringsAsync(bool? isPalindrome = null, int? minLength = null, int? maxLength = null, int? wordCount = null, string? containsCharacter = null);
        Task<NaturalLanguageResponse> FilterByNaturalLanguageAsync(string query);
    }
}
