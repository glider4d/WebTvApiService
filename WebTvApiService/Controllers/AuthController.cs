using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebTvApiService.Models;


namespace WebTvApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
//        public static User user = new();
        public static List<User> users = new()
        {
            new User { Username = "root", PasswordHash = "$2a$11$/0l0JjAZnx.UkznVUixMXO9LumtHgKUpSnryC4Lg1bhzykwPJSf0q" },
            new User { Username = "user", PasswordHash = "$2a$11$/0l0JjAZnx.UkznVUixMXO9LumtHgKUpSnryC4Lg1bhzykwPJSf0q" }
        };
        
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost(nameof(Register))]
        public ActionResult<User> Register(UserDto request)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            User? newUser = new ();
            newUser.Username = request.Username;
            newUser.PasswordHash = passwordHash;

            var res = users.Find(o => o.Username == newUser.Username);
            if (res is not null) 
                return BadRequest("User exists");

            users.Add(newUser);
            return Ok(newUser);
        }

        [HttpPost("login")]
        public ActionResult<AuthDataResponse> Login(UserDto request)
        {
            Task.Delay(5000).Wait();

            User? findUser = users.Find(o => o.Username == request.Username);
            //if (user.Username != request.Username)
            if (findUser is null)
            {
                return BadRequest("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, findUser.PasswordHash))
            {
                return BadRequest("Wrong password");
            }

            string token = CreateToken(findUser);
            AuthDataResponse data = new() { username = findUser.Username, access_token = token };
            return Ok(data);
        }

        [HttpGet(nameof(GetUsers)), Authorize]
        public ActionResult<List<User>> GetUsers()
        {
            return Ok(users);
        }

        [HttpGet(nameof(CheckAuth)), Authorize]
        public ActionResult<bool> CheckAuth()
        {
            Task.Delay(2000).Wait();
            return Ok(true);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Username)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);


            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
