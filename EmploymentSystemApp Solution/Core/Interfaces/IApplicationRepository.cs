
using Core.Models;

namespace Core.Interfaces
{
    public interface IApplicationRepository
    {
        Task<ICollection<Application>> GetAllApplications();

        Task<Application> GetApplicationById(int id);

        Task<ICollection<Application>> GetApplicationsByApplicantName(string applicantName);

        Task<ICollection<Application>> GetApplicationsByApplicationDate(DateTime dateTime);

        Task<bool> isApplicationExists(int applicationId);

        Task<bool> isApplicationExists(string applicantName);

        Task<bool> isApplicationExists(DateTime dateTime);

        Task<bool> deleteApplication(Application application);
    }
}
