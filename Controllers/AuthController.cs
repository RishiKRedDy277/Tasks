using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LoginJWTApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
namespace LoginJWTApi.Controllers{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController :ControllerBase{
        public static User user = new User();
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration){
            _configuration=configuration;
        }
        [HttpPost("register")]
        public ActionResult<User> Register(UserDto request){
            string passwordHash=BCrypt.Net.BCrypt.HashPassword(request.password);
            user.Username=request.Username;
            user.PasswordHash=passwordHash;
            return Ok(user);
        }
        [HttpPost("login")]
        public ActionResult<User> login(UserDto request){
            if(user.Username!=request.Username){
                return BadRequest("User not found.");
            }
            if(!BCrypt.Net.BCrypt.Verify(request.password,user.PasswordHash)){
                return BadRequest("Wrong Password");
            }
            string token=CreateToken(user);
            return Ok(token);
        }
        private string CreateToken(User user){
            List<Claim> claims =new List<Claim>{
                new Claim(ClaimTypes.Name,user.Username)
            };
            var key= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));
            var creds=new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);
            var token =new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;

        }

    } 
}