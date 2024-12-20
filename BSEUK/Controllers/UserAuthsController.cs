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
    public class UserAuthsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserAuthsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserAuths
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserAuth>>> GetUserAuths()
        {
            return await _context.UserAuths.ToListAsync();
        }

        // GET: api/UserAuths/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserAuth>> GetUserAuth(int id)
        {
            var userAuth = await _context.UserAuths.FindAsync(id);

            if (userAuth == null)
            {
                return NotFound();
            }

            return userAuth;
        }

        // PUT: api/UserAuths/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAuth(int id, UserAuth userAuth)
        {
            if (id != userAuth.UaID)
            {
                return BadRequest();
            }

            _context.Entry(userAuth).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserAuthExists(id))
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

        // POST: api/UserAuths
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserAuth>> PostUserAuth(UserAuth userAuth)
        {
            _context.UserAuths.Add(userAuth);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserAuth", new { id = userAuth.UaID }, userAuth);
        }

        // DELETE: api/UserAuths/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAuth(int id)
        {
            var userAuth = await _context.UserAuths.FindAsync(id);
            if (userAuth == null)
            {
                return NotFound();
            }

            _context.UserAuths.Remove(userAuth);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserAuthExists(int id)
        {
            return _context.UserAuths.Any(e => e.UaID == id);
        }
    }
}
