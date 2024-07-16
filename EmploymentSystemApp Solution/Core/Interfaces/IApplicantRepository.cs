
using Core.Models;

namespace Core.Interfaces
{
    public interface IApplicantRepository
    {
        Task<ICollection<Applicant>> GetAllApplicants();

        Task<Applicant> GetApplicantById(int id);

        Task<Applicant> GetApplicantByName(string name);

        Task<bool> isApplicantExists(int id);

        Task<bool> isApplicantExists(string name);

        Task<bool> ApplyToVacancy(Application application);
    }
}
