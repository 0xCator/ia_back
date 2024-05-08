using ia_back.Data;
using ia_back.Models;
using ia_back.DTOs.Login;
using ia_back.DTOs.RequestDTO;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using ia_back.Data.Custom_Repositories;

namespace ia_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;


        public UserController(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO login, [FromServices] IConfiguration configuration)
        {
            if (login == null)
            {
                return BadRequest();
            }

            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username.ToLower() == login.Username.ToLower());
            if (user == null)
            {
                return NotFound("User doesn't exist");
            }
            if (!VerifyPasswordHash(login.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Invalid password");
            }

            string token = CreateToken(user);

            return Ok(new { token = token });
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                        return false;
                }
            }
            return true;
        }

        private string CreateToken(User user){
            
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, _configuration["Jwt:Audience"]),
                new Claim(JwtRegisteredClaimNames.Iss, _configuration["Jwt:Issuer"])
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.Now,
                Expires = DateTime.Now.AddDays(30),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO register)
        {
            if (register == null)
            {
                return BadRequest();
            }

            var userExists = await _userRepository.UserExists(register.Username, register.Email);
            if (userExists)
            {
                return BadRequest("User already exists");
            }

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(register.Password, out passwordHash, out passwordSalt);

            User registeringUser = new User
            {
                Name = register.Name.ToLower(),
                Email = register.Email,
                Username = register.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = register.Role
            };

            await _userRepository.AddAsync(registeringUser);
            await _userRepository.Save();

            return Ok("User registered");
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }


        [Authorize(Roles = "Developer")]
        [HttpPost("acceptRequest")]
        public async Task<IActionResult> AcceptRequest(RequestDTO request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var project = user.ProjectRequests.FirstOrDefault(p => p.Id == request.ProjectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            user.AssignedProjects.Add(project);
            user.ProjectRequests.Remove(project);

            await _userRepository.UpdateAsync(user);
            await _userRepository.Save();

            return Ok("Project accepted");
        }


        [Authorize(Roles = "Developer")]
        [HttpPost("rejectRequest")]
        public async Task<IActionResult> RejectRequest(RequestDTO request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var project = user.ProjectRequests.FirstOrDefault(p => p.Id == request.ProjectId);

            if (project == null)
            {
                return NotFound("Project not found");
            }

            user.ProjectRequests.Remove(project);

            await _userRepository.UpdateAsync(user);
            await _userRepository.Save();

            return Ok("Project rejected");
        }
    }
}
