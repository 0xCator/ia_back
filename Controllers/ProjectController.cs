using ia_back.Data.Custom_Repositories;
using ia_back.Data;
using ia_back.Models;
using Microsoft.AspNetCore.Mvc;
using ia_back.DTOs.ProjectDTO;
using Microsoft.AspNetCore.Authorization;
using ia_back.WebSocket;

namespace ia_back.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : Controller
    {
        private readonly IDataRepository<Project> _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly SocketManager _socketManager = new SocketManager();

        public ProjectController(IDataRepository<Project> projectRepository, IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
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
                var developer = await _userRepository.GetByUsernameAsync(developerName);
                if (developer == null)
                {
                    return NotFound("Developer doesn't exist");
                }
                project.RequestedDevelopers.Add(developer);
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

        [HttpPatch("{id}/{newName}")]
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _projectRepository.GetByIdIncludeAsync(id,
                                                                       p => p.TeamLeader,
                                                                       p => p.RequestedDevelopers,
                                                                       p => p.AssignedDevelopers);
            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _projectRepository.GetAllIncludeAsync(p => p.TeamLeader,
                                                                       p => p.RequestedDevelopers,
                                                                       p => p.AssignedDevelopers);
            if (projects == null)
            {
                return NotFound();
            }

            return Ok(projects);
        }

        [HttpPost("{id}/sendRequest/{developerName}")]
        public async Task<IActionResult> AssignDeveloperToProject(int id, string developerName)
        {
            var project = await _projectRepository.GetByIdIncludeAsync(id, 
                                                                       p => p.TeamLeader, 
                                                                       p=>p.RequestedDevelopers, 
                                                                       p=>p.AssignedDevelopers);
            if (project == null)
            {
                return NotFound();
            }
            var developer = await _userRepository.GetByUsernameAsync(developerName);
            if (developer == null)
            {
                return NotFound("Developer doesn't exist");
            }
            if (project.RequestedDevelopers.Contains(developer) || project.AssignedDevelopers.Contains(developer))
            {
                return BadRequest("Developer is already in the project");
            }

            project.RequestedDevelopers.Add(developer);

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.Save();
            await _socketManager.ProjectHasUpdate(developer.Id);
            return Ok();
            
        }

        [HttpDelete("{id}/{developerName}")]
        public async Task<IActionResult> RemoveDeveloperFromProject(int id, string developerName)
        {
            var project = await _projectRepository.GetByIdIncludeAsync(id,
                                                                       p => p.TeamLeader,
                                                                       p => p.RequestedDevelopers,
                                                                       p => p.AssignedDevelopers);
            if (project == null)
            {
                return NotFound();
            }
            var developer = await _userRepository.GetByUsernameAsync(developerName);
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
