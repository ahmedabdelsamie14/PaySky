using Infrastructure.Data;

using Core.Models;
using Microsoft.EntityFrameworkCore;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class ApplicantRepository : IApplicantRepository
    {
        private readonly EmploymentSystemDbContext _context;

        public ApplicantRepository(EmploymentSystemDbContext context) 
        {
            _context = context;
        }

        public async Task<bool> ApplyToVacancy(Application application)
        {
            await _context.Applications.AddAsync(application);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ICollection<Applicant>> GetAllApplicants()
        {
            return await _context.Applicants.
                Include(A => A.Applications).
                ThenInclude(Ap => Ap.Vacancy).
                OrderBy(A => A.UserId).ToListAsync();
        }

        public async Task<Applicant> GetApplicantById(int id)
        {
            return await _context.Applicants.Where(A => A.UserId == id)
                .Include(A => A.Applications)
                .ThenInclude(Ap => Ap.Vacancy)
                .FirstOrDefaultAsync();
        }

        public async Task<Applicant> GetApplicantByName(string name)
        {
            return await _context.Applicants.Where(A => A.Username == name)
                .Include(A => A.Applications)
                .ThenInclude(Ap => Ap.Vacancy)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> isApplicantExists(int id)
        {
            return await _context.Applicants.AnyAsync(A => A.UserId == id);
        }

        public async Task<bool> isApplicantExists(string name)
        {
            return await _context.Applicants.AnyAsync(A => A.Username == name);
        }
    }
}
