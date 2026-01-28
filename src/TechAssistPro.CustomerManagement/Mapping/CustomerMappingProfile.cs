using AutoMapper;
using TechAssistPro.CustomerManagement.Application;
using TechAssistPro.CustomerManagement.Entities;
using TechAssistPro.SharedKernel.Common;

namespace TechAssistPro.CustomerManagement.Mapping
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {


            CreateMap<CustomerCreateDto, CustomerCreateCommand>()
                .ForCtorParam(nameof(CustomerCreateCommand.Name),
                    opt => opt.MapFrom(src => src.Name))
                .ForCtorParam(nameof(CustomerCreateCommand.Email),
                    opt => opt.MapFrom(src => src.Email))
                .ForCtorParam(nameof(CustomerCreateCommand.PhoneNumber),
                    opt => opt.MapFrom(src => src.PhoneNumber));

            CreateMap<Customer, CustomerCreateResponse>()
.ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id));
        }
    }
}