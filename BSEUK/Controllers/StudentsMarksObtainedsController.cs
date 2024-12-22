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
    public class StudentsMarksObtainedsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentsMarksObtainedsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/StudentsMarksObtaineds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentsMarksObtained>>> GetStudentsMarksObtaineds()
        {
            return await _context.StudentsMarksObtaineds.ToListAsync();
        }

        // GET: api/StudentsMarksObtaineds/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentsMarksObtained>> GetStudentsMarksObtained(int id)
        {
            var studentsMarksObtained = await _context.StudentsMarksObtaineds.FindAsync(id);

            if (studentsMarksObtained == null)
            {
                return NotFound();
            }

            return studentsMarksObtained;
        }

        // PUT: api/StudentsMarksObtaineds/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudentsMarksObtained(int id, StudentsMarksObtained studentsMarksObtained)
        {
            if (id != studentsMarksObtained.SmoID)
            {
                return BadRequest();
            }

            _context.Entry(studentsMarksObtained).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentsMarksObtainedExists(id))
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

        [HttpGet("GetResult/{id}")]
        public async Task<ActionResult> GetResult(int id)
        {
            // Fetch data for the specified candidate ID
            var candidateScores = _context.StudentsMarksObtaineds
                .Where(u => u.CandidateID == id)
                .ToList();

            if (candidateScores == null || !candidateScores.Any())
            {
                return NotFound();
            }

            // Add row-wise summation for each row
            var scoresWithRowTotal = candidateScores
                .Select(s => new
                {
                    s.SmoID,
                    s.CandidateID,
                    s.PaperID,
                    s.TheoryPaperMarks,
                    s.InteralMarks,
                    s.PracticalMaxMarks,
                    RowTotal = s.TheoryPaperMarks + s.InteralMarks + s.PracticalMaxMarks
                })
                .ToList();

            // Calculate column-wise totals
            var columnWiseTotals = new
            {
                TotalTheoryPaperMarks = candidateScores.Sum(s => s.TheoryPaperMarks),
                TotalInternalMarks = candidateScores.Sum(s => s.InteralMarks),
                TotalPracticalMaxMarks = candidateScores.Sum(s => s.PracticalMaxMarks),
                TotalRowSummation = candidateScores.Sum(s => s.TheoryPaperMarks + s.InteralMarks + s.PracticalMaxMarks)
            };

            // Combine results
            var result = new
            {
                RowWiseSummation = scoresWithRowTotal,
                ColumnWiseTotals = columnWiseTotals
            };

            return Ok(result);
        }

        // GET: api/StudentsMarksObtaineds/GetStudentPaperMarks/{studentId}/{paperId}
        [HttpGet("GetStudentPaperMarks/{studentId}/{paperId}")]
        public async Task<ActionResult<StudentsMarksObtained>> GetStudentPaperMarks(int studentId, int paperId)
        {
            var mark = await _context.StudentsMarksObtaineds
                .FirstOrDefaultAsync(m => m.CandidateID == studentId && m.PaperID == paperId);

            if (mark == null)
            {
                return NotFound();
            }

            return mark;
        }

        [HttpPost("GetCumulativeResult")]
        public async Task<ActionResult> GetCumulativeResult(inputforGCR igcr)
        {
            // Fetch all relevant data
            var students = _context.Candidates
                .Where(u => u.SemID == igcr.SemID && u.SesID == igcr.SesID)
                .Select(u => new
                {
                    u.CandidateID,
                    u.CandidateName,
                    u.RollNumber,
                    u.Group,
                    u.FName,
                    u.MName,
                    u.InstitutionName,
                    u.DOB
                })
                .ToList();

            var papers = _context.Papers
                .Where(u => u.SemID == igcr.SemID)
                .Select(p => new
                {
                    p.PaperID,
                    p.PaperName,
                    p.PaperType,
                    p.PaperCode
                })
                .ToList();

            var marks = _context.StudentsMarksObtaineds
                .Where(m => students.Select(s => s.CandidateID).Contains(m.CandidateID) &&
                            papers.Select(p => p.PaperID).Contains(m.PaperID))
                .ToList();

            // Prepare cumulative result
            var cumulativeResults = new List<Dictionary<string, object>>();

            foreach (var student in students)
            {
                var resultRow = new Dictionary<string, object>
        {
            { "CandidateID", student.CandidateID },
            { "CandidateName", student.CandidateName },
            { "RollNumber", student.RollNumber },
            { "Group", student.Group },
            { "FName", student.FName },
            { "MName", student.MName },
            { "InstitutionName", student.InstitutionName },
            { "DOB", student.DOB }
        };

                foreach (var paper in papers)
                {
                    var studentMark = marks.FirstOrDefault(m => m.CandidateID == student.CandidateID && m.PaperID == paper.PaperID);

                    // Add only if the value is greater than 0
                    if (studentMark?.TheoryPaperMarks > 0)
                        resultRow[$"{paper.PaperCode}_TheoryMarks"] = studentMark.TheoryPaperMarks;

                    if (studentMark?.InteralMarks > 0)
                        resultRow[$"{paper.PaperCode}_InternalMarks"] = studentMark.InteralMarks;

                    if (studentMark?.PracticalMaxMarks > 0)
                        resultRow[$"{paper.PaperCode}_PracticalMarks"] = studentMark.PracticalMaxMarks;
                }

                cumulativeResults.Add(resultRow);
            }

            // Return results
            return Ok(cumulativeResults);
        }



        /*[HttpPost("GetCumulativeResult")]
        public async Task<ActionResult> GetCumulativeResult(inputforGCR igcr)
        {
            // Fetch all relevant data
            var students = _context.Candidates
                .Where(u => u.SemID == igcr.SemID && u.SesID == igcr.SesID)
                .Select(u => new {
                    u.CandidateID,
                    u.CandidateName,
                    u.RollNumber,
                    u.Group,
                    u.FName,
                    u.MName,
                    u.InstitutionName,
                    u.DOB
                })
                .ToList();

            var papers = _context.Papers
                .Where(u => u.SemID == igcr.SemID)
                .Select(p => new {
                    p.PaperID,
                    p.PaperName,
                    p.PaperCode,
                    p.PaperType
                })
                .ToList();

            var marks = _context.StudentsMarksObtaineds
                .Where(m => students.Select(s => s.CandidateID).Contains(m.CandidateID) &&
                            papers.Select(p => p.PaperID).Contains(m.PaperID))
                .ToList();

            // Prepare result in the specified format
            var categorizedResults = new Dictionary<string, object>();

            foreach (var paper in papers)
            {
                // Initialize the outer object for each PaperCode
                if (!categorizedResults.ContainsKey(paper.PaperCode.ToString()))
                {
                    categorizedResults[paper.PaperCode.ToString()] = new
                    {
                        PaperDetails = new
                        {
                            paper.PaperID,
                            paper.PaperName,
                            paper.PaperCode,
                            paper.PaperType
                        },
                        Students = new Dictionary<string, Dictionary<string, int>>()
                    };
                }

                var studentMarksDict = ((dynamic)categorizedResults[paper.PaperCode.ToString()]).Students;

                foreach (var student in students)
                {
                    var studentMark = marks.FirstOrDefault(m => m.CandidateID == student.CandidateID && m.PaperID == paper.PaperID);

                    if (studentMark != null)
                    {
                        // Add the candidate's marks to the inner object for the current PaperCode
                        var studentData = new Dictionary<string, int>();

                        if (studentMark.TheoryPaperMarks > 0)
                            studentData["Theory"] = (int)studentMark.TheoryPaperMarks;

                        if (studentMark.InteralMarks > 0)
                            studentData["Internal"] = (int)studentMark.InteralMarks;

                        if (studentMark.PracticalMaxMarks > 0)
                            studentData["Practical"] = (int)studentMark.PracticalMaxMarks;

                        if (studentData.Count > 0) // Only add if there are non-zero marks
                        {
                            studentMarksDict[student.CandidateID.ToString()] = studentData;
                        }
                    }
                }
            }

            // Combine the results with student data
            var result = new
            {
                Students = students.Select(s => new
                {
                    s.CandidateID,
                    s.CandidateName,
                    s.RollNumber,
                    s.Group,
                    s.FName,
                    s.MName,
                    s.InstitutionName,
                    s.DOB
                }),
                Papers = papers.Select(p => new
                {
                    p.PaperID,
                    p.PaperName,
                    p.PaperCode,
                    p.PaperType
                }),
                CategorizedResults = categorizedResults
            };

            return Ok(result);
        }
*/



        // POST: api/StudentsMarksObtaineds
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /*[HttpPost]
        public async Task<ActionResult<StudentsMarksObtained>> PostStudentsMarksObtained(StudentsMarksObtained studentsMarksObtained)
        {
            _context.StudentsMarksObtaineds.Add(studentsMarksObtained);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudentsMarksObtained", new { id = studentsMarksObtained.SmoID }, studentsMarksObtained);
        }*/

        [HttpPost]
        public async Task<ActionResult<StudentsMarksObtained>> PostStudentsMarksObtained(StudentsMarksObtained studentsMarksObtained)
        {
            // Check if the record exists for the given CandidateID and PaperID
            var existingRecord = _context.StudentsMarksObtaineds
                .FirstOrDefault(s => s.CandidateID == studentsMarksObtained.CandidateID && s.PaperID == studentsMarksObtained.PaperID);

            if (existingRecord != null)
            {
                // Update only the fields that have been provided (not null)
                if (studentsMarksObtained.TheoryPaperMarks.HasValue)
                {
                    existingRecord.TheoryPaperMarks = studentsMarksObtained.TheoryPaperMarks;
                }

                if (studentsMarksObtained.InteralMarks.HasValue)
                {
                    existingRecord.InteralMarks = studentsMarksObtained.InteralMarks;
                }

                if (studentsMarksObtained.PracticalMaxMarks.HasValue)
                {
                    existingRecord.PracticalMaxMarks = studentsMarksObtained.PracticalMaxMarks;
                }

                // Save changes to the database
                _context.Entry(existingRecord).State = EntityState.Modified;
            }
            else
            {
                // Add a new record if no existing record is found
                _context.StudentsMarksObtaineds.Add(studentsMarksObtained);
            }

            await _context.SaveChangesAsync();

            // Return the updated or newly created record
            return CreatedAtAction("GetStudentsMarksObtained", new { id = studentsMarksObtained.SmoID }, studentsMarksObtained);
        }

        // DELETE: api/StudentsMarksObtaineds/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudentsMarksObtained(int id)
        {
            var studentsMarksObtained = await _context.StudentsMarksObtaineds.FindAsync(id);
            if (studentsMarksObtained == null)
            {
                return NotFound();
            }

            _context.StudentsMarksObtaineds.Remove(studentsMarksObtained);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudentsMarksObtainedExists(int id)
        {
            return _context.StudentsMarksObtaineds.Any(e => e.SmoID == id);
        }

        [HttpGet("GetStudentResult/{rollNumber}")]
        public async Task<ActionResult> GetStudentResult(string rollNumber)
        {
            // First get the candidate
            var candidate = await _context.Candidates
                .FirstOrDefaultAsync(c => c.RollNumber == rollNumber);

            if (candidate == null)
            {
                return NotFound("Candidate not found");
            }

            // Get all papers for the candidate's semester
            var papers = await _context.Papers
                .Where(p => p.SemID == candidate.SemID)
                .Select(p => new
                {
                    p.PaperID,
                    p.PaperCode,
                    p.PaperName,
                    TheoryMaxMarks = p.TheoryPaperMaxMarks ?? 0,
                    InternalMaxMarks = p.InteralMaxMarks ?? 0,
                    PracticalMaxMarks = p.PracticalMaxMarks ?? 0,
                    p.PaperType,
                    TotalMaxMarks = (p.TheoryPaperMaxMarks ?? 0) + (p.InteralMaxMarks ?? 0) + (p.PracticalMaxMarks ?? 0)
                })
                .ToListAsync();

            // Get marks for all papers
            var marks = await _context.StudentsMarksObtaineds
                .Where(m => m.CandidateID == candidate.CandidateID)
                .ToListAsync();

            // Prepare result
            var result = new
            {
                StudentDetails = new
                {
                    CandidateID = candidate.CandidateID,
                    CandidateName = candidate.CandidateName,
                    RollNumber = candidate.RollNumber,
                    FName = candidate.FName,
                    MName = candidate.MName,
                    Group = candidate.Group,
                    InstitutionName = candidate.InstitutionName,
                    DOB = candidate.DOB
                },
                MarksDetails = papers.Select(paper =>
                {
                    var studentMark = marks.FirstOrDefault(m => m.PaperID == paper.PaperID);
                    return new
                    {
                        paper.PaperCode,
                        paper.PaperName,
                        MaxMarks = paper.TotalMaxMarks,
                        TheoryMarks = studentMark?.TheoryPaperMarks ?? 0,
                        InternalMarks = studentMark?.InteralMarks ?? 0,
                        PracticalMarks = studentMark?.PracticalMaxMarks ?? 0,
                        Total = (studentMark?.TheoryPaperMarks ?? 0) + 
                               (studentMark?.InteralMarks ?? 0) + 
                               (studentMark?.PracticalMaxMarks ?? 0)
                    };
                }).ToList(),
                TotalMarks = marks.Sum(m => m.TheoryPaperMarks + m.InteralMarks + m.PracticalMaxMarks),
                MaximumMarks = papers.Sum(p => p.TotalMaxMarks)
            };

            return Ok(result);
        }

        [HttpPost("GetBulkResult")]
        public async Task<ActionResult> GetBulkResult(BulkResultRequest request)
        {
            // Get all candidates for the session and semester
            var candidates = await _context.Candidates
                .Where(c => c.SesID == request.SessionId && c.SemID == request.SemesterId)
                .ToListAsync();

            if (!candidates.Any())
            {
                return NotFound("No candidates found for the specified session and semester");
            }

            // Get all papers for the semester
            var papers = await _context.Papers
                .Where(p => p.SemID == request.SemesterId)
                .Select(p => new
                {
                    p.PaperID,
                    p.PaperCode,
                    p.PaperName,
                    TheoryMaxMarks = p.TheoryPaperMaxMarks ?? 0,
                    InternalMaxMarks = p.InteralMaxMarks ?? 0,
                    PracticalMaxMarks = p.PracticalMaxMarks ?? 0,
                    p.PaperType,
                    TotalMaxMarks = (p.TheoryPaperMaxMarks ?? 0) + (p.InteralMaxMarks ?? 0) + (p.PracticalMaxMarks ?? 0)
                })
                .ToListAsync();

            // Get all marks for these candidates
            var candidateIds = candidates.Select(c => c.CandidateID).ToList();
            var allMarks = await _context.StudentsMarksObtaineds
                .Where(m => candidateIds.Contains(m.CandidateID))
                .ToListAsync();

            // Prepare results for each candidate
            var results = candidates.Select(candidate =>
            {
                var studentMarks = allMarks.Where(m => m.CandidateID == candidate.CandidateID);
                
                return new
                {
                    StudentDetails = new
                    {
                        CandidateID = candidate.CandidateID,
                        CandidateName = candidate.CandidateName,
                        RollNumber = candidate.RollNumber,
                        FName = candidate.FName,
                        MName = candidate.MName,
                        Group = candidate.Group,
                        InstitutionName = candidate.InstitutionName,
                        DOB = candidate.DOB
                    },
                    MarksDetails = papers.Select(paper =>
                    {
                        var mark = studentMarks.FirstOrDefault(m => m.PaperID == paper.PaperID);
                        return new
                        {
                            paper.PaperCode,
                            paper.PaperName,
                            MaxMarks = paper.TotalMaxMarks,
                            TheoryMarks = mark?.TheoryPaperMarks ?? 0,
                            InternalMarks = mark?.InteralMarks ?? 0,
                            PracticalMarks = mark?.PracticalMaxMarks ?? 0,
                            Total = (mark?.TheoryPaperMarks ?? 0) + 
                                   (mark?.InteralMarks ?? 0) + 
                                   (mark?.PracticalMaxMarks ?? 0)
                        };
                    }).ToList(),
                    TotalMarks = studentMarks.Sum(m => m.TheoryPaperMarks + m.InteralMarks + m.PracticalMaxMarks),
                    MaximumMarks = papers.Sum(p => p.TotalMaxMarks)
                };
            }).ToList();

            return Ok(results);
        }

        public class BulkResultRequest
        {
            public int SessionId { get; set; }
            public int SemesterId { get; set; }
        }
    }

    public class inputforGCR
    {
        public int SemID { get; set; }

        public int SesID { get; set; }
    }
}
