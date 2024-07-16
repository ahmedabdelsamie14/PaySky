using Core.Models;
namespace Core.Models
{
    public class Applicant : User
    {
        public string Experience { get; set; }
        public string Skills { get; set; }

        public ICollection<Application> Applications { get; set; }
    }
}
