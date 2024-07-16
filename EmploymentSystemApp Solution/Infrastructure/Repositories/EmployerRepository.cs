using Infrastructure.Data;
using Core.Interfaces;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class EmployerRepository : IEmployerRepository
    {
        private readonly EmploymentSystemDbContext _context;

        public EmployerRepository(EmploymentSystemDbContext context)
        {
            _context = context;
        }
        public async Task<ICollection<Employer>> GetAllEmployers()
        {
            return await _context.Employers.Include(E => E.Vacancies).OrderBy(E => E.UserId).ToListAsync();
        }

        public async Task<Employer> GetEmployerById(int id)
        {
            return await _context.Employers
                .Where(E => E.UserId == id)
                .Include(E => E.Vacancies)
                .FirstOrDefaultAsync();
        }

        public async Task<Employer> GetEmployerByName(string name)
        {
            return await _context.Employers
                .Where(E => E.Username == name)
                .Include(E => E.Vacancies)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> isEmployerExists(int id)
        {
            return await _context.Employers.AnyAsync(E => E.UserId == id);
        }

        public async Task<bool> isEmployerExists(string name)
        {
            return await _context.Employers.AnyAsync(E => E.Username == name);
        }

        public async Task<bool> Save()
        {
            var saved = _context.SaveChangesAsync();
            return await saved > 0 ? true : false;
        }

        public async Task<bool> CreateVacancy(Vacancy vacancy)
        {
            await _context.Vacancies.AddAsync(vacancy);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateVacancy(Vacancy vacancy)
        {
            _context.Vacancies.Update(vacancy);
            return await Save();
        }

        public async Task<bool> DeleteVacancy(Vacancy vacancy)
        {
            _context.Vacancies.Remove(vacancy);
            return await Save();
        }
    }
}
