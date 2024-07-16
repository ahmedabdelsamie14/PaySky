namespace ApplicationLayer.DTO
{
    public class ApplicantDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }

        public List<ApplicationDTO> Applications { get; set; }
    }
}
