using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using StringAnalyzer.Data;
using StringAnalyzer.Models;
using StringAnalyzer.Services;

namespace StringAnalyzer.Controllers
{
    [Route("strings")]
    [ApiController]
    public class StringAnalyzerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringAnalyzerService _service;

        public StringAnalyzerController(ApplicationDbContext context, IStringAnalyzerService service)
        {
            _context = context;
            _service = service;
        }

        // GET: api/StringAnalyzer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnalyzedString>>> GetAnalyzedStrings()
        {
            return await _context.AnalyzedStrings.ToListAsync();
        }

        // GET: api/StringAnalyzer/{string_value}
        [HttpGet("{string_value}")]
        public async Task<ActionResult<AnalyzedString>> GetStringByValue(string string_value)
        {
            var analyzed = await _service.GetStringByValueAsync(string_value);
            return Ok(analyzed);
        }

        // PUT: api/StringAnalyzer/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnalyzedString(string id, AnalyzedString analyzedString)
        {
            if (id != analyzedString.Id)
            {
                return BadRequest();
            }

            _context.Entry(analyzedString).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnalyzedStringExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/StringAnalyzer
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AnalyzedString>> PostAnalyzedString([FromBody, Required] CreateStringRequest request)
        {

            // No try/catch needed
            var analyzed = await _service.AnalyzeStringAsync(request.Value);
            return CreatedAtAction(nameof(GetStringByValue), new { string_value = analyzed.value }, analyzed);
        }


        [HttpGet]
        public async Task<ActionResult<StringListResponse>> GetAllStrings(
            [FromQuery] bool? is_palindrome = null,
            [FromQuery] int? min_length = null,
            [FromQuery] int? max_length = null,
            [FromQuery] int? word_count = null,
            [FromQuery] string? contains_character = null)
        {
            var result = await _service.GetAllStringsAsync(
                isPalindrome: is_palindrome,
                minLength: min_length,
                maxLength: max_length,
                wordCount: word_count,
                containsCharacter: contains_character);

            return Ok(result);
        }


        [HttpGet("filter-by-natural-language")]
        public async Task<IActionResult> FilterByNaturalLanguage([FromQuery] string query)
        {
            var response = await _service.FilterByNaturalLanguageAsync(query);
            return Ok(response);
        }

        // DELETE: api/StringAnalyzer/5
        [HttpDelete("{string_value}")]
        public async Task<IActionResult> DeleteAnalyzedString(string string_value)
        {
            await _service.DeleteStringAsync(string_value);
            return NoContent();
        }

        private bool AnalyzedStringExists(string id)
        {
            return _context.AnalyzedStrings.Any(e => e.Id == id);
        }
    }
}
