using Infrastructure.Data;
using Core.Interfaces;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class VacancyRepository : IVacancyRepository
    {
        private readonly EmploymentSystemDbContext _context;

        public VacancyRepository(EmploymentSystemDbContext context)
        {
            _context = context;
        }

        

        public async Task<ICollection<Vacancy>> GetAllVacancies()
        {
            var allVacancies =  await _context.Vacancies.Include(V => V.Employer)
                .Include(V => V.Applications)
                .ThenInclude(Ap => Ap.Applicant)
                .OrderBy(V => V.VacancyId)
                .ToListAsync();

            foreach (var vacancy in allVacancies)
            {
                if(DateTime.Now > vacancy.ExpireDate)
                {
                    vacancy.IsActive = false;
                }
            }

            return allVacancies;
        }

        public async Task<ICollection<Vacancy>> GetArchievedVacancy()
        {
            var Archieved = await _context.Vacancies.Where(V => DateTime.Now > V.ExpireDate)
                .Include(V => V.Employer)
                .Include(V => V.Applications)
                .ThenInclude(AP => AP.Applicant)
                .OrderBy(V => V.VacancyId)
                .ToListAsync();

            foreach (var vacancy in Archieved)
            {
                vacancy.IsActive = false;
            }


            return Archieved;
        }

        public async Task<Vacancy> GetVacancyById(int id)
        {
            var vacancy = await _context.Vacancies
                .Where(V => V.VacancyId == id)
                .Include(V => V.Employer)
                .Include(V => V.Applications)
                .ThenInclude(Ap => Ap.Applicant)
                .FirstOrDefaultAsync();


            if (DateTime.Now > vacancy?.ExpireDate)
                vacancy.IsActive = false;

            return vacancy;
        }

        public async Task<Vacancy> GetVacancyByTitle(string title)
        {
            var vacancy = await _context.Vacancies
                .Where(V => V.Title == title)
                .Include(V => V.Employer)
                .Include(V => V.Applications)
                .ThenInclude(Ap => Ap.Applicant)
                .FirstOrDefaultAsync();

            if(DateTime.Now > vacancy?.ExpireDate)
                vacancy.IsActive=false;

            return vacancy;
        }

        public async Task<bool> isVacancyExists(string title)
        {
            return await _context.Vacancies.AnyAsync(V => V.Title == title);   
        }

        public async Task<bool> isVacancyExists(int id)
        {
            return await _context.Vacancies.AnyAsync(V => V.VacancyId == id);
        }

        
    }
}
