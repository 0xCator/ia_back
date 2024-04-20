using ia_back.Data.Custom_Repositories;
using ia_back.Data;
using ia_back.Models;
using Microsoft.AspNetCore.Mvc;
using ia_back.DTOs.ProjectDTO;
using ia_back.DTOs.ReqestDTO;


namespace ia_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : Controller
    {
        private readonly IDataRepository<Project> _projectRepository;
        private readonly IDataRepository<User> _userRepository;
        private readonly ProjectRequestContorller _projectRequestContorller;

        public ProjectController(IDataRepository<Project> projectRepository, IDataRepository<User> userRepository, ProjectRequestContorller projectRequestContorller)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _projectRequestContorller = projectRequestContorller;
        }

        [HttpPut]
        public async Task<IActionResult> CreateProject(ProjectEntryDTO projectInfo)
        {
            if (projectInfo == null)
            {
                return BadRequest();
            }

            var teamLeader = await _userRepository.GetByIdAsync(projectInfo.TeamLeaderID);
            if (teamLeader == null)
            {
                return NotFound("Team leader doesn't exist");
            }

            Project project = new Project
            {
                Name = projectInfo.Name,
                TeamLeaderId = projectInfo.TeamLeaderID,
                RequestedDevelopers = new List<User>(),
            };

            foreach (var developerName in projectInfo.RequestedDevelopers)
            {
                if (developerName == null || developerName.Length == 0 || developerName == teamLeader.Name)
                {
                    return NotFound("Invalid names");
                }
                var userRepository = _userRepository as UserRepository;
                if (userRepository == null)
                {
                    return NotFound("User repository is null");
                }
                var developer = await userRepository.GetByUsernameAsync(developerName);
                if (developer == null)
                {
                    return NotFound("Developer doesn't exist");
                }
                if(await _projectRequestContorller.CreateProjectRequest(new ReqestEntryDTO
                {
                    ProjectId = project.Id,
                    UserId = developer.Id
                }) == null)
                {
                    return NotFound("Request failed");
                }
            }

            await _projectRepository.AddAsync(project);
            await _projectRepository.Save();

            return Ok();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            await _projectRepository.DeleteAsync(project);
            await _projectRepository.Save();

            return Ok();
        }

        [HttpPatch("id/newName")]
        public async Task<IActionResult> UpdateProjectName(int id, string newName)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            project.Name = newName;
            await _projectRepository.UpdateAsync(project);
            await _projectRepository.Save();

            return Ok();
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _projectRepository.GetAllAsync();
            if (projects == null)
            {
                return NotFound();
            }

            return Ok(projects);
        }

        [HttpPost("projId/sendRequest/developerName")]
        public async Task<IActionResult> AssigneDeveloperToProject(int id, string developerName)
        {
            var userRepository = _userRepository as UserRepository;
            if (userRepository == null)
            {
                return NotFound("User repository is null");
            }
            var developer = await userRepository.GetByUsernameAsync(developerName);
            if (developer == null)
            {
                return NotFound("Developer doesn't exist");
            }

            if(await _projectRequestContorller.CreateProjectRequest(new ReqestEntryDTO
            {
                ProjectId = id,
                UserId = developer.Id 
            }) == null){

                return NotFound("Request failed"); 
            }

            return Ok();
            
        }

        [HttpDelete("id/developerName")]
        public async Task<IActionResult> RemoveDeveloperFromProject(int id, string developerName)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            var userRepository = _userRepository as UserRepository; 
            if (userRepository == null)
            {
                return NotFound("User repository is null");
            }
            var developer = await userRepository.GetByUsernameAsync(developerName);
            if (developer == null)
            {
                return NotFound("Developer doesn't exist");
            }
            
            if(project.RequestedDevelopers.Contains(developer))
            {
                project.RequestedDevelopers.Remove(developer);
            }
            else if (project.AssignedDevelopers.Contains(developer))
            {
                project.AssignedDevelopers.Remove(developer);
            }
            else
            {
                return NotFound("Developer is not in the project");
            }
            
            await _projectRepository.UpdateAsync(project);
            await _projectRepository.Save();

            return Ok();
        }


    }
}
