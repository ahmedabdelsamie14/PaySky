using Core.Models;

namespace Core.Interfaces
{
    public interface IVacancyRepository
    {
        Task<ICollection<Vacancy>> GetAllVacancies();

        Task<Vacancy> GetVacancyById(int id);

        Task<Vacancy> GetVacancyByTitle(string title);

        Task<bool> isVacancyExists(string title);

        Task<bool> isVacancyExists(int id);

        Task<ICollection<Vacancy>> GetArchievedVacancy();
        
    }
}
