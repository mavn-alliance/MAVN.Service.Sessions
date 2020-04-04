using AutoMapper;
using JetBrains.Annotations;
using MAVN.Service.Sessions.Client.Models;
using MAVN.Service.Sessions.Core.Services;

namespace MAVN.Service.Sessions.MappingProfiles
{
    [UsedImplicitly]
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IClientSession, ClientSession>();
        }
    }
}
