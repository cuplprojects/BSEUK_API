using BSEUK.Data;
using BSEUK.Models;
using BSEUK.Models.NonDBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BSEUK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly AppDbContext _appDbContext;

        public LoginController(IConfiguration configuration, AppDbContext appDbContext)
        {
            _configuration = configuration;
            _appDbContext = appDbContext;
        }

        private string GenerateToken(UserAuth user)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserID.ToString())
            };

            var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials
        );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost]
        public IActionResult Login([FromBody] MLoginRequest model)
        {
            var UserAuth = _appDbContext.UserAuths.FirstOrDefault(u => u.UserName == model.UserName);
            if (UserAuth == null)
            {
                return NotFound();
            }

            if(UserAuth.Password != model.Password)
            {
                return Unauthorized("Invalid Password");
            }
            var token = GenerateToken(UserAuth);

            return Ok(new {token = token, UserAuth.UserID});
        }

        [HttpPut("Changepassword/{id}")]
        public IActionResult ChangePassword(int id, MChangePassword cred)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userauth = _appDbContext.UserAuths.FirstOrDefault(i => i.UserID == id);

            if (userauth == null)
            {
                return NotFound("User Authentication Data Not Found");
            }

            if (userauth.Password != cred.OldPassword)
            {
                return BadRequest("Existing Password Invalid");
            }

            userauth.Password = cred.NewPassword;
            _appDbContext.SaveChanges();

            return Ok();
        }

    }
}
