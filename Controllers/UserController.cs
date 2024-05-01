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
            var user = users.FirstOrDefault(u => u.Username.ToLower() == login.Username.ToLower() && 
                                            BCrypt.Net.BCrypt.Verify(login.Password, u.Password));
            if (user == null)
            {
                return NotFound("User doesn't exist");
            }

            string token = CreateToken(user);

            return Ok(token);
        }

        private string CreateToken(User user){
            
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, _configuration["Jwt:Audience"]),
                new Claim(JwtRegisteredClaimNames.Iss, _configuration["Jwt:Issuer"])
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                audience: _configuration["Audience"],
                issuer: _configuration["Issuer"],
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
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

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(register.Password);

            User registeringUser = new User
            {
                Name = register.Name.ToLower(),
                Email = register.Email,
                Username = register.Username,
                Password = hashedPassword, 
            };

            await _userRepository.AddAsync(registeringUser);
            await _userRepository.Save();

            return Ok(registeringUser);
        }

        [Authorize]
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

            return Ok();
        }

        [Authorize]
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

            return Ok();
        }
    }
}
