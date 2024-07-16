using AutoMapper;
using ApplicationLayer.DTO;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using ApplicationLayer.Helper;
using Microsoft.Extensions.Caching.Memory;

namespace EmploymentSystemApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicantController : ControllerBase
    {
        private readonly IApplicantRepository _applicantRepository;
        private readonly IMapper _mapper;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ApplicantController> _logger;

        public ApplicantController(IApplicantRepository applicantRepository , IMapper mapper , IVacancyRepository vacancyRepository , IMemoryCache memoryCache , ILogger<ApplicantController> logger)
        {
            _applicantRepository = applicantRepository;
            _mapper = mapper;
            _vacancyRepository = vacancyRepository;
            _memoryCache = memoryCache;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllApplicants()
        {
            _logger.LogInformation("Get All Applicants");
            var cacheKey = "GetAllApplicants";

            if(_memoryCache.TryGetValue(cacheKey , out var applicantsDTO))
            {
                _logger.LogInformation("Applicants Are In Memory Cache");
            }
            else
            {
                

                var applicants = await _applicantRepository.GetAllApplicants();
                applicantsDTO = _mapper.Map<List<ApplicantDTO>>(applicants);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, applicantsDTO, cacheOptions);

                _logger.LogInformation("Applicants Are Putted In Cache Now");
            }

            if(!ModelState.IsValid)
            {
                _logger.LogWarning("Model State Is Not Valid");
                return BadRequest(ModelState);
            }

            return Ok(applicantsDTO);
        }

        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetApplicantById(int id)
        {
            _logger.LogInformation($"Get Applicant With Id = {id}");
            var cacheKey = $"applicant_{id}";

            if(!_memoryCache.TryGetValue(cacheKey , out var applicantDTO))
            {
                _logger.LogInformation($"Applicant With Id = {id} isn't in the cache");

                if (! await _applicantRepository.isApplicantExists(id))
                {
                    _logger.LogWarning($"Get Applicant With Id = {id} is not found");
                    return NotFound();
                }

                var applicant = await _applicantRepository.GetApplicantById(id);
                applicantDTO = _mapper.Map<ApplicantDTO>(applicant);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, applicantDTO, cacheEntryOptions);

                _logger.LogInformation($"Applicant With Id = {id} is putted cache now");
            }
            else
            {
                _logger.LogInformation($"Applicant With Id = {id} is already in cache");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model State is not valid");
                return BadRequest(ModelState);
            }

            return Ok(applicantDTO);
        }

        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetApplicantByName(string name)
        {
            _logger.LogInformation($"Get Applicant With Name = {name}");

            var cacheKey = $"applicant_{name}";

            if (!_memoryCache.TryGetValue(cacheKey, out var applicantDTO))
            {
                _logger.LogInformation($"Applicant With Name = {name} is not in the cache");

                if (!await _applicantRepository.isApplicantExists(name))
                {
                    _logger.LogWarning($"Get Applicant With Name = {name} is not found");
                    return NotFound();
                }

                var applicant = await _applicantRepository.GetApplicantByName(name);
                applicantDTO = _mapper.Map<ApplicantDTO>(applicant);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, applicantDTO, cacheEntryOptions);

                _logger.LogInformation($"Applicant With Name = {name} putted in cache now");
            }
            else
            {
                _logger.LogInformation($"Applicant With Name = {name} is already in cache");
            }
            

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model State is not valid");
                return BadRequest(ModelState);
            }
            return Ok(applicantDTO);
        }

        [HttpGet]
        [Route("ApplicantSearchForVacancy")]
        public async Task<IActionResult> SearchForVacancy(string title) 
        {
            _logger.LogInformation($"Searching For Vacancy With Title = {title}");

            var cacheKey = $"Vacancy_{title}";

            if(!_memoryCache.TryGetValue(cacheKey, out var vacancyDTO))
            {
                _logger.LogInformation($"Vacancy With Title : {title} is not in cache");

                if (!await _vacancyRepository.isVacancyExists(title))
                {
                    _logger.LogWarning($"Vacancy With Title = {title} is not found");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model State is not found");

                    return BadRequest(ModelState);
                }

                var vacancy = await _vacancyRepository.GetVacancyByTitle(title);

                if (vacancy == null)
                {
                    _logger.LogWarning($"Vacancy with {title} came with null");
                    return BadRequest(ModelState);
                }

                vacancyDTO = _mapper.Map<VacancyDTO>(vacancy);

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, vacancyDTO, cacheEntryOptions);
                _logger.LogInformation($"Vacancy With Title = {title} putted in cache now");
            }
            else
            {
                _logger.LogInformation($"Vacancy With Title = {title} Already in the cache");
            }

            return Ok(vacancyDTO);
        }

        [HttpPost]
        [Route("ApplyToVacancy")]
        [Authorize(Policy = "ApplicantOnly")]
        public async Task<IActionResult> ApplyToVacancy(string vacancyTitle)
        {
            var applicantIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation($"Applicant With Id = {applicantIdClaim} in Vacancy with Title = {vacancyTitle}");

            var lastApplication = (await _applicantRepository.GetApplicantById(int.Parse(applicantIdClaim))).Applications.LastOrDefault();


            if (lastApplication != null)
            {


                var lastApplicationDate = lastApplication.ApplicationDate;

                _logger.LogInformation($"Last Application The Applicant Did Was {lastApplicationDate}");

                if (lastApplicationDate.AddHours(24) > DateTime.UtcNow)
                {
                    _logger.LogWarning("You Need To Wait 24 Hours Before Applying");
                    ModelState.AddModelError("More 24 Hours", "You Need To Wait 24 Hours Before Apply Again");
                    return StatusCode(400, ModelState);
                }
            }



            if (!await _vacancyRepository.isVacancyExists(vacancyTitle))
            {
                _logger.LogWarning("Vacancy That Applicant Want To Apply is not found");
                return NotFound();
            }

            var vacancy = await _vacancyRepository.GetVacancyByTitle(vacancyTitle);

            if (!(vacancy.Applications.Count() < vacancy.MaxApplications))
            {
                _logger.LogWarning("Vacancy That Applicant Want To Apply is Full Capacity");
                ModelState.AddModelError("Full Capacity", "Vacancy Application is FULL");
                return BadRequest(ModelState);
            }
            else if (DateTime.Now > vacancy.ArchivedDate)
            {
                _logger.LogWarning("Vacancy That Applicant Want To Apply Expired And Will Be Archieved");
                ModelState.AddModelError("Archieved", "Vacancy is expired you can't apply");
                return BadRequest(ModelState);
            }
            else
            {


                var vacancyId = (await _vacancyRepository.GetVacancyByTitle(vacancyTitle)).VacancyId;

                var application = new Application
                {
                    ApplicantId = int.Parse(applicantIdClaim),
                    VacancyId = vacancyId,
                    ApplicationDate = DateTime.UtcNow
                };


                if (!await _applicantRepository.ApplyToVacancy(application))
                {
                    _logger.LogWarning("Failed To Apply");
                    ModelState.AddModelError("Not Applied", "Failed To Apply");
                    return StatusCode(500, ModelState);
                }

                _logger.LogInformation("Application Submitted");
                return Ok("Application Submitted");
            }

        }
    }
}
