using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSEUK.Data;
using BSEUK.Models;
using Microsoft.AspNetCore.Authorization;

namespace BSEUK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LockStatusController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LockStatusController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LockStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LockStatus>>> GetLockStatuses()
        {
            return await _context.LockStatuses.ToListAsync();
        }

        // GET: api/LockStatus/5
        [HttpGet("getbysessionandsemester")]
        public async Task<ActionResult<LockStatus>> GetLockStatus(inputinfo info)
        {
            var lockStatus = await _context.LockStatuses.FirstOrDefaultAsync(u=>u.SemID == info.SemID && u.SesID==info.SesID);

            if (lockStatus == null)
            {
                return NotFound();
            }

            return lockStatus;
        }

        // PUT: api/LockStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLockStatus(int id, LockStatus lockStatus)
        {
            if (id != lockStatus.Id)
            {
                return BadRequest();
            }

            _context.Entry(lockStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LockStatusExists(id))
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

        // POST: api/LockStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LockStatus>> PostLockStatus(LockStatus lockStatus)
        {
            _context.LockStatuses.Add(lockStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLockStatus", new { id = lockStatus.Id }, lockStatus);
        }

        // DELETE: api/LockStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLockStatus(int id)
        {
            var lockStatus = await _context.LockStatuses.FindAsync(id);
            if (lockStatus == null)
            {
                return NotFound();
            }

            _context.LockStatuses.Remove(lockStatus);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LockStatusExists(int id)
        {
            return _context.LockStatuses.Any(e => e.Id == id);
        }
    }
    public class inputinfo
    {
        public int SesID { get; set; }
        public int SemID { get; set; }
    }
}
