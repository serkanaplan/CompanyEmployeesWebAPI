
using AutoMapper;
using Entities.Models;
using Shared.DTO;

namespace CompanyEmployees.API.MapProfile;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Company, CompanyDto>().ForCtorParam("FullAddress", opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
        CreateMap<Employee,  EmployeeDto>();
        CreateMap<CompanyForCreationDto, Company>(); 
        CreateMap<EmployeeForCreationDto, Employee>();
        CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();//reversemap patch işlemi için
        CreateMap<CompanyForUpdateDto, Company>().ReverseMap();//reversemap patch işlemi için
    }
}