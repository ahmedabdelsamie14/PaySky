using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Application
    {
        public int ApplicationId { get; set; }

        public DateTime ApplicationDate { get; set; }

        public int VacancyId { get; set; }
        public Vacancy Vacancy { get; set; }

        public int ApplicantId { get; set; }
        public Applicant Applicant { get; set; }

    }
}
