using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSEUK.Data;
using BSEUK.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace BSEUK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CandidatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CandidatesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Candidates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candidate>>> GetCandidates()
        {
            return await _context.Candidates.ToListAsync();
        }

        // GET: api/Candidates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Candidate>> GetCandidate(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);

            if (candidate == null)
            {
                return NotFound();
            }

            return candidate;
        }


        [HttpPost("GetStudents")]
        public async Task<ActionResult<IEnumerable<Candidate>>> Getstudents(inputdata info)
        {
            var candidates = await _context.Candidates.Where(u => u.SesID == info.SesID && u.SemID == info.SemID).ToListAsync();

            if (candidates == null)
            {
                return NotFound();
            }

            return candidates;
        }

        // PUT: api/Candidates/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCandidate(int id, Candidate candidate)
        {
            if (id != candidate.CandidateID)
            {
                return BadRequest();
            }

            _context.Entry(candidate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CandidateExists(id))
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

        // POST: api/Candidates
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Candidate>> PostCandidate(Candidate candidate)
        {
            var existingCount = _context.Candidates.Count(c => c.RollNumber == candidate.RollNumber && c.SesID == candidate.SesID);

            if (existingCount >= 2)
            {
                return BadRequest("Candidate is already entrolled for 2 semesters in this session");
            }

            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCandidate", new { id = candidate.CandidateID }, candidate);
        }




        // DELETE: api/Candidates/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidate(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
            {
                return NotFound();
            }

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CandidateExists(int id)
        {
            return _context.Candidates.Any(e => e.CandidateID == id);
        }

        [HttpGet("GetByDetails/{rollNumber}/{sesId}/{semId}")]
        public async Task<ActionResult<object>> GetCandidateByDetails(string rollNumber, int sesId, int semId)
        {
            var candidate = await _context.Candidates
                .Where(c => c.RollNumber == rollNumber && c.SesID == sesId && c.SemID == semId)
                .Join(
                    _context.Sessions,
                    c => c.SesID,
                    s => s.SesID,
                    (c, s) => new { c, s }
                )
                .Join(
                    _context.Semesters,
                    cs => cs.c.SemID,
                    sem => sem.SemID,
                    (cs, sem) => new
                    {
                        cs.c.CandidateID,
                        cs.c.CandidateName,
                        cs.c.Group,
                        cs.c.RollNumber,
                        cs.c.FName,
                        cs.c.MName,
                        cs.c.DOB,
                        cs.c.InstitutionName,
                        cs.c.Category,
                        cs.c.PapersOpted,
                        SessionName = cs.s.SessionName,
                        SemesterName = sem.SemesterName
                    }
                )
                .FirstOrDefaultAsync();

            if (candidate == null)
            {
                return NotFound($"No candidate found with roll number: {rollNumber} in session: {sesId} and semester: {semId}");
            }

            return Ok(candidate);
        }

        [HttpPut("UpdateByDetails/{rollNumber}/{sesId}/{semId}")]
        public async Task<IActionResult> UpdateCandidateByDetails(string rollNumber, int sesId, int semId, Candidate updatedCandidate)
        {
            var candidate = await _context.Candidates
                .FirstOrDefaultAsync(c => c.RollNumber == rollNumber && c.SesID == sesId && c.SemID == semId);

            if (candidate == null)
            {
                return NotFound($"No candidate found with roll number: {rollNumber} in session: {sesId} and semester: {semId}");
            }

            // Update fields
            candidate.CandidateName = updatedCandidate.CandidateName;
            candidate.Group = updatedCandidate.Group;
            candidate.FName = updatedCandidate.FName;
            candidate.MName = updatedCandidate.MName;
            candidate.DOB = updatedCandidate.DOB;
            candidate.InstitutionName = updatedCandidate.InstitutionName;
            candidate.Category = updatedCandidate.Category;
            candidate.PapersOpted = updatedCandidate.PapersOpted;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Candidate updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating: {ex.Message}");
            }
        }
    }

    public class inputdata
    {
        public int SesID { get; set; }
        public int SemID { get; set; }
    }
}
