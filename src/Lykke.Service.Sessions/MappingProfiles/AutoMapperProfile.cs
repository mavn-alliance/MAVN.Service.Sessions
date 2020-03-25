using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.Sessions.Client.Models;
using Lykke.Service.Sessions.Core.Services;

namespace Lykke.Service.Sessions.MappingProfiles
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
