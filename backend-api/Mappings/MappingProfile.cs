using AutoMapper;
using MiCareerAcer.Api.DTOs;
using MiCareerAcer.Api.Models;

namespace MiCareerAcer.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Job, JobResponse>();
        CreateMap<AgentSession, AgentSessionResponse>()
            .ForMember(d => d.AgentType, o => o.MapFrom(s => s.AgentType.ToString()));
    }
}
