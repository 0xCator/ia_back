﻿using Microsoft.AspNetCore.Mvc;
using ia_back.Data;
using ia_back.DTOs.TaskDTO;
using ia_back.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using ia_back.WebSocket;

namespace ia_back.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectTaskController : Controller
    {
        private readonly IDataRepository<ProjectTask> _projectTaskRepository;
        private readonly IDataRepository<Project> _projectRepository;
        private readonly SocketManager _socketManager = new SocketManager();


        public ProjectTaskController(IDataRepository<ProjectTask> projectTaskRepository,
                                    IDataRepository<Project> projectRepository)
        {
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
        }

        [HttpPost("{id}/UploadAttachment")]
        public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
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



        [HttpPut]
        public async Task<IActionResult> CreateProjectTask(TaskEntryDTO projectTaskInfo)
        {
            if (projectTaskInfo == null)
            {
                return BadRequest();
            }

            var project = await _projectRepository.GetByIdIncludeAsync(projectTaskInfo.ProjectID,
                                                                       p=>p.AssignedDevelopers);
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

            return Ok(projectTask);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectTask(int id)
        {
            var projectTask = await _projectTaskRepository.GetByIdIncludeAsync(id,
                                                                               t=>t.Comments,
                                                                               t=>t.AssignedDev);
            if (projectTask == null)
            {
                return NotFound();
            }

            return Ok(projectTask);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateTaskStatus(int id, ProjectStatus newStatus)
        {

            var projectTask = await _projectTaskRepository.GetByIdAsync(id);
            if (projectTask == null)
            {
                return NotFound();
            }

            projectTask.Status = newStatus;

            var project = await _projectRepository.GetByIdIncludeAsync(projectTask.ProjectId,
                                                                       p=>p.AssignedDevelopers);
            var assignedDevs = project.AssignedDevelopers;

            await _projectTaskRepository.UpdateAsync(projectTask);
            await _projectTaskRepository.Save();
            await _socketManager.TaskHasUpdate(assignedDevs);

            return Ok();
        }

        
    }
}
