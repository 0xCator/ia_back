using AutoMapper;
using ia_back.DTOs.CommentDTO;
using ia_back.DTOs.ProjectDTO;
using ia_back.DTOs.TaskDTO;
using ia_back.Models;

namespace ia_back.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, DeveloperInfo>().ReverseMap();

            CreateMap<Project, ProjectCardDTO>().ReverseMap();
            CreateMap<Project, ProjectInfoDTO>()
                .ForMember(dest => dest.AssignedDevelopers, 
                           opt => opt.MapFrom(src => src.AssignedDevelopers))
                .ReverseMap();
            
            CreateMap<ProjectTask, TaskCardDTO>()
                .ForMember(dest => dest.AssignedDev,
                           opt => opt.MapFrom(src => src.AssignedDev))
                .ReverseMap();
            CreateMap<ProjectTask, TaskInfoDTO>()
                .ForMember(dest => dest.AssignedDev,
                           opt => opt.MapFrom(src => src.AssignedDev))
                .ReverseMap();

            CreateMap<Comment, CommentOutputDTO>()
                .ForMember(dest => dest.CommenterInfo,
                           opt => opt.MapFrom(src => src.User))
                .ReverseMap();
        }
    }
}
