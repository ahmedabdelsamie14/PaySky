namespace Core.Models
{
    public class Employer : User
    {
        public string Location { get; set; }
        public string? AdditionalContactInfo { get; set; }

        public ICollection<Vacancy> Vacancies { get; set; }
    }
}
