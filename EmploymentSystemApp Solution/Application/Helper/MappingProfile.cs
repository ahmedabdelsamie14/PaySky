using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using ApplicationLayer.DTO;
using Core.Interfaces;


namespace ApplicationLayer.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Employer, EmployerDTO>()
                .ForMember(dest => dest.Vacancies, opt => opt.MapFrom(src => src.Vacancies));

            CreateMap<EmployerDTO, Employer>()
                .ForMember(dest => dest.Vacancies, opt => opt.MapFrom(src => src.Vacancies));

            CreateMap<Applicant, ApplicantDTO>()
                .ForMember(dest => dest.Applications, opt => opt.MapFrom(src => src.Applications));

            CreateMap<ApplicantDTO, Applicant>()
                .ForMember(dest => dest.Applications, opt => opt.MapFrom(src => src.Applications));

            CreateMap<Vacancy, VacancyDTO>()
            .ForPath(dest => dest.EmployerName, opt => opt.MapFrom(src => src.Employer.Username));
            
            

            CreateMap<VacancyDTO, Vacancy>()
                .ForPath(dest => dest.Employer.Username ,opt => opt.MapFrom(src => src.EmployerName));


            CreateMap<Application, ApplicationDTO>()
                .ForPath(dest => dest.ApplicantName, opt => opt.MapFrom(src => src.Applicant.Username))
                .ForPath(dest => dest.VacancyTitle, opt => opt.MapFrom(src => src.Vacancy.Title));

            CreateMap<ApplicationDTO, Application>()
                .ForPath(dest => dest.Applicant.Username, opt => opt.MapFrom(src => src.ApplicantName))
                .ForPath(dest => dest.Vacancy.Title, opt => opt.MapFrom(src => src.VacancyTitle));

            CreateMap<RegisterApplicantDTO, Applicant>();

            CreateMap<RegisterEmployerDTO, Employer>();
        }

    }
}
