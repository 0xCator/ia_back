using ia_back.Data;
using ia_back.Models;
using ia_back.DTOs.Login;
using ia_back.DTOs.RequestDTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using ia_back.Data.Custom_Repositories;
using Azure.Core;

namespace ia_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IDataRepository<User> _userRepository;
        private readonly IDataRepository<Project> _projectRepository;

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

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(login.Password);
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username.ToLower() == login.Username.ToLower() && 
                                            u.Password == hashedPassword);
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

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(register.Password);

            User registeringUser = new User
            {
                Name = register.Name,
                Email = register.Email,
                Username = register.Username,
                Password = hashedPassword, 
            };

            await _userRepository.AddAsync(registeringUser);
            await _userRepository.Save();

            return Ok(registeringUser);
        }

        [HttpPost("acceptRequest")]
        public async Task<IActionResult> AcceptRequest(RequestDTO request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var project = user.ProjectRequests.FirstOrDefault(p => p.Id == request.ProjectId);
            user.AssignedProjects.Add(project);
            user.ProjectRequests.Remove(project);

            await _userRepository.UpdateAsync(user);
            await _userRepository.Save();

            return Ok();
        }

        [HttpPost("rejectRequest")]
        public async Task<IActionResult> RejectRequest(RequestDTO request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var project = user.ProjectRequests.FirstOrDefault(p => p.Id == request.ProjectId);

            user.ProjectRequests.Remove(project);

            await _userRepository.UpdateAsync(user);
            await _userRepository.Save();

            return Ok();
        }
    }
}
