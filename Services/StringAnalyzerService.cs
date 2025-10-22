using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StringAnalyzer.Data;
using StringAnalyzer.Exceptions;
using StringAnalyzer.Models;

namespace StringAnalyzer.Services
{
    public class StringAnalyzerService : IStringAnalyzerService
    {
        private readonly ApplicationDbContext _context;

        public StringAnalyzerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StringResponse> AnalyzeStringAsync(string value)
        {
            if (value is null)
                throw new InvalidStringTypeException("'value' must be a string.");

            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidStringException("Invalid request body or missing \"value\" field");

            // SHA-256 hash
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // Check if string already exists
            var existing = await _context.AnalyzedStrings.FindAsync(hashString);
            if (existing != null)
                throw new StringAlreadyExistsException("String already exists in the system.");

            // Compute properties
            var properties = new StringProperties
            {
                Length = value.Length,
                IsPalindrome = IsPalindrome(value.ToLower()),
                UniqueCharacters = CountUniqueCharacters(value.ToLower()),
                WordCount = value.Split(new[] { ' ', '\t', '\n', '\r' },
                  StringSplitOptions.RemoveEmptyEntries).Length,
                Sha256Hash = hashString,
                CharacterFrequencyMap = BuildCharacterFrequencyMap(value.ToLower())
            };

            // Save to database
            var entity = new AnalyzedString
            {
                Id = hashString,
                Value = value,
                Length = properties.Length,
                IsPalindrome = properties.IsPalindrome,
                UniqueCharacters = properties.UniqueCharacters,
                WordCount = properties.WordCount,
                Sha256Hash = properties.Sha256Hash,
                CharacterFrequencyMapJson = JsonSerializer.Serialize(properties.CharacterFrequencyMap),
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyzedStrings.Add(entity);
            await _context.SaveChangesAsync();

            return new StringResponse
            {
                Id = entity.Id,
                Value = entity.Value,
                Properties = properties,
                CreatedAt = entity.CreatedAt
            };
        }


        public async Task<StringListResponse> GetAllStringsAsync(
       bool? isPalindrome = null,
       int? minLength = null,
       int? maxLength = null,
       int? wordCount = null,
       string? containsCharacter = null)
        {
            if (minLength.HasValue && minLength < 0)
                throw new InvalidStringException("Invalid query parameter values or types");

            if (maxLength.HasValue && maxLength < 0)
                throw new InvalidStringException("Invalid query parameter values or types");

            if (minLength.HasValue && maxLength.HasValue && minLength > maxLength)
                throw new InvalidStringException("Invalid query parameter values or types");

            if (wordCount.HasValue && wordCount < 1)
                throw new InvalidStringException("Invalid query parameter values or types");

            if (!string.IsNullOrEmpty(containsCharacter) && containsCharacter.Length != 1)
                throw new InvalidStringException("Invalid query parameter values or types");


            // Start with all strings
            var query = _context.AnalyzedStrings.AsQueryable();

            // Apply filters
            if (isPalindrome.HasValue)
            {
                query = query.Where(s => s.IsPalindrome == isPalindrome.Value);
            }

            if (minLength.HasValue)
            {
                query = query.Where(s => s.Length >= minLength.Value);
            }

            if (maxLength.HasValue)
            {
                query = query.Where(s => s.Length <= maxLength.Value);
            }

            if (wordCount.HasValue)
            {
                query = query.Where(s => s.WordCount == wordCount.Value);
            }

            if (!string.IsNullOrEmpty(containsCharacter))
            {
                query = query.Where(s => s.Value.Contains(containsCharacter));
            }

            // Execute query
            var results = await query.ToListAsync();

            // Convert to response format
            var responseData = results.Select(e => MapToResponse(e)).ToList();

            // Build filters_applied object
            var filtersApplied = new Dictionary<string, object>();
            if (isPalindrome.HasValue) filtersApplied["is_palindrome"] = isPalindrome.Value;
            if (minLength.HasValue) filtersApplied["min_length"] = minLength.Value;
            if (maxLength.HasValue) filtersApplied["max_length"] = maxLength.Value;
            if (wordCount.HasValue) filtersApplied["word_count"] = wordCount.Value;
            if (!string.IsNullOrEmpty(containsCharacter)) filtersApplied["contains_character"] = containsCharacter;

            return new StringListResponse
            {
                Data = responseData,
                Count = responseData.Count,
                FiltersApplied = filtersApplied.Count > 0 ? filtersApplied : null
            };
        }

        public async Task<NaturalLanguageResponse> FilterByNaturalLanguageAsync(string query)
        {

            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidStringException("Query cannot be empty.");

            // Parse the natural language query
            var parsedFilters = ParseNaturalLanguageQuery(query);

            if (parsedFilters == null || parsedFilters.Count == 0)
            {
                throw new InvalidStringException("Unable to parse natural language query");
            }

            var conflictError = DetectConflictingFilters(parsedFilters);
            if (conflictError != null)
            {
                throw new InvalidStringTypeException(conflictError);
            }

            // Apply filters using the parsed values
            var dbQuery = _context.AnalyzedStrings.AsQueryable();

            if (parsedFilters.ContainsKey("is_palindrome"))
            {
                dbQuery = dbQuery.Where(s => s.IsPalindrome == (bool)parsedFilters["is_palindrome"]);
            }

            if (parsedFilters.ContainsKey("word_count"))
            {
                dbQuery = dbQuery.Where(s => s.WordCount == (int)parsedFilters["word_count"]);
            }

            if (parsedFilters.ContainsKey("min_length"))
            {
                dbQuery = dbQuery.Where(s => s.Length >= (int)parsedFilters["min_length"]);
            }

            if (parsedFilters.ContainsKey("max_length"))
            {
                dbQuery = dbQuery.Where(s => s.Length <= (int)parsedFilters["max_length"]);
            }

            if (parsedFilters.ContainsKey("contains_character"))
            {
                string character = (string)parsedFilters["contains_character"];
                dbQuery = dbQuery.Where(s => s.Value.Contains(character));
            }

            var results = await dbQuery.ToListAsync();
            var responseData = results.Select(e => MapToResponse(e)).ToList();

            var response = new NaturalLanguageResponse
            {
                Data = responseData,
                Count = responseData.Count,
                InterpretedQuery = new InterpretedQuery
                {
                    Original = query,
                    ParsedFilters = parsedFilters
                }
            };

            return response;
        }

        // 

        public async Task<StringResponse?> GetStringByValueAsync(string value)
        {
            var entity = await _context.AnalyzedStrings
            .FirstOrDefaultAsync(c => c.Value.ToLower() == value.ToLower());
            if (entity == null)
                throw new StringNotFoundException("String does not exist in the system");

            var frequencyMap = JsonSerializer.Deserialize<Dictionary<string, int>>(entity.CharacterFrequencyMapJson) ?? new Dictionary<string, int>();

            return new StringResponse
            {
                Id = entity.Id,
                Value = entity.Value,
                Properties = new StringProperties
                {
                    Length = entity.Length,
                    IsPalindrome = entity.IsPalindrome,
                    UniqueCharacters = entity.UniqueCharacters,
                    WordCount = entity.WordCount,
                    Sha256Hash = entity.Sha256Hash,
                    CharacterFrequencyMap = frequencyMap
                },
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<List<StringResponse>> GetAllStringsAsync()
        {
            var entities = await _context.AnalyzedStrings.ToListAsync();
            var result = new List<StringResponse>();
            foreach (var entity in entities)
            {
                var frequencyMap = JsonSerializer.Deserialize<Dictionary<string, int>>(entity.CharacterFrequencyMapJson) ?? new Dictionary<string, int>();
                result.Add(new StringResponse
                {
                    Id = entity.Id,
                    Value = entity.Value,
                    Properties = new StringProperties
                    {
                        Length = entity.Length,
                        IsPalindrome = entity.IsPalindrome,
                        UniqueCharacters = entity.UniqueCharacters,
                        WordCount = entity.WordCount,
                        Sha256Hash = entity.Sha256Hash,
                        CharacterFrequencyMap = frequencyMap
                    },
                    CreatedAt = entity.CreatedAt
                });
            }
            return result;
        }

        public async Task<bool> DeleteStringAsync(string value)
        {
            var entity = await _context.AnalyzedStrings
            .FirstOrDefaultAsync(c => c.Value.ToLower() == value.ToLower());
            if (entity == null)
                throw new StringNotFoundException("String does not exist in the system");

            _context.AnalyzedStrings.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static string ComputeSha256Hash(string value)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }


        private bool IsPalindrome(string value)
        {
            // Remove whitespace and convert to lowercase for comparison
            string cleaned = new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLower();

            // Compare with reversed version
            string reversed = new string(cleaned.Reverse().ToArray());

            return cleaned == reversed;
        }

        private int CountUniqueCharacters(string value)
        {
            // Use HashSet to get distinct characters
            return value.ToLower().Distinct().Count();
        }


        private Dictionary<string, int> BuildCharacterFrequencyMap(string value)
        {
            var frequencyMap = new Dictionary<string, int>();

            foreach (char c in value)
            {
                string key = c.ToString();

                if (frequencyMap.ContainsKey(key))
                {
                    frequencyMap[key]++;
                }
                else
                {
                    frequencyMap[key] = 1;
                }
            }

            return frequencyMap;
        }



        private string? DetectConflictingFilters(Dictionary<string, object> filters)
        {
            // Check for min_length > max_length conflict
            if (filters.ContainsKey("min_length") && filters.ContainsKey("max_length"))
            {
                int minLength = (int)filters["min_length"];
                int maxLength = (int)filters["max_length"];

                if (minLength > maxLength)
                {
                    return $"Query parsed but resulted in conflicting filters";
                }
            }

            // Check for impossible length constraints
            if (filters.ContainsKey("min_length"))
            {
                int minLength = (int)filters["min_length"];
                if (minLength < 0)
                {
                    return $"Query parsed but resulted in conflicting filters";
                }
            }

            if (filters.ContainsKey("max_length"))
            {
                int maxLength = (int)filters["max_length"];
                if (maxLength < 0)
                {
                    return $"Query parsed but resulted in conflicting filters";
                }
            }

            // Check for impossible word count
            if (filters.ContainsKey("word_count"))
            {
                int wordCount = (int)filters["word_count"];
                if (wordCount < 0)
                {
                    return $"Query parsed but resulted in conflicting filters";
                }
            }

            // No conflicts detected
            return null;
        }


        private Dictionary<string, object>? ParseNaturalLanguageQuery(string query)
        {
            var filters = new Dictionary<string, object>();
            string lowerQuery = query.ToLower();

            // Pattern 1: Check for palindrome
            if (lowerQuery.Contains("palindrome") || lowerQuery.Contains("palindromic"))
            {
                filters["is_palindrome"] = true;
            }

            // Pattern 2: Check for word count
            if (lowerQuery.Contains("single word"))
            {
                filters["word_count"] = 1;
            }
            else if (lowerQuery.Contains("two word") || lowerQuery.Contains("2 word"))
            {
                filters["word_count"] = 2;
            }
            else if (lowerQuery.Contains("three word") || lowerQuery.Contains("3 word"))
            {
                filters["word_count"] = 3;
            }

            // Pattern 3: Check for "longer than X"
            if (lowerQuery.Contains("longer than"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(lowerQuery, @"longer than (\d+)");
                if (match.Success)
                {
                    // "longer than 10" means min_length = 11
                    filters["min_length"] = int.Parse(match.Groups[1].Value) + 1;
                }
            }

            // Pattern 4: Check for "shorter than X"
            if (lowerQuery.Contains("shorter than"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(lowerQuery, @"shorter than (\d+)");
                if (match.Success)
                {
                    // "shorter than 10" means max_length = 9
                    filters["max_length"] = int.Parse(match.Groups[1].Value) - 1;
                }
            }

            // Pattern 5: Check for "at least X characters"
            if (lowerQuery.Contains("at least") && lowerQuery.Contains("character"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(lowerQuery, @"at least (\d+)");
                if (match.Success)
                {
                    filters["min_length"] = int.Parse(match.Groups[1].Value);
                }
            }

            // Pattern 6: Check for "containing the letter X"
            if (lowerQuery.Contains("containing the letter") || lowerQuery.Contains("contains the letter"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(lowerQuery, @"letter ([a-z])");
                if (match.Success)
                {
                    filters["contains_character"] = match.Groups[1].Value;
                }
            }

            // Pattern 7: Check for "strings containing X" (simpler pattern)
            if (lowerQuery.Contains("strings containing"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(lowerQuery, @"containing ([a-z])");
                if (match.Success)
                {
                    filters["contains_character"] = match.Groups[1].Value;
                }
            }

            // Pattern 8: Check for "first vowel"
            if (lowerQuery.Contains("first vowel"))
            {
                filters["contains_character"] = "a";
            }

            // Return null if no filters were parsed (triggers 400 error)
            return filters.Count > 0 ? filters : null;
        }

        private StringResponse MapToResponse(AnalyzedString entity)
        {
            var frequencyMap = string.IsNullOrEmpty(entity.CharacterFrequencyMapJson)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(entity.CharacterFrequencyMapJson)
                  ?? new Dictionary<string, int>();

            return new StringResponse
            {
                Id = entity.Id,
                Value = entity.Value,
                CreatedAt = entity.CreatedAt,
                Properties = new StringProperties
                {
                    Length = entity.Length,
                    IsPalindrome = entity.IsPalindrome,
                    UniqueCharacters = entity.UniqueCharacters,
                    WordCount = entity.WordCount,
                    Sha256Hash = entity.Sha256Hash,
                    CharacterFrequencyMap = frequencyMap
                }
            };
        }

    }
}
