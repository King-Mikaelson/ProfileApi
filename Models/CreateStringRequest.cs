using System.ComponentModel.DataAnnotations;

namespace StringAnalyzer.Models;

public class CreateStringRequest
{
    [Required(ErrorMessage = "\"value\" field is required.")]
    public required string Value { get; set; } 
}
