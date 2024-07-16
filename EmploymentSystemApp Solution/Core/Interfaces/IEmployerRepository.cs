using Core.Models;

namespace Core.Interfaces
{
    public interface IEmployerRepository
    {
        Task<ICollection<Employer>> GetAllEmployers();

        Task<Employer> GetEmployerById(int id);

        Task<Employer> GetEmployerByName(string name);

        Task<bool> isEmployerExists(int id);

        Task<bool> isEmployerExists(string name);

        Task<bool> DeleteVacancy(Vacancy vacancy);

        Task<bool> CreateVacancy(Vacancy vacancy);

        Task<bool> UpdateVacancy(Vacancy vacancy);

        Task<bool> Save();
    }
}
