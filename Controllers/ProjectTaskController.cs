﻿using Microsoft.AspNetCore.Mvc;
using ia_back.Data;
using ia_back.DTOs.TaskDTO;
using ia_back.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text;

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
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var fileContent = await reader.ReadToEndAsync();
                var attachment = file.FileName + '-' + fileContent;
                var base64FileContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(attachment));
                projectTask.Attachment = base64FileContent;
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
            var fileContent = Encoding.UTF8.GetString(Convert.FromBase64String(base64FileContent));
            var fileName = fileContent.Split('-')[0];
            var fileContentWithoutName = fileContent.Substring(fileName.Length + 1);
            var bytes = Encoding.UTF8.GetBytes(fileContentWithoutName);
            return File(bytes, "application/octet-stream", fileName);
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
            var fileContent = Encoding.UTF8.GetString(Convert.FromBase64String(base64FileContent));
            var fileName = fileContent.Split('-')[0];
            return Ok(fileName);
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
