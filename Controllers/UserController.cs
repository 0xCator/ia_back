using ia_back.Data;
using ia_back.Models;
using ia_back.DTOs.Login;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ia_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IDataRepository<User> _userRepository;

        public UserController(IDataRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            if (login == null)
            {
                return BadRequest();
            }

            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username.ToLower() == login.Username.ToLower() && 
                                            u.Password == login.Password);
            if (user == null)
            {
                return NotFound("User doesn't exist");
            }
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO register)
        {
            if (register == null)
            {
                return BadRequest();
            }

            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username.ToLower() == register.Username.ToLower() ||
                                            u.Email.ToLower() == register.Email.ToLower());
            if (user != null)
            {
                return BadRequest("User already exists");
            }

            User registeringUser = new User
            {
                Name = register.Name,
                Email = register.Email,
                Username = register.Username,
                Password = register.Password
            };

            await _userRepository.AddAsync(registeringUser);
            await _userRepository.Save();

            return Ok(registeringUser);
        }
    }
}
