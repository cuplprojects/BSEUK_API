using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSEUK.Data;
using BSEUK.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;



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

        [HttpGet("GetStudentPaperMarks/{studentId}/{paperID}")]
        public async Task<ActionResult<StudentsMarksObtained>> GetStudentPaperMarks(int studentId, int paperID)
        {
            var marks = _context.StudentsMarksObtaineds.FirstOrDefault(u => u.CandidateID == studentId && u.PaperID == paperID);
            if (marks == null)
            {
                return NotFound();
            }
            return marks;
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
                        paper.TheoryPaperMaxMarks,
                        paper.PracticalMaxMarks,
                        paper.InteralMaxMarks
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
        s.PaperName,
        s.PaperType,
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
            };

            return Ok(result);
        }



        [HttpPost("GetCumulativeResult")]
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

                if (studentsMarksObtained.PracticalMarks.HasValue)
                {
                    existingRecord.PracticalMarks = studentsMarksObtained.PracticalMarks;
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
