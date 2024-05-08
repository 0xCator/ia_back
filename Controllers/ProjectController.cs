using ia_back.Data.Custom_Repositories;
using ia_back.Data;
using ia_back.Models;
using Microsoft.AspNetCore.Mvc;
using ia_back.DTOs.ProjectDTO;
using Microsoft.AspNetCore.Authorization;
using ia_back.WebSocket;
using System.Linq.Expressions;
using AutoMapper;

namespace ia_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : Controller
    {
        private readonly IDataRepository<Project> _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly SocketManager _socketManager = new SocketManager();

        public ProjectController(IMapper mapper, IDataRepository<Project> projectRepository, IUserRepository userRepository)
        {
            _mapper = mapper;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
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


        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetProjectsByUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                CreatedProjects = _mapper.Map<ICollection<Project>, ICollection<ProjectCardDTO>>(user.CreatedProjects),
                AssignedProjects = _mapper.Map<ICollection<Project>, ICollection<ProjectCardDTO>>(user.AssignedProjects),
                ProjectRequests = _mapper.Map<ICollection<Project>, ICollection<ProjectCardDTO>>(user.ProjectRequests)
            });
        }


        [Authorize(Roles = "TeamLeader")]
        [HttpPost]
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

            return Ok(new { projectID = project.Id });
        }


        [Authorize(Roles = "TeamLeader")]
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

            return Ok("Project deleted successfully");
        }


        [Authorize(Roles = "TeamLeader")]
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


        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            Expression<Func<Project, bool>> criteria = p => p.Id == id;
            var project = await _projectRepository.GetByIdIncludeAsync(criteria,
                                                                       p => p.TeamLeader,
                                                                       p => p.RequestedDevelopers,
                                                                       p => p.AssignedDevelopers,
                                                                       p => p.Tasks);
            if (project == null)
            {
                return NotFound();
            }

            ProjectInfoDTO projectInfo = _mapper.Map<Project, ProjectInfoDTO>(project);

            return Ok(projectInfo);
        }


        [Authorize(Roles = "TeamLeader")]
        [HttpPost("{id}/developer/{developerUserName}")]
        public async Task<IActionResult> AssignDeveloperToProject(int id, string developerUserName)
        {
            Expression<Func<Project, bool>> criteria = p => p.Id == id;
            var project = await _projectRepository.GetByIdIncludeAsync(criteria, 
                                                                       p => p.TeamLeader, 
                                                                       p=>p.RequestedDevelopers, 
                                                                       p=>p.AssignedDevelopers);
            if (project == null)
            {
                return NotFound();
            }
            var developer = await _userRepository.GetByUsernameAsync(developerUserName);
            if (developer == null)
            {
                return NotFound("Developer doesn't exist");
            }
            if (developer.Role != Role.Developer)
            {
                return BadRequest("Developer is not a developer");
            }
            if (project.RequestedDevelopers.Contains(developer) || project.AssignedDevelopers.Contains(developer))
            {
                return BadRequest("Developer is already in the project");
            }
            if (project.TeamLeaderId == developer.Id)
            {
                return BadRequest("Developer is the team leader");
            }

            project.RequestedDevelopers.Add(developer);

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.Save();
            await _socketManager.ProjectHasUpdate(developer.Id);
            return Ok();
            
        }


        [Authorize(Roles = "TeamLeader")]
        [HttpDelete("{id}/developer/{developerUserName}")]
        public async Task<IActionResult> RemoveDeveloperFromProject(int id, string developerUserName)
        {
            Expression<Func<Project, bool>> criteria = p => p.Id == id;
            var project = await _projectRepository.GetByIdIncludeAsync(criteria,
                                                                       p => p.TeamLeader,
                                                                       p => p.RequestedDevelopers,
                                                                       p => p.AssignedDevelopers,
                                                                       p => p.Tasks);
            if (project == null)
            {
                return NotFound();
            }
            var developer = await _userRepository.GetByUsernameAsync(developerUserName);
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

                foreach (var task in project.Tasks)
                {
                    if (task.AssignedDevId == developer.Id)
                    {
                        project.Tasks.Remove(task);
                    }
                }
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
