using ia_back.Data.Custom_Repositories;
using ia_back.Data;
using ia_back.Models;
using Microsoft.AspNetCore.Mvc;
using ia_back.DTOs.ReqestDTO;


namespace ia_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectRequestContorller : Controller{
        private readonly IDataRepository<ProjectRequest> _projectRequestRepository;
        private readonly IDataRepository<User> _userRepository;
        private readonly IDataRepository<Project> _projectRepository;

        public ProjectRequestContorller(IDataRepository<ProjectRequest> projectRequestRepository, IDataRepository<User> userRepository, IDataRepository<Project> projectRepository)
        {
            _projectRequestRepository = projectRequestRepository;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectRequest(int id){
            var projectRequest = await _projectRequestRepository.GetByIdAsync(id);
            if (projectRequest == null)
            {
                return NotFound("Project request not found");
            }
            return Ok(projectRequest);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjectRequests(){
            var projectRequests = await _projectRequestRepository.GetAllAsync();
            if (projectRequests == null)
            {
                return NotFound("No project requests found");
            }
            return Ok(projectRequests);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateProjectRequest(ReqestEntryDTO ReqestEntryDTO){
            var project = await _projectRepository.GetByIdAsync(ReqestEntryDTO.ProjectId);
            var user = await _userRepository.GetByIdAsync(ReqestEntryDTO.UserId);
            if (project == null || user == null)
            {
                return NotFound("Project or user not found");
            }
            
            var projectRequest = new ProjectRequest
            {
                ProjectId = project.Id,
                Project = project,
                UserId = user.Id,
                User = user
            };
                        
            project.RequestedDevelopers.Add(user);
            await _projectRepository.UpdateAsync(project);
            await _projectRepository.Save(); 
            await _projectRequestRepository.AddAsync(projectRequest);
            await _projectRequestRepository.Save();
            return Ok(projectRequest); 
        }

        [HttpDelete("{RequestId}")]
        public async Task<IActionResult> DeleteProjectRequest(int RequestId){
            var projectRequest = await _projectRequestRepository.GetByIdAsync(RequestId);
            if (projectRequest == null)
            {
                return NotFound("Project request not found");
            }
            await _projectRequestRepository.DeleteAsync(projectRequest);
            await _projectRequestRepository.Save();
            return Ok(); 
        }

        [HttpPatch("{RequestId}")]
        public async Task<IActionResult> AcceptProjectRequest(int RequestId){
            var projectRequest = await _projectRequestRepository.GetByIdAsync(RequestId);
            if (projectRequest == null)
            {
                return NotFound("Project request not found");
            }
            var project = await _projectRepository.GetByIdAsync(projectRequest.ProjectId);
            var user = await _userRepository.GetByIdAsync(projectRequest.UserId);
            if (project == null || user == null)
            {
                return NotFound("Project or user not found");
            }

            if(this.DeleteProjectRequest(RequestId).Result == null){ 
                return NotFound("Request deletion failed");
            }

            project.AssignedDevelopers.Add(user);
            project.RequestedDevelopers.Remove(user);
            await _projectRepository.UpdateAsync(project);
            await _projectRepository.Save();
            return Ok();
        }

    

    }
}
