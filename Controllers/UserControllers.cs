

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using ShelfSpace.Models;


namespace ShelfSpace;

[Authorize]
[ApiController]
[Route("media")]

public class UserController : ControllerBase
    {   
        private readonly ShelfSpaceContext _dbContext;
        private readonly JwtSettings jwtSettings;

        public UserController(ShelfSpaceContext shelfSpaceContex, IOptions<JwtSettings> options)
        {
            this._dbContext=shelfSpaceContex;
            this.jwtSettings=options.Value;
        }

        [HttpPost("Authenticate")]   
        public async Task<IActionResult> Authenticate([FromBody] UserCred userCred)
        {
            var user = await this._dbContext.User.FirstOrDefaultAsync(item=>item.Email==userCred.Email);

        if(user==null)
            return Unauthorized();
            // Generate Token 
        if(userCred.Password == null)
            return Unauthorized();

            bool isPasswordValid = VerifyPassword(userCred.Password, user.PasswordHash);
        var tokenhandler = new JwtSecurityTokenHandler();
        var tokenkey = Encoding.UTF8.GetBytes(this.jwtSettings.securitykey);
        var tokendesc = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new Claim[] { new Claim(ClaimTypes.Name, user.Id)}
    ),
            Expires=DateTime.Now.AddMinutes(30), 
            SigningCredentials=new SigningCredentials(new SymmetricSecurityKey(tokenkey),SecurityAlgorithms.HmacSha256)
        }; 
        var token=tokenhandler.CreateToken(tokendesc);
        string finaltoken=tokenhandler.WriteToken(token);

        return Ok(new
    {
        Token = finaltoken,
        ExpiresIn = tokendesc.Expires,
        User = new
        {
            user.Id,
            user.Email,
            user.Name,
            user.LastName,
            user.Phone,
            user.Address,
            user.BirthDate,
            user.PasswordHash
        }
    });
    }

    public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

    public bool VerifyPassword(string inputPassword, string StoredHash){
        return BCrypt.Net.BCrypt.Verify(inputPassword, StoredHash);
    }

    }