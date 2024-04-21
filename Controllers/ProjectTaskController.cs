using Microsoft.AspNetCore.Mvc;
using ia_back.Data.Custom_Repositories;
using ia_back.Data;
using ia_back.DTOs.TaskDTO;
using ia_back.Models;
using Microsoft.AspNetCore.Authorization;

namespace ia_back.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectTaskController : Controller
    {
        private readonly IDataRepository<ProjectTask> _projectTaskRepository;
        private readonly IDataRepository<Project> _projectRepository;

        public ProjectTaskController(IDataRepository<ProjectTask> projectTaskRepository,
                                    IDataRepository<Project> projectRepository)
        {
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
        }

        [HttpPut]
        public async Task<IActionResult> CreateProjectTask(TaskEntryDTO projectTaskInfo)
        {
            if (projectTaskInfo == null)
            {
                return BadRequest();
            }

            var project = await _projectRepository.GetByIdAsync(projectTaskInfo.ProjectID);
            if (project == null)
            {
                return NotFound("Project doesn't exist");
            }

            var assignedDev = project.AssignedDevelopers.FirstOrDefault(d => d.Id == projectTaskInfo.AssignedDevId);
            if (assignedDev == null)
            {
                return NotFound("Developer doesn't exist");
            }

            ProjectTask projectTask = new ProjectTask
            {
                Name = projectTaskInfo.Name,
                Description = projectTaskInfo.Description,
                ProjectId = projectTaskInfo.ProjectID,
                AssignedDevId = projectTaskInfo.AssignedDevId,
                Status = ProjectStatus.ToDo,
                Attachment = ""
            };

            await _projectTaskRepository.AddAsync(projectTask);
            await _projectTaskRepository.Save();

            return Ok(projectTask);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectTask(int id)
        {
            var projectTask = await _projectTaskRepository.GetByIdAsync(id);
            if (projectTask == null)
            {
                return NotFound();
            }

            return Ok(projectTask);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateTaskStatus(int id, ProjectStatus newStatus)
        {
            if (newStatus == null)
            {
                return BadRequest();
            }

            var projectTask = await _projectTaskRepository.GetByIdAsync(id);
            if (projectTask == null)
            {
                return NotFound();
            }

            projectTask.Status = newStatus;

            await _projectTaskRepository.UpdateAsync(projectTask);
            await _projectTaskRepository.Save();

            return Ok();
        }

        //Upload attachment
        
    }
}
