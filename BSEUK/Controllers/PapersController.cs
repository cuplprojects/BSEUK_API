using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSEUK.Data;
using BSEUK.Models;

namespace BSEUK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PapersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PapersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Papers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Paper>>> GetPapers()
        {
            return await _context.Papers.ToListAsync();
        }

        // GET: api/Papers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Paper>> GetPaper(int id)
        {
            var paper = await _context.Papers.FindAsync(id);

            if (paper == null)
            {
                return NotFound();
            }

            return paper;
        }
        
        [HttpGet("GetBySem/{SemID}")]
        public async Task<ActionResult<IEnumerable<Paper>>> GetPaperBySemID(int SemID)
        {
            var papers = _context.Papers.Where(u => u.SemID == SemID).ToList();
            
            if (papers == null)
            {
                return NotFound();
            }

            return papers;
        }

        // PUT: api/Papers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaper(int id, Paper paper)
        {
            if (id != paper.PaperID)
            {
                return BadRequest();
            }

            _context.Entry(paper).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaperExists(id))
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

        // POST: api/Papers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Paper>> PostPaper(Paper paper)
        {
            paper.TotalMaxMarks =(int)paper.TheoryPaperMaxMarks + (int)paper.PracticalMaxMarks + (int)paper.InteralMaxMarks;
            _context.Papers.Add(paper);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPaper", new { id = paper.PaperID }, paper);
        }

        [HttpGet("TotalMaxMarks/{SemID}")]
        public async Task<ActionResult> GetTotalMaxMarks(int SemID)
        {
            // Fetch papers for the specified Semester ID (SemID)
            var paperScores = _context.Papers
                .Where(p => p.SemID == SemID)
                .ToList();

            if (paperScores == null || !paperScores.Any())
            {
                return NotFound();
            }

            // Add row-wise summation for each paper
            var papersWithRowTotal = paperScores
                .Select(p => new
                {
                    p.PaperID,
                    p.PaperName,
                    p.PaperCode,
                    p.PaperType,
                    p.TheoryPaperMaxMarks,
                    p.InteralMaxMarks,
                    p.PracticalMaxMarks,
                    RowTotal = p.TheoryPaperMaxMarks + p.InteralMaxMarks + p.PracticalMaxMarks
                })
                .ToList();

            // Calculate column-wise totals
            var columnWiseTotals = new
            {
                TotalTheoryPaperMaxMarks = paperScores.Sum(p => p.TheoryPaperMaxMarks),
                TotalInternalMaxMarks = paperScores.Sum(p => p.InteralMaxMarks),
                TotalPracticalMaxMarks = paperScores.Sum(p => p.PracticalMaxMarks),
                TotalRowSummation = paperScores.Sum(p => p.TheoryPaperMaxMarks + p.InteralMaxMarks + p.PracticalMaxMarks)
            };

            // Combine results
            var result = new
            {
                RowWiseSummation = papersWithRowTotal,
                ColumnWiseTotals = columnWiseTotals
            };

            return Ok(result);
        }


        // DELETE: api/Papers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaper(int id)
        {
            var paper = await _context.Papers.FindAsync(id);
            if (paper == null)
            {
                return NotFound();
            }

            _context.Papers.Remove(paper);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaperExists(int id)
        {
            return _context.Papers.Any(e => e.PaperID == id);
        }
    }
}
