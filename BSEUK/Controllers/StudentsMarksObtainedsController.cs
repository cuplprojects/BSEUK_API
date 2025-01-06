using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSEUK.Data;
using BSEUK.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Data;
using BSEUK.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;



namespace BSEUK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentsMarksObtainedsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILoggerService _loggerService;

        public StudentsMarksObtainedsController(AppDbContext context, ILoggerService loggerService)
        {
            _context = context;
            _loggerService = loggerService;
        }

        // GET: api/StudentsMarksObtaineds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentsMarksObtained>>> GetStudentsMarksObtaineds()
        {
            return await _context.StudentsMarksObtaineds.ToListAsync();
        }

        [HttpGet("GetStudentPaperMarks/{paperID}")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentPaperMarks(int paperID)
        {
            // Fetch the paper details based on paperID
            var paper = await _context.Papers.FirstOrDefaultAsync(u => u.PaperID == paperID);
            if (paper == null)
            {
                return NotFound("Paper not found.");
            }

            string paperCode = paper.PaperCode.ToString();

            // Fetch all candidates
            var candidates = await _context.Candidates.ToListAsync(); // Bring candidates into memory

            // Check PaperType and conditionally filter candidates
            var filteredCandidates = paper.PaperType == 1
                ? candidates
                    .Where(u => u.PapersOpted.Split(',').Contains(paperCode) && u.SemID == paper.SemID)
                    .Select(c => new { c.CandidateID, c.CandidateName, c.RollNumber }) // Include additional candidate details if needed
                    .ToList()
                : candidates
                    .Where(u => u.SemID == paper.SemID)
                    .Select(c => new { c.CandidateID, c.CandidateName, c.RollNumber })
                    .ToList();

            if (!filteredCandidates.Any())
            {
                return NotFound("No candidates found for the specified paper.");
            }

            // Fetch marks for all the candidates for the specified paper
            var marks = await _context.StudentsMarksObtaineds
                .Where(u => filteredCandidates.Select(c => c.CandidateID).Contains(u.CandidateID) && u.PaperID == paperID)
                .ToListAsync();

            // Perform a left join in-memory
            var result = filteredCandidates
                .GroupJoin(
                    marks,
                    candidate => candidate.CandidateID,
                    mark => mark.CandidateID,
                    (candidate, candidateMarks) => new
                    {
                        CandidateID = candidate.CandidateID,
                        CandidateName = candidate.CandidateName,
                        CandidateRollNumber = candidate.RollNumber,
                        PaperType = paper.PaperType,
                        Marks = candidateMarks.FirstOrDefault() // Take the first match or null
                    }
                )
                .ToList();

            return Ok(result);
        }


        /*        [HttpGet("GetAllYearsResult/{rollNumber}")]
                public async Task<ActionResult<object>> GetAllTotals(string rollNumber)
                {
                    var candidates = await _context.Candidates.Where(u => u.RollNumber == rollNumber).OrderBy(u=>u.SemID).ToListAsync();
                    if (!candidates.Any())
                    {
                        return NotFound($"No Student found with this Roll Number: {rollNumber}");
                    }

                    var results = new List<object>();

                    foreach (var can in candidates)
                    {
                        // Split the comma-separated paper codes into a list
                        var optedPaperCodes = can.PapersOpted.Split(',', StringSplitOptions.RemoveEmptyEntries);

                        // Calculate totals considering the PaperType condition
                        var TheoryTotal = await _context.Papers
                            .Where(u => u.SemID == can.SemID &&
                                        (u.PaperType != 1 || (u.PaperType == 1 && optedPaperCodes.Contains(u.PaperCode.ToString()))))
                            .SumAsync(u => u.TheoryPaperMaxMarks);

                        var PracticalTotal = await _context.Papers
                            .Where(u => u.SemID == can.SemID &&
                                        (u.PaperType != 1 || (u.PaperType == 1 && optedPaperCodes.Contains(u.PaperCode.ToString()))))
                            .SumAsync(u => u.PracticalMaxMarks);

                        var InternalTotal = await _context.Papers
                            .Where(u => u.SemID == can.SemID &&
                                        (u.PaperType != 1 || (u.PaperType == 1 && optedPaperCodes.Contains(u.PaperCode.ToString()))))
                            .SumAsync(u => u.InteralMaxMarks);

                        var papertotals = new List<object>();
                        // Summation of Theory, Internal, and Practical marks
                        var theoryMarks = await _context.StudentsMarksObtaineds
                            .Where(u => u.CandidateID == can.CandidateID)
                            .SumAsync(u => u.TheoryPaperMarks);


                        var internalMarks = await _context.StudentsMarksObtaineds
                            .Where(u => u.CandidateID == can.CandidateID)
                            .SumAsync(u => u.InteralMarks);

                        var practicalMarks = await _context.StudentsMarksObtaineds
                            .Where(u => u.CandidateID == can.CandidateID)
                            .SumAsync(u => u.PracticalMarks);

                        var OverallTotalMarks = theoryMarks + internalMarks + practicalMarks;
                        var OverallTotalMaxMarks = TheoryTotal + PracticalTotal + InternalTotal;

                        var Status = OverallTotalMarks >= (OverallTotalMaxMarks / 2) ? "Pass" : "Fail";

                        results.Add(new
                        {
                            CandidateId = can.CandidateID,
                            CandidateName = can.CandidateName,
                            SemID = can.SemID,
                            RollNumber = can.RollNumber,
                            TotalTheoryMaxMarks = TheoryTotal,
                            TotalTheoryMarks = theoryMarks,
                            TotalInternalMaxMarks = InternalTotal,
                            TotalInternalMarks = internalMarks,
                            TotalPracticalMaxMarks = PracticalTotal,
                            TotalPracticalMarks = practicalMarks,
                            OverallTotalMarks,
                            OverallTotalMaxMarks,
                            Status
                        });
                    }

                    return Ok(results);
                }*/


        [HttpGet("GetAllYearsResult/{rollNumber}")]
        public async Task<ActionResult<object>> GetAllTotals(string rollNumber)
        {
            var candidates = await _context.Candidates.Where(u => u.RollNumber == rollNumber).OrderBy(u => u.SemID).ToListAsync();
            if (!candidates.Any())
            {
                return NotFound($"No Student found with this Roll Number: {rollNumber}");
            }

            var results = new List<dynamic>();

            foreach (var can in candidates)
            {
                var optedPaperCodes = can.PapersOpted.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var TheoryTotal = await _context.Papers
                    .Where(u => u.SemID == can.SemID &&
                                (u.PaperType != 1 || (u.PaperType == 1 && optedPaperCodes.Contains(u.PaperCode.ToString()))))
                    .SumAsync(u => u.TheoryPaperMaxMarks);

                var PracticalTotal = await _context.Papers
                    .Where(u => u.SemID == can.SemID &&
                                (u.PaperType != 1 || (u.PaperType == 1 && optedPaperCodes.Contains(u.PaperCode.ToString()))))
                    .SumAsync(u => u.PracticalMaxMarks);

                var InternalTotal = await _context.Papers
                    .Where(u => u.SemID == can.SemID &&
                                (u.PaperType != 1 || (u.PaperType == 1 && optedPaperCodes.Contains(u.PaperCode.ToString()))))
                    .SumAsync(u => u.InteralMaxMarks);



                var theoryMarks = await _context.StudentsMarksObtaineds
                    .Where(u => u.CandidateID == can.CandidateID)
                    .SumAsync(u => u.TheoryPaperMarks);

                var internalMarks = await _context.StudentsMarksObtaineds
                    .Where(u => u.CandidateID == can.CandidateID)
                    .SumAsync(u => u.InteralMarks);

                var practicalMarks = await _context.StudentsMarksObtaineds
                    .Where(u => u.CandidateID == can.CandidateID)
                    .SumAsync(u => u.PracticalMarks);

                var OverallTotalMarks = theoryMarks + internalMarks + practicalMarks;
                var OverallTotalMaxMarks = TheoryTotal + PracticalTotal + InternalTotal;

                var Status = OverallTotalMarks >= (OverallTotalMaxMarks / 2) ? "Pass" : "Fail";

                results.Add(new
                {
                    CandidateId = can.CandidateID,
                    CandidateName = can.CandidateName,
                    SemID = can.SemID,
                    RollNumber = can.RollNumber,
                    TotalTheoryMaxMarks = TheoryTotal,
                    TotalTheoryMarks = theoryMarks,
                    TotalInternalMaxMarks = InternalTotal,
                    TotalInternalMarks = internalMarks,
                    TotalPracticalMaxMarks = PracticalTotal,
                    TotalPracticalMarks = practicalMarks,
                    OverallTotalMarks,
                    OverallTotalMaxMarks,
                    Status
                });
            }

            // Calculate totals from the results
            var totalTheoryMarks = results.Where(u => u.SemID != 4).Sum(r => r.TotalTheoryMaxMarks);
            var totalInternalMarks = results.Where(u => u.SemID != 4).Sum(r => r.TotalInternalMaxMarks);
            var totalSemMarksforInternal = totalTheoryMarks + totalInternalMarks;
            var totalPracticalMarks = results.Sum(r => r.TotalPracticalMaxMarks) + results.Where(u => u.SemID == 4).Sum(r => r.TotalTheoryMaxMarks) + results.Where(u => u.SemID == 4).Sum(r => r.TotalInternalMaxMarks);
            var total = totalSemMarksforInternal + totalPracticalMarks;

            return Ok(new
            {
                Results = results,
                SemMarks = totalSemMarksforInternal,
                TotalPracticalMarks = totalPracticalMarks,
                Total = total
            });
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

        /*[HttpGet("GetResult/{id}")]
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
        }*/

        /*[HttpPost("GetStudentResult")]
        public async Task<ActionResult> GetResult(studentinfo info)
        {
            var studentDetails = _context.Candidates
    .Where(u => u.RollNumber == info.RollNumber && u.SemID == info.SemesterId && u.SesID == info.SessionId)
    .Join(
        _context.Sessions, // Join with Sessions table
        candidate => candidate.SesID, // Candidate's foreign key to Sessions
        session => session.SesID,         // Primary key in Sessions table
        (candidate, session) => new
        {
            candidate,
            session // Include session details
        }
    )
    .Join(
        _context.Semesters, // Join with Semester table
        cs => cs.candidate.SemID,  // Candidate's foreign key to Semester
        semester => semester.SemID, // Primary key in Semester table
        (cs, semester) => new
        {
            cs.candidate.CandidateID,
            cs.candidate.CandidateName,
            cs.candidate.RollNumber,
            cs.candidate.FName,
            cs.candidate.MName,
            cs.candidate.InstitutionName,
            cs.candidate.Group,
            cs.session.SessionName, // Assuming session has a SessionName field
            semester.SemesterName,  // Assuming semester has a SemesterName field
        }
    )
    .FirstOrDefault(); // Since you're looking for a single student

            // Fetch data for the specified candidate ID and join with the Papers table
            var candidateScores = _context.StudentsMarksObtaineds
                .Where(u => u.CandidateID == studentDetails.CandidateID)
                .Join(
                    _context.Papers, // Joining with the Papers table
                    marks => marks.PaperID, // Foreign key in StudentsMarksObtaineds
                    paper => paper.PaperID, // Primary key in Papers
                    (marks, paper) => new
                    {
                        marks.SmoID,
                        marks.CandidateID,
                        marks.PaperID,
                        marks.TheoryPaperMarks,
                        marks.InteralMarks,
                        marks.PracticalMarks,
                        paper.PaperName,
                        paper.TheoryPaperMaxMarks,
                        paper.PracticalMaxMarks,
                        paper.InteralMaxMarks
                    }
                )
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
                    s.PaperName,
                    s.TheoryPaperMarks,
                    s.InteralMarks,
                    s.PracticalMarks,
                    s.TheoryPaperMaxMarks,
                    s.PracticalMaxMarks,
                    s.InteralMaxMarks,
                    RowTotal = s.TheoryPaperMarks + s.InteralMarks + s.PracticalMaxMarks,
                    RowMaxTotal = s.TheoryPaperMaxMarks + s.InteralMaxMarks + s.PracticalMaxMarks
                })
                .ToList();

            // Calculate column-wise totals
            var columnWiseTotals = new
            {
                MaxTheoryMarks = candidateScores.Sum(s => s.TheoryPaperMaxMarks),
                TotalTheoryPaperMarks = candidateScores.Sum(s => s.TheoryPaperMarks),
                MaxInternalMarks = candidateScores.Sum(s => s.InteralMaxMarks),
                TotalInternalMarks = candidateScores.Sum(s => s.InteralMarks),
                MaxPracticalMarks = candidateScores.Sum(s => s.PracticalMaxMarks),
                TotalPracticalMarks = candidateScores.Sum(s => s.PracticalMarks),
                TotalCandidateMarks = candidateScores.Sum(s => s.TheoryPaperMarks + s.InteralMarks + s.PracticalMarks),
                TotalMaxMarks = candidateScores.Sum(s => s.TheoryPaperMaxMarks + s.InteralMaxMarks + s.PracticalMaxMarks)
            };

            // Combine results
            var result = new
            {
                RowWiseSummation = scoresWithRowTotal,
                ColumnWiseTotals = columnWiseTotals,
                studentDetails = studentDetails,
            };

            return Ok(result);
        }*/


        [HttpPost("GetStudentResult")]
        public async Task<ActionResult> GetResult(studentinfo info)
        {
            var studentDetails = _context.Candidates
                .Where(u => u.RollNumber == info.RollNumber && u.SemID == info.SemesterId && u.SesID == info.SessionId)
                .Join(
                    _context.Sessions,
                    candidate => candidate.SesID,
                    session => session.SesID,
                    (candidate, session) => new { candidate, session }
                )
                .Join(
                    _context.Semesters,
                    cs => cs.candidate.SemID,
                    semester => semester.SemID,
                    (cs, semester) => new
                    {
                        cs.candidate.CandidateID,
                        cs.candidate.CandidateName,
                        cs.candidate.RollNumber,
                        cs.candidate.FName,
                        cs.candidate.MName,
                        cs.candidate.InstitutionName,
                        cs.candidate.Group,
                        cs.session.SessionName,
                        semester.SemesterName,
                    }
                )
                .FirstOrDefault();

            if (studentDetails == null)
            {
                return NotFound(new { Message = "Student not found." });
            }

            var papers = _context.Papers.Where(u => u.SemID == info.SemesterId).ToList();

            var candidateScores = _context.StudentsMarksObtaineds
                .Where(u => u.CandidateID == studentDetails.CandidateID)
                .Join(
                    _context.Papers,
                    marks => marks.PaperID,
                    paper => paper.PaperID,
                    (marks, paper) => new
                    {
                        marks.SmoID,
                        marks.CandidateID,
                        marks.PaperID,
                        paper.PaperType,
                        marks.TheoryPaperMarks,
                        marks.InteralMarks,
                        marks.PracticalMarks,
                        paper.PaperName,
                        paper.PaperCode,
                        paper.TheoryPaperMaxMarks,
                        paper.PracticalMaxMarks,
                        paper.InteralMaxMarks,
                        marks.IsAbsent,
                    }
                )
                .ToList();

            if (!candidateScores.Any())
            {
                return NotFound(new { Message = "No scores found for the student." });
            }

            // Row-wise summation
            var scoresWithRowTotal = candidateScores
    .Select(s => new
    {
        s.SmoID,
        s.CandidateID,
        s.PaperID,
        s.PaperCode,
        s.PaperName,
        s.PaperType,
        s.IsAbsent,
        TheoryPaperMarks = s.TheoryPaperMarks ?? 0,
        InternalMarks = s.InteralMarks ?? 0,
        PracticalMarks = s.PracticalMarks ?? 0,
        TheoryPaperMaxMarks = s.TheoryPaperMaxMarks ?? 0,
        PracticalMaxMarks = s.PracticalMaxMarks ?? 0,
        InternalMaxMarks = s.InteralMaxMarks ?? 0,
        RowTotal = (s.TheoryPaperMarks ?? 0) + (s.InteralMarks ?? 0) + (s.PracticalMarks ?? 0),
        RowMaxTotal = (s.TheoryPaperMaxMarks ?? 0) + (s.InteralMaxMarks ?? 0) + (s.PracticalMaxMarks ?? 0)
    })
    .Select(r => new
    {
        r.SmoID,
        r.CandidateID,
        r.PaperID,
        r.PaperName,
        r.PaperType,
        r.PaperCode,
        r.IsAbsent,
        r.TheoryPaperMarks,
        r.InternalMarks,
        r.PracticalMarks,
        r.TheoryPaperMaxMarks,
        r.PracticalMaxMarks,
        r.InternalMaxMarks,
        r.RowTotal,
        r.RowMaxTotal,
        PaperRemarks = r.RowTotal >= (r.RowMaxTotal / 2) ? "उत्तीर्ण" : "असफल"
    })
    .ToList();


            // Column-wise totals
            var columnWiseTotals = new
            {
                MaxTheoryMarks = scoresWithRowTotal.Sum(s => s.TheoryPaperMaxMarks),
                TotalTheoryPaperMarks = scoresWithRowTotal.Sum(s => s.TheoryPaperMarks),
                MaxInternalMarks = scoresWithRowTotal.Sum(s => s.InternalMaxMarks),
                TotalInternalMarks = scoresWithRowTotal.Sum(s => s.InternalMarks),
                MaxPracticalMarks = scoresWithRowTotal.Sum(s => s.PracticalMaxMarks),
                TotalPracticalMarks = scoresWithRowTotal.Sum(s => s.PracticalMarks),
                TotalCandidateMarks = scoresWithRowTotal.Sum(s => s.RowTotal),
                TotalMaxMarks = scoresWithRowTotal.Sum(s => s.RowMaxTotal)
            };

            // Combine the results
            var result = new
            {
                studentDetails = new
                {
                    studentDetails.CandidateID,
                    name = studentDetails.CandidateName,
                    rollNo = studentDetails.RollNumber,
                    fName = studentDetails.FName,
                    mName = studentDetails.MName,
                    institutionName = studentDetails.InstitutionName,
                    group = studentDetails.Group,
                    session = studentDetails.SessionName,
                    sem = studentDetails.SemesterName,
                    result = new
                    {
                        totalMaxMarks = columnWiseTotals.TotalMaxMarks,
                        TotalInternalMaxMarks = columnWiseTotals.TotalInternalMarks,
                        TotalInternalMarksObtained = columnWiseTotals.TotalInternalMarks,
                        TotalExternalMaxMarks = columnWiseTotals.MaxTheoryMarks,
                        TotalExternalMarksObtained = columnWiseTotals.TotalTheoryPaperMarks,
                        TotalPracticalMaxMarks = columnWiseTotals.MaxPracticalMarks,
                        TotalPracticalMarksObtained = columnWiseTotals.TotalPracticalMarks,
                        totalMarksObtained = columnWiseTotals.TotalCandidateMarks,
                        remarks = columnWiseTotals.TotalCandidateMarks >= (columnWiseTotals.TotalMaxMarks / 2) ? "उत्तीर्ण" : "असफल",
                        marksDetails = scoresWithRowTotal
                    }
                },
                papers

            };

            return Ok(result);
        }



        /*[HttpPost("GetCumulativeResult")]
        public IActionResult GetCumulativeResultPDF(inputforGCR igcr)
        {
            // Fetch the necessary data
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
                    p.PaperCode,
                    p.PaperType,
                    p.TheoryPaperMaxMarks,
                    p.PracticalMaxMarks,
                    p.InteralMaxMarks
                })
                .ToList();

            var marks = _context.StudentsMarksObtaineds
                .Where(m => students.Select(s => s.CandidateID).Contains(m.CandidateID) &&
                            papers.Select(p => p.PaperID).Contains(m.PaperID))
                .ToList();

            // Generate PDF using QuestPDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(10);
                    page.Size(PageSizes.A3.Landscape());

                    // Add header section
                    page.Header().Row(row =>
                    {
                        row.ConstantItem(80).AlignCenter().Image(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uttarakhand_Board_of_School_Education_Logo.jpg"));
                        row.RelativeItem().AlignCenter().Column(column =>
                        {
                            column.Item().Text("उत्तराखंड विद्यालयी शिक्षा परिषद् रामनगर (नैनीताल)").FontSize(16).Bold().AlignCenter();
                            column.Item().Text("परीक्षाफल द्वि-वर्षीय डिप्लोमा इन एलीमैंटरी एजुकेशन, प्रशिक्षण, प्रथम सेमेस्टर").FontSize(12).AlignCenter();
                        });
                    });

                    // Content Section
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(20); // Serial No
                            columns.ConstantColumn(40); // Roll No
                            columns.RelativeColumn(2);  // Name
                            columns.RelativeColumn(1.5f);  // Mother's Name
                            columns.RelativeColumn(1.5f);  // Father's Name
                            columns.RelativeColumn(2);  // DOB
                            columns.ConstantColumn(30); // Group

                            foreach (var paper in papers)
                            {
                                if (paper.PaperType == 2) // Practical only
                                {
                                    columns.RelativeColumn();
                                }
                                else
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                }
                            }

                            columns.ConstantColumn(50); // Total Marks
                            columns.ConstantColumn(50); // Result
                        });

                        // Header Section
                        table.Header(header =>
                        {
                            // First Row: Top-level headers
                            header.Cell().ColumnSpan(7).Border(1).Padding(5).AlignCenter().Text("छात्र विवरण").FontSize(8);

                            foreach (var paper in papers.Where(p => p.PaperType != 2)) // Theory/Internal papers
                            {
                                header.Cell().ColumnSpan(3).Border(1).Padding(5).AlignCenter().Text($"({paper.PaperCode})\n{paper.PaperName}").FontSize(8);
                            }

                            int practicalColumnSpan = papers.Count(p => p.PaperType == 2);
                            if (practicalColumnSpan > 0)
                            {
                                header.Cell().ColumnSpan((uint)practicalColumnSpan).Border(1).Padding(5).AlignCenter().Text("अभ्यासक्रम").FontSize(10);
                            }

                            header.Cell().RowSpan(2).Border(1).Padding(5).AlignCenter().Text("कुल योग").FontSize(10);
                            header.Cell().RowSpan(2).Border(1).Padding(5).AlignCenter().Text("परीक्षाफल").FontSize(10);

                            // Second Row: Column-specific headers
                            header.Cell().Border(1).Padding(2).AlignCenter().Text("क्र० सं०").FontSize(10);
                            header.Cell().Border(1).Padding(2).AlignCenter().Text("अनुक्रमांक").FontSize(10);
                            header.Cell().Border(1).Padding(5).AlignCenter().Text("प्रशिक्षु का नाम").FontSize(10);
                            header.Cell().Border(1).Padding(5).AlignCenter().Text("माता का नाम").FontSize(10);
                            header.Cell().Border(1).Padding(5).AlignCenter().Text("पिता का नाम").FontSize(10);
                            header.Cell().Border(1).Padding(5).AlignCenter().Text("जन्मतिथि").FontSize(10);
                            header.Cell().Border(1).Padding(5).AlignCenter().Text("वर्ग").FontSize(10);

                            foreach (var paper in papers)
                            {
                                if (paper.PaperType == 2) // Practical only
                                {
                                    header.Cell().Border(1).Padding(5).AlignCenter().Column(column =>
                                    {
                                        column.Item().Text($"{paper.PaperName}").FontSize(10);
                                        column.Item().Text($"({paper.PracticalMaxMarks})").FontSize(8);
                                    });
                                }
                                else
                                {
                                    header.Cell().Border(1).Padding(5).AlignCenter().Column(column =>
                                    {
                                        column.Item().Text("बाह्य").FontSize(8);
                                        column.Item().Text($"({paper.TheoryPaperMaxMarks})").FontSize(8);
                                    });

                                    header.Cell().Border(1).Padding(5).AlignCenter().Column(column =>
                                    {
                                        column.Item().Text("आंत०").FontSize(8);
                                        column.Item().Text($"({paper.InteralMaxMarks})").FontSize(8);
                                    });

                                    header.Cell().Border(1).Padding(5).AlignCenter().Column(column =>
                                    {
                                        column.Item().Text("योग").FontSize(8);
                                        column.Item().Text($"({paper.TheoryPaperMaxMarks + paper.InteralMaxMarks})").FontSize(8);
                                    });
                                }
                            }
                        });

                        // Data Rows
                        int serialNumber = 1;
                        foreach (var student in students)
                        {
                            int totalMarks = 0;
                            int totalTheoryMarks = 0;
                            int totalInteralMarks = 0;
                            int totalPracticalMarks = 0;

                            table.Cell().Border(1).Padding(2).AlignCenter().AlignMiddle().Text(serialNumber++.ToString()).FontSize(8);
                            table.Cell().Border(1).Padding(2).AlignCenter().AlignMiddle().Text(student.RollNumber).FontSize(8);
                            table.Cell().Border(1).Padding(5).AlignCenter().AlignMiddle().Text(student.CandidateName).FontSize(8);
                            table.Cell().Border(1).Padding(5).AlignCenter().AlignMiddle().Text(student.MName ?? "-").FontSize(8);
                            table.Cell().Border(1).Padding(5).AlignCenter().AlignMiddle().Text(student.FName ?? "-").FontSize(8);
                            table.Cell().Border(1).Padding(5).AlignCenter().AlignMiddle().Text(student.DOB?.ToString() ?? "-").FontSize(8);
                            table.Cell().Border(1).Padding(5).AlignCenter().AlignMiddle().Text(student.Group ?? "-").FontSize(8);

                            foreach (var paper in papers)
                            {
                                var studentMark = marks.FirstOrDefault(m => m.CandidateID == student.CandidateID && m.PaperID == paper.PaperID);

                                if (paper.PaperType == 2) // Practical only
                                {
                                    var practicalMarks = studentMark?.PracticalMarks ?? 0;
                                    table.Cell().Border(1).Padding(5).AlignCenter().Text(practicalMarks > 0 ? practicalMarks.ToString() : "-").FontSize(8);
                                    totalPracticalMarks += practicalMarks;
                                    totalMarks += practicalMarks;
                                }
                                else
                                {
                                    var theoryMarks = studentMark?.TheoryPaperMarks ?? 0;
                                    var internalMarks = studentMark?.InteralMarks ?? 0;
                                    var paperTotal = theoryMarks + internalMarks;
                                    totalTheoryMarks += theoryMarks;
                                    totalInteralMarks += internalMarks;

                                    table.Cell().Border(1).Padding(5).AlignCenter().Text(theoryMarks > 0 ? theoryMarks.ToString() : "-").FontSize(8);
                                    table.Cell().Border(1).Padding(5).AlignCenter().Text(internalMarks > 0 ? internalMarks.ToString() : "-").FontSize(8);
                                    table.Cell().Border(1).Padding(5).AlignCenter().Text(paperTotal > 0 ? paperTotal.ToString() : "-").FontSize(8);
                                    totalMarks += paperTotal;
                                }
                            }

                            table.Cell().Border(1).Padding(5).AlignCenter().Text(totalMarks.ToString()).FontSize(8); // Total Marks
                            table.Cell().Border(1).Padding(5).AlignCenter().Text(totalMarks >= 300 ? "PASS" : "FAIL").FontSize(8); // Result
                        }
                    });
                });
            });

            // Generate PDF
            var pdf = document.GeneratePdf();
            return File(pdf, "application/pdf", "CumulativeResult.pdf");
        }
*/
        [HttpPost("GetCumulativeResult")]
        public IActionResult GetCumulativeResultPDF(inputforGCR igcr)
        {
            try
            {
                // Fetch the necessary data and prepare DataTables
                var studentsTable = new DataTable("Students");
                studentsTable.Columns.Add("CandidateID", typeof(int));
                studentsTable.Columns.Add("CandidateName", typeof(string));
                studentsTable.Columns.Add("RollNumber", typeof(string));
                studentsTable.Columns.Add("Group", typeof(string));
                studentsTable.Columns.Add("FName", typeof(string));
                studentsTable.Columns.Add("MName", typeof(string));
                studentsTable.Columns.Add("InstitutionName", typeof(string));
                studentsTable.Columns.Add("DOB", typeof(DateTime));

                var papersTable = new DataTable("Papers");
                papersTable.Columns.Add("PaperID", typeof(int));
                papersTable.Columns.Add("PaperName", typeof(string));
                papersTable.Columns.Add("PaperCode", typeof(string));
                papersTable.Columns.Add("PaperType", typeof(int));
                papersTable.Columns.Add("TheoryPaperMaxMarks", typeof(int));
                papersTable.Columns.Add("PracticalMaxMarks", typeof(int));
                papersTable.Columns.Add("InteralMaxMarks", typeof(int));

                var marksTable = new DataTable("Marks");
                marksTable.Columns.Add("CandidateID", typeof(int));
                marksTable.Columns.Add("PaperID", typeof(int));
                marksTable.Columns.Add("TheoryPaperMarks", typeof(int));
                marksTable.Columns.Add("InteralMarks", typeof(int));
                marksTable.Columns.Add("PracticalMarks", typeof(int));

                // Populate DataTables
                var students = _context.Candidates
                    .Where(u => u.SemID == igcr.SemID && u.SesID == igcr.SesID)
                    .ToList();
                foreach (var student in students)
                {
                    studentsTable.Rows.Add(student.CandidateID, student.CandidateName, student.RollNumber, student.Group,
                                           student.FName, student.MName, student.InstitutionName, student.DOB);
                }

                var papers = _context.Papers
                    .Where(u => u.SemID == igcr.SemID)
                    .ToList();
                foreach (var paper in papers)
                {
                    papersTable.Rows.Add(paper.PaperID, paper.PaperName, paper.PaperCode, paper.PaperType,
                                         paper.TheoryPaperMaxMarks, paper.PracticalMaxMarks, paper.InteralMaxMarks);
                }

                var marks = _context.StudentsMarksObtaineds
                    .Where(m => students.Select(s => s.CandidateID).Contains(m.CandidateID) &&
                                papers.Select(p => p.PaperID).Contains(m.PaperID))
                    .ToList();
                foreach (var mark in marks)
                {
                    marksTable.Rows.Add(mark.CandidateID, mark.PaperID, mark.TheoryPaperMarks, mark.InteralMarks, mark.PracticalMarks);
                }

                // Load the Crystal Report
                ReportDocument reportDocument = new ReportDocument();
                string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "UTTRAKHAND - FIRST.rpt");
                reportDocument.Load(reportPath);

                // Set the data source
                DataSet reportData = new DataSet();
                reportData.Tables.Add(studentsTable);
                reportData.Tables.Add(papersTable);
                reportData.Tables.Add(marksTable);

                reportDocument.SetDataSource(reportData);

                // Export the report to PDF
                using (var stream = reportDocument.ExportToStream(ExportFormatType.PortableDocFormat))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream, "application/pdf", "CumulativeResult.pdf");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }





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
            var can = await _context.Candidates.FirstOrDefaultAsync(u => u.CandidateID == studentsMarksObtained.CandidateID);
            var lockstatus = await _context.LockStatuses.FirstOrDefaultAsync(u => u.SesID == can.SesID && u.SemID == can.SemID);
            if (lockstatus != null)
            {
                if (lockstatus?.IsLocked == true)
                {
                    return BadRequest("Database is locked");
                }

            }
            // Check if the record exists for the given CandidateID and PaperID
            var existingRecord = _context.StudentsMarksObtaineds
                .FirstOrDefault(s => s.CandidateID == studentsMarksObtained.CandidateID && s.PaperID == studentsMarksObtained.PaperID);
            var paper = await _context.Papers.FirstOrDefaultAsync(u => u.PaperID == studentsMarksObtained.PaperID);
            int userID = 0;
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

            if (userIdClaim != null)
            {
                Console.WriteLine($"Retrieved Claim Value: {userIdClaim.Value}");
                if (int.TryParse(userIdClaim.Value, out userID))
                {
                    Console.WriteLine($"User ID: {userID}");
                }
            }

            if (existingRecord != null)
            {
                // Update only the fields that have been provided (not null)
                if (studentsMarksObtained.TheoryPaperMarks.HasValue && paper.PaperType == 1)
                {
                    int oldMarks = existingRecord.TheoryPaperMarks.Value;
                    int newMarks = studentsMarksObtained.TheoryPaperMarks.Value;
                    existingRecord.TheoryPaperMarks = studentsMarksObtained.TheoryPaperMarks;
                    if (oldMarks != newMarks)
                    {
                        _loggerService.LogChangeInMarks($"Marks Updated for Paper:{paper.PaperName} for Candidate: {can.CandidateID}", "Theory", oldMarks, newMarks, userID);
                    }
                }

                if (studentsMarksObtained.InteralMarks.HasValue && paper.PaperType == 1)
                {
                    int oldMarks = existingRecord.InteralMarks.Value;
                    int newMarks = studentsMarksObtained.InteralMarks.Value;
                    existingRecord.InteralMarks = studentsMarksObtained.InteralMarks;
                    if(oldMarks!=newMarks)
                        _loggerService.LogChangeInMarks($"Marks Updated for Paper:{paper.PaperName} for Candidate: {can.CandidateID}", "Internal", oldMarks, newMarks, userID);
                }

                if (studentsMarksObtained.PracticalMarks.HasValue && paper.PaperType != 1)
                {
                    int oldMarks = existingRecord.PracticalMarks.Value;
                    int newMarks = studentsMarksObtained.PracticalMarks.Value;
                    existingRecord.PracticalMarks = studentsMarksObtained.PracticalMarks;
                    if(oldMarks!=newMarks)
                        _loggerService.LogChangeInMarks($"Marks Updated for Paper:{paper.PaperName} for Candidate: {can.CandidateID}", "Practical", oldMarks, newMarks, userID);
                }

                if (studentsMarksObtained.IsAbsent.HasValue)
                {
                    bool oldMarks = existingRecord.IsAbsent.Value;
                    bool newMarks = studentsMarksObtained.IsAbsent.Value;
                    existingRecord.IsAbsent = studentsMarksObtained.IsAbsent;
                    if (oldMarks != newMarks)
                        _loggerService.LogChangeInAbsent($"Marks Updated for Paper:{paper.PaperName} for Candidate: {can.CandidateID}", "Practical", oldMarks, newMarks, userID);
                }

                // Calculate TotalMarks
                existingRecord.TotalMarks = (existingRecord.TheoryPaperMarks ?? 0) +
                                            (existingRecord.InteralMarks ?? 0) +
                                            (existingRecord.PracticalMarks ?? 0);
                if ((existingRecord.TheoryPaperMarks >= (paper.TheoryPaperMaxMarks / 2)) && (existingRecord.InteralMarks >= (paper.InteralMaxMarks / 2)) && (existingRecord.PracticalMarks >= (paper.PracticalMaxMarks / 2)) && (existingRecord.TotalMarks >= (paper.TotalMaxMarks / 2)))
                {
                    existingRecord.Status = "Pass";
                }
                else
                {
                    existingRecord.Status = "Fail";
                }

                // Save changes to the database
                _context.Entry(existingRecord).State = EntityState.Modified;
            }
            else
            {
                // Calculate TotalMarks for the new record
                studentsMarksObtained.TotalMarks = (studentsMarksObtained.TheoryPaperMarks ?? 0) +
                                                   (studentsMarksObtained.InteralMarks ?? 0) +
                                                   (studentsMarksObtained.PracticalMarks ?? 0);

                if ((studentsMarksObtained.TheoryPaperMarks >= (paper.TheoryPaperMaxMarks / 2)) && (studentsMarksObtained.InteralMarks >= (paper.InteralMaxMarks / 2)) && (studentsMarksObtained.PracticalMarks >= (paper.PracticalMaxMarks / 2)) && (studentsMarksObtained.TotalMarks >= (paper.TotalMaxMarks / 2)))
                {
                    studentsMarksObtained.Status = "Pass";
                }
                else
                {
                    studentsMarksObtained.Status = "Fail";
                }

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


        /*[HttpPost("Audit")]
        public async Task<ActionResult<object>> AuditSem(inputforGCR info)
        {
            List<object> remarks = new List<object>();
            var candidates = await _context.Candidates.Where(u => u.SemID == info.SemID && u.SesID == info.SesID).OrderBy(u => u.CandidateID).ToListAsync();
            if (!candidates.Any())
            {
                return NotFound();
            }
            foreach (var can in candidates)
            {

                var optedPaperCodes = can.PapersOpted.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var markslist = await _context.StudentsMarksObtaineds.Where(u => u.CandidateID == can.CandidateID).ToListAsync();
                var papers = await _context.Papers
                    .Where(u => u.SemID == can.SemID &&
                                (u.PaperType != 1 || (u.PaperType == 1 && optedPaperCodes.Contains(u.PaperCode.ToString())))).ToListAsync();
                int lenghtofmarklist = markslist.Count;
                int lengthofpaperlist = papers.Count;
                Console.WriteLine($"Candidate: {can.CandidateID}");
                Console.WriteLine($"MarkList Length: " + lenghtofmarklist);
                Console.WriteLine($"PaperList Lenght: " + lengthofpaperlist);
                if (lenghtofmarklist == lengthofpaperlist)
                {
                    remarks.Add(new
                    {
                        RollNumber = can.RollNumber,
                        CanGenerateCertificate = true
                    });
                }
                else
                {
                    remarks.Add(new
                    {
                        RollNumber = can.RollNumber,
                        CanGenerateCertificate = false
                    });
                }
            }
            return Ok(remarks);
        }*/

        [HttpPost("Audit")]
        public async Task<ActionResult<object>> AuditSem(inputforGCR info)
        {
            List<object> remarks = new List<object>();
            var candidates = await _context.Candidates
                .Where(u => u.SemID == info.SemID && u.SesID == info.SesID)
                .OrderBy(u => u.CandidateID)
                .ToListAsync();

            if (!candidates.Any())
            {
                return NotFound();
            }

            foreach (var can in candidates)
            {
                var optedPaperCodes = can.PapersOpted.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var markslist = await _context.StudentsMarksObtaineds
                    .Where(u => u.CandidateID == can.CandidateID)
                    .ToListAsync();

                var papers = await _context.Papers
                    .Where(u => u.SemID == can.SemID &&
                                (u.PaperType != 1 || (u.PaperType == 1 && optedPaperCodes.Contains(u.PaperCode.ToString()))))
                    .ToListAsync();

                int lengthOfMarkList = markslist.Count;
                int lengthOfPaperList = papers.Count;

                Console.WriteLine($"Candidate: {can.CandidateID}");
                Console.WriteLine($"MarkList Length: {lengthOfMarkList}");
                Console.WriteLine($"PaperList Length: {lengthOfPaperList}");

                if (lengthOfMarkList == lengthOfPaperList)
                {
                    remarks.Add(new
                    {
                        RollNumber = can.RollNumber,
                        CanGenerateCertificate = true
                    });
                }
                else
                {
                    // Get the list of paper codes for which marks are missing
                    var papersWithMissingMarks = papers
                        .Where(p => !markslist.Any(m => m.PaperID == p.PaperID))
                        .Select(p => p.PaperName)
                        .ToList();

                    remarks.Add(new
                    {
                        RollNumber = can.RollNumber,
                        CanGenerateCertificate = false,
                        MissingPapers = papersWithMissingMarks
                    });
                }
            }

            return Ok(remarks);
        }


        [HttpPost("AuditforSingle")]
        public async Task<ActionResult<bool>> AuditRollNumberforCertificate(studentinfo info)
        {
            var can = await _context.Candidates.FirstOrDefaultAsync(u => u.RollNumber == info.RollNumber && u.SemID == info.SemesterId && u.SesID == info.SessionId);
            if (can == null)
            {
                return NotFound(false);
            }
            var optedPaperCodes = can.PapersOpted.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var markslist = await _context.StudentsMarksObtaineds.Where(u => u.CandidateID == can.CandidateID).ToListAsync();
            var papers = await _context.Papers
                    .Where(u => u.SemID == can.SemID &&
                                (u.PaperType != 1 || (u.PaperType == 1 && optedPaperCodes.Contains(u.PaperCode.ToString())))).ToListAsync();
            int lenghtofmarklist = markslist.Count;
            int lengthofpaperlist = papers.Count;
            if (lenghtofmarklist == lengthofpaperlist)
            {
                return Ok(true);
            }
            else
            {
                return Ok(false);
            }
        }

        private bool StudentsMarksObtainedExists(int id)
        {
            return _context.StudentsMarksObtaineds.Any(e => e.SmoID == id);
        }
    }

    public class inputforGCR
    {
        public int SemID { get; set; }

        public int SesID { get; set; }
    }

    public class studentinfo
    {
        public string RollNumber { get; set; }

        public int SessionId { get; set; }

        public int SemesterId { get; set; }
    }
}
