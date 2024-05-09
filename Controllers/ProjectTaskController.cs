using Microsoft.AspNetCore.Mvc;
using ia_back.Data;
using ia_back.DTOs.TaskDTO;
using ia_back.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using ia_back.WebSocket;
using System.Linq.Expressions;
using AutoMapper;
using ia_back.DTOs.ProjectDTO;

namespace ia_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectTaskController : Controller
    {
        private readonly IDataRepository<ProjectTask> _projectTaskRepository;
        private readonly IDataRepository<Project> _projectRepository;
        private readonly IMapper _mapper;
        private readonly SocketManager _socketManager = SocketManager.Instance;


        public ProjectTaskController(IMapper mapper, 
                                    IDataRepository<ProjectTask> projectTaskRepository,
                                    IDataRepository<Project> projectRepository)
        {
            _mapper = mapper;
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
        }


        [Authorize]
        [HttpGet("project/{id}")]
        public async Task<IActionResult> GetProjectTasksByProject(int id)
        {
            Expression<Func<Project, bool>> criteria = p => p.Id == id;
            var project = await _projectRepository.GetByIdIncludeAsync(criteria, 
                                                                       p => p.Tasks,
                                                                       p => p.AssignedDevelopers);
            if (project == null)
            {
                return NotFound();
            }

            var projectTasks = _mapper.Map<ICollection<ProjectTask>, ICollection<TaskCardDTO>>(project.Tasks);

            return Ok(projectTasks);
        }


        [Authorize(Roles = "TeamLeader")]
        [HttpPost]
        public async Task<IActionResult> CreateProjectTask(TaskEntryDTO projectTaskInfo)
        {
            if (projectTaskInfo == null)
            {
                return BadRequest();
            }

            Expression<Func<Project, bool>> criteria = pt => pt.Id == projectTaskInfo.ProjectID;
            var project = await _projectRepository.GetByIdIncludeAsync(criteria,
                                                                       p => p.AssignedDevelopers);
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

            var assignedDevs = project.AssignedDevelopers;

            await _projectTaskRepository.AddAsync(projectTask);
            await _projectTaskRepository.Save();
            await _socketManager.TaskHasUpdate(assignedDevs);

            return Ok("Task created");
        }


        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectTask(int id)
        {
            Expression<Func<ProjectTask, bool>> criteria = pt => pt.Id == id;
            var projectTask = await _projectTaskRepository.GetByIdIncludeAsync(criteria,
                                                                               t => t.AssignedDev);
            if (projectTask == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ProjectTask, TaskInfoDTO>(projectTask));
        }


        [Authorize(Roles = "Developer")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, ProjectStatus newStatus)
        {

            var projectTask = await _projectTaskRepository.GetByIdAsync(id);
            if (projectTask == null)
            {
                return NotFound();
            }

            projectTask.Status = newStatus;

            Expression<Func<Project, bool>> criteria = pt => pt.Id == projectTask.ProjectId;
            var project = await _projectRepository.GetByIdIncludeAsync(criteria,
                                                                       p => p.AssignedDevelopers,
                                                                       p => p.TeamLeader);
            var assignedDevs = project.AssignedDevelopers;

            await _projectTaskRepository.UpdateAsync(projectTask);
            await _projectTaskRepository.Save();
            await _socketManager.TaskHasUpdate(assignedDevs);

            return Ok();
        }


        [Authorize(Roles = "TeamLeader")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskEntryDTO projectTaskInfo)
        {
            if (projectTaskInfo == null)
            {
                return BadRequest();
            }

            var projectTask = await _projectTaskRepository.GetByIdAsync(id);
            if (projectTask == null)
            {
                return NotFound();
            }

            Expression<Func<Project, bool>> criteria = pt => pt.Id == projectTask.ProjectId;
            var project = await _projectRepository.GetByIdIncludeAsync(criteria,
                                                                       p => p.AssignedDevelopers);

            var assignedDevs = _mapper.Map<Project, ProjectInfoDTO>(project).AssignedDevelopers;
            if (assignedDevs == null)
            {
                return NotFound();
            }

            if (projectTaskInfo.Name != null) projectTask.Name = projectTaskInfo.Name;
            if (projectTaskInfo.Description != null) projectTask.Description = projectTaskInfo.Description;
            if (projectTaskInfo.AssignedDevId != null)
            {
                // Check assignedDevs if it contains the new assignedDevId
                var assignedDev = project.AssignedDevelopers.FirstOrDefault(d => d.Id == projectTaskInfo.AssignedDevId);
                if (assignedDev == null)
                {
                    return NotFound("Developer doesn't exist");
                }
                projectTask.AssignedDevId = projectTaskInfo.AssignedDevId;
            }

            await _projectTaskRepository.UpdateAsync(projectTask);
            await _projectTaskRepository.Save();

            return Ok(projectTask);

        }


        [Authorize(Roles = "TeamLeader")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectTask(int id)
        {
            var projectTask = await _projectTaskRepository.GetByIdAsync(id);
            if (projectTask == null)
            {
                return NotFound();
            }

            await _projectTaskRepository.DeleteAsync(projectTask);
            await _projectTaskRepository.Save();

            return Ok("Task deleted successfully");
        }


        [Authorize(Roles = "Developer")]
        [HttpPost("{id}/UploadAttachment")]
        public async Task<IActionResult> UploadAttachment(int id, [FromForm] IFormFile file)
        {
            var projectTask = await _projectTaskRepository.GetByIdAsync(id);
            if (projectTask == null)
            {
                return NotFound();
            }

            if (file == null)
            {
                return BadRequest();
            }
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                if (memoryStream.Length == 0)
                {
                    return BadRequest();
                }
                var fileContent = memoryStream.ToArray();
                var fileName = Convert.ToBase64String(Encoding.UTF8.GetBytes(file.FileName));
                var base64FileContent = Convert.ToBase64String(fileContent);
                var fileContentWithFileName = fileName + "-" + base64FileContent;
                projectTask.Attachment = fileContentWithFileName;
                await _projectTaskRepository.UpdateAsync(projectTask);
                await _projectTaskRepository.Save();
                return Ok();
            }


        }


        [Authorize]
        [HttpGet("{id}/AttachmentFile")]
        public async Task<IActionResult> GetAttachmentFile(int id){
            var projectTask = await _projectTaskRepository.GetByIdAsync(id);
            if (projectTask == null)
            {
                return NotFound();
            }

            if (projectTask.Attachment == null)
            {
                return NotFound();
            }

            var base64FileContent = projectTask.Attachment;

            try
            {
                var fileName = base64FileContent.Split('-')[0];
                var fileContent = base64FileContent.Split('-')[1];
                var fileBytes = Convert.FromBase64String(fileContent);
                var fileNameString = Convert.FromBase64String(fileName);
                return File(fileBytes, "application/octet-stream", Encoding.UTF8.GetString(fileNameString));

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [Authorize]
        [HttpGet("{id}/AttachmentName")]
        public async Task<IActionResult> GetAttachmentName(int id){
            var projectTask = await _projectTaskRepository.GetByIdAsync(id);
            if (projectTask == null)
            {
                return NotFound();
            }

            if (projectTask.Attachment == null)
            {
                return NotFound();
            }

            var base64FileContent = projectTask.Attachment;
            var fileNameBase64 = base64FileContent.Split('-')[0];
            var fileName = Encoding.UTF8.GetString(Convert.FromBase64String(fileNameBase64));
            return Ok(fileName);
        }
    }
}
