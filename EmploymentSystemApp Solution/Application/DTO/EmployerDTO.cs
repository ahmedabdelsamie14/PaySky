namespace ApplicationLayer.DTO
{
    public class EmployerDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }

       
        public List<VacancyDTO> Vacancies { get; set; }
    }
}
