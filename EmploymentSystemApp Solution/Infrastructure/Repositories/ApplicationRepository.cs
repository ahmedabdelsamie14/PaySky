using Infrastructure.Data;
using Core.Interfaces;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly EmploymentSystemDbContext _context;

        public ApplicationRepository(EmploymentSystemDbContext context)
        {
            _context = context;
        }

        public async Task<bool> deleteApplication(Application application)
        {
            _context.Applications.Remove(application);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ICollection<Application>> GetAllApplications()
        {
            return await _context.Applications.Include(AP => AP.Applicant)
                .Include(AP => AP.Vacancy)
                .OrderBy(AP => AP.ApplicationId)
                .ToListAsync();
        }

        public async Task<ICollection<Application>> GetApplicationsByApplicantName(string applicantName)
        {
            return await _context.Applications
                .Include(AP => AP.Applicant)
                .Where(AP => AP.Applicant.Username == applicantName)
                .Include(AP => AP.Vacancy)
                .OrderBy(AP => AP.ApplicationId)
                .ToListAsync();
                
        }

        public async Task<ICollection<Application>> GetApplicationsByApplicationDate(DateTime dateTime)
        {
            return await _context.Applications
                .Where(AP => AP.ApplicationDate == dateTime)
                .Include(AP => AP.Applicant)
                .Include(AP => AP.Vacancy)
                .OrderBy(AP => AP.ApplicationId)
                .ToListAsync();
        }

        public async Task<Application> GetApplicationById(int id)
        {
            return await _context.Applications
                .Where(AP => AP.ApplicationId == id)
                .Include(AP => AP.Applicant)
                .Include(AP => AP.Vacancy)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> isApplicationExists(int applicationId)
        {
            return await _context.Applications.AnyAsync(AP => AP.ApplicationId == applicationId);
        }

        public async Task<bool> isApplicationExists(string applicantName)
        {
            return await _context.Applications.Include(AP => AP.Applicant).AnyAsync(AP => AP.Applicant.Username == applicantName);
        }

        public async Task<bool> isApplicationExists(DateTime dateTime)
        {
            return await _context.Applications.AnyAsync(AP => AP.ApplicationDate == dateTime);
        }
    }
}
