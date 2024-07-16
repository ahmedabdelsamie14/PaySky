namespace ApplicationLayer.DTO
{
    public class VacancyDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int MaxApplications { get; set; }
        public DateTime ExpireDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public string EmployerName { get; set; }

        //public int EmployerId { get; set; }
        

    }
}
