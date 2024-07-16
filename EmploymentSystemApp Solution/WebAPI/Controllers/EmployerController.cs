using AutoMapper;
using ApplicationLayer.DTO;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using ApplicationLayer.Helper;
using Microsoft.Extensions.Caching.Memory;
using System.Xml.Linq;

namespace EmploymentSystemApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class EmployerController : ControllerBase
    {
        private readonly IEmployerRepository _employerRepository;
        private readonly IMapper _mapper;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly ILogger<EmployerController> _logger;
        private readonly IMemoryCache _memoryCache;

        public EmployerController(
                                    IEmployerRepository employerRepository ,
                                    IMapper mapper ,
                                    IVacancyRepository vacancyRepository ,
                                    IApplicationRepository applicationRepository,
                                    ILogger<EmployerController> logger,
                                    IMemoryCache memoryCache
            )
        {
            _employerRepository = employerRepository;
            _mapper = mapper;
            _vacancyRepository = vacancyRepository;
            _applicationRepository = applicationRepository;
            _logger = logger;
            _memoryCache = memoryCache;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllEmployers() 
        {
            _logger.LogInformation("Get All Employers In The System");
            var cacheKey = "GetAllEmployers";
            
            if(!_memoryCache.TryGetValue(cacheKey , out var employersDTO))
            {
                var employers = await _employerRepository.GetAllEmployers();
                employersDTO = _mapper.Map<List<EmployerDTO>>(employers);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid Model State");
                    return BadRequest(ModelState);
                }

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey , employersDTO , cacheOptions);

                _logger.LogInformation("All Employers Are Cached Now");
            }
            else
            {
                _logger.LogInformation("All Employers Are Already In Cache");
            }

            return Ok(employersDTO);

        }

        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetEmployerById(int id)
        {

            _logger.LogInformation($"Get Employer With Id = {id}");

            var cacheKey = $"Employer{id}";

            if(!_memoryCache.TryGetValue(cacheKey , out var employerDTO))
            {
                if (!await _employerRepository.isEmployerExists(id))
                {
                    _logger.LogWarning($"Employer with Id = {id} is not found");
                    return NotFound();
                }

                _logger.LogInformation($"Employer With Id = {id} is not in cache");

                var employer = await _employerRepository.GetEmployerById(id);
                employerDTO = _mapper.Map<EmployerDTO>(employer);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model state is invalid");
                    return BadRequest(ModelState);
                }

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, employerDTO, cacheOptions);
                _logger.LogInformation($"Employer With Id = {id} is cached now");

            }
            else
            {
                _logger.LogInformation($"Employer With Id = {id} is Already In Cache");
            }
   
            return Ok(employerDTO);
        }

        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetEmployerByName(string name)
        {

            _logger.LogInformation($"Get Employer With Name = {name}");
            var cacheKey = $"Employer_{name}";

            if(!_memoryCache.TryGetValue(cacheKey , out var employerDTO))
            {
                if (!await _employerRepository.isEmployerExists(name))
                {
                    _logger.LogWarning($"Employee with name = {name} is not found");
                    return NotFound();
                }

                _logger.LogInformation($"Employer With name = {name} is not in cache");

                var employer = await _employerRepository.GetEmployerByName(name);
                employerDTO = _mapper.Map<EmployerDTO>(employer);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model state is invalid");
                    return BadRequest(ModelState);
                }

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, employerDTO, cacheOptions);
                _logger.LogInformation($"Employer With Name = {name} is cached now");
                
            }
            else
            {
                _logger.LogInformation($"Employer With Name = {name} is already in cache");
            }

            return Ok(employerDTO);

        }

        [HttpPost]
        [Authorize(Policy = "EmployerOnly")]
        [Route("CreateVacancy")]
        public async Task<IActionResult> CreateVacancy([FromBody] VacancyDTO vacancyDTO)
        {
            _logger.LogInformation($"Employee With Id = {User.FindFirstValue(ClaimTypes.NameIdentifier)} and Name = {User.FindFirstValue(ClaimTypes.Name)} is creating vacancy");

            if (vacancyDTO == null)
            {
                _logger.LogWarning("VacancyDTO is null");
                return BadRequest(ModelState);
            }

            var existed = await _vacancyRepository.GetVacancyByTitle(vacancyDTO.Title);
            if (existed != null)
            {
                _logger.LogWarning($"Vacancy with title: {vacancyDTO.Title} already exists");
                ModelState.AddModelError("Existed", "Vacancy with this title already existed");
                return StatusCode(422, ModelState);
            }

            var employerName = User.FindFirstValue(ClaimTypes.Name);
            var employer = await _employerRepository.GetEmployerByName(employerName);


            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state");
                return BadRequest(ModelState);
            }

            var vacancy = _mapper.Map<Vacancy>(vacancyDTO);

            vacancy.Employer = employer;

            vacancy.ArchivedDate = vacancy.ExpireDate.AddSeconds(1);

            if (vacancy.MaxApplications == 0)
            {
                _logger.LogWarning("Vacancy max applications cannot be zero");
                ModelState.AddModelError("Invalid", "Vacancy Max Application Cannot Be Zero");
                return StatusCode(400, ModelState);
            }
            if (!await _employerRepository.CreateVacancy(vacancy))
            {
                _logger.LogError("Failed to create vacancy");
                ModelState.AddModelError("Not Created", "Failed To Create");
                return StatusCode(500, ModelState);
            }

            _memoryCache.Remove("GetAllVacancies");

            _logger.LogInformation("Vacancy created successfully");
            return Ok("Succusfully Created");
        }

        [HttpPut]
        [Authorize(Policy = "EmployerOnly")]
        [Route("UpdateVacancy")]
        public async Task<IActionResult> UpdateVacancy(int vacancyId , [FromBody] VacancyDTO vacancyDTO) 
        {

            _logger.LogInformation($"Updating vacancy with ID: {vacancyId} by Employer = {User.FindFirstValue(ClaimTypes.Name)}");

            if (vacancyDTO == null)
            {
                _logger.LogWarning("Vacancy DTO is null");
                return BadRequest(ModelState);
            }

            var existingVacancy = await _vacancyRepository.GetVacancyById(vacancyId);

            if (existingVacancy == null)
            {
                _logger.LogWarning($"Vacancy with ID: {vacancyId} not found");
                return NotFound();
            }

            if (existingVacancy.EmployerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state");
                return BadRequest(ModelState);
            }

            existingVacancy.ArchivedDate = existingVacancy.ExpireDate.AddSeconds(1);
            _mapper.Map(vacancyDTO, existingVacancy);

            if (!await _employerRepository.UpdateVacancy(existingVacancy))
            {
                _logger.LogError("Failed to update vacancy");
                ModelState.AddModelError("", "Something Went Wrong At Updating");
                return StatusCode(500, ModelState);
            }

            _memoryCache.Remove($"vacancy_{vacancyId}");

            _logger.LogInformation("Vacancy updated successfully");
            return Ok("Updated Successfuly");
        }

        [HttpDelete]
        [Authorize(Policy = "EmployerOnly")]
        [Route("DeleteVacancy")]
        public async Task<IActionResult> DeleteVacancy(int vacancyId)
        {

            _logger.LogInformation($"Deleting vacancy with ID: {vacancyId}");

            if (!await _vacancyRepository.isVacancyExists(vacancyId))
            {
                _logger.LogWarning($"Vacancy with ID: {vacancyId} not found");
                return NotFound();
            }

            var vacancyToDelete = await _vacancyRepository.GetVacancyById(vacancyId);

            if(vacancyToDelete == null)
            {
                _logger.LogWarning("Vacancy to delete is null");
                return BadRequest(ModelState);
            }

            if (vacancyToDelete.EmployerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state");
                return BadRequest();
            }


            foreach (var Application in vacancyToDelete.Applications)
            {
                _logger.LogInformation("Applications Linked With Vacancy Will Be Deleted");
                _applicationRepository.deleteApplication(Application);
            }

            if(!await _employerRepository.DeleteVacancy(vacancyToDelete))
            {
                _logger.LogError("Failed to delete vacancy");
                ModelState.AddModelError("", "Something went wrong while deleting");
                return StatusCode(500, ModelState);
            }

            _memoryCache.Remove($"GetAllVacancies");

            _logger.LogInformation("Vacancy deleted successfully");
            return Ok("Deleted");
        }

        [HttpGet]
        [Authorize(Policy = "EmployerOnly")]
        [Route("ShowApplicantsOfVacancy")]
        public async Task<IActionResult> GetApplicantsOfVacancy(int vacancyId)
        {
            _logger.LogInformation($"Getting applicants for vacancy with ID: {vacancyId}");

            if (!await _vacancyRepository.isVacancyExists(vacancyId))
            {
                _logger.LogWarning($"Vacancy with ID: {vacancyId} not found");
                return NotFound();
            }

            var targetedVacancy = await _vacancyRepository.GetVacancyById(vacancyId);

            if (targetedVacancy == null)
            {
                _logger.LogWarning("Targeted vacancy is null");
                return BadRequest(ModelState);
            }

            if (targetedVacancy.EmployerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state");
                return BadRequest();
            }

            return Ok(targetedVacancy.Applications.Select(AP => AP.Applicant.Username));
        }


        [HttpPut]
        [Authorize(Policy = "EmployerOnly")]
        [Route("Deactive Vacancy")]
        public async Task<IActionResult> DeactiveVacancy(int vacancyId)
        {
            _logger.LogInformation($"Deactivating vacancy with ID: {vacancyId}");

            if (!await _vacancyRepository.isVacancyExists(vacancyId))
            {
                _logger.LogWarning($"Vacancy with ID: {vacancyId} not found");
                return NotFound();
            }

            var existingVacancy = await _vacancyRepository.GetVacancyById(vacancyId);

            if (!existingVacancy.IsActive)
            {
                _logger.LogWarning("Vacancy is already deactivated");
                ModelState.AddModelError("", "Vacancy Is Already Deactive");
                return StatusCode(400, ModelState);
            }

            

            if (existingVacancy == null)
            {
                _logger.LogWarning($"Vacancy with ID: {vacancyId} not found");
                return NotFound();
            }

            if (existingVacancy.EmployerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state");
                return BadRequest(ModelState);
            }


            existingVacancy.IsActive = false;

            if (!await _employerRepository.UpdateVacancy(existingVacancy))
            {
                _logger.LogError("Failed to deactivate vacancy");
                ModelState.AddModelError("", "Something Went Wrong At Updating");
                return StatusCode(500, ModelState);
            }

            _memoryCache.Remove($"vacancy_{vacancyId}");

            _logger.LogInformation("Vacancy deactivated successfully");
            return Ok("Vacancy Deactivated");
        }

        [HttpPut]
        [Authorize(Policy = "EmployerOnly")]
        [Route("Active Vacancy")]
        public async Task<IActionResult> ActiveVacancy(int vacancyId)
        {

            _logger.LogInformation($"Activating vacancy with ID: {vacancyId}");

            if (!await _vacancyRepository.isVacancyExists(vacancyId))
            {
                _logger.LogWarning($"Vacancy with ID: {vacancyId} not found");

                return NotFound();
            }

            var existingVacancy = await _vacancyRepository.GetVacancyById(vacancyId);

            if (existingVacancy.IsActive)
            {
                _logger.LogWarning("Vacancy is already active");
                ModelState.AddModelError("", "Vacancy Is Already Active");
                return StatusCode(400, ModelState);
            }

            

            if (existingVacancy == null)
            {
                _logger.LogWarning($"Vacancy with ID: {vacancyId} not found");
                return NotFound();
            }

            if (existingVacancy.EmployerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state");
                return BadRequest(ModelState);
            }


            existingVacancy.IsActive = true;

            if (!await _employerRepository.UpdateVacancy(existingVacancy))
            {
                _logger.LogError("Failed to activate vacancy");
                ModelState.AddModelError("", "Something Went Wrong At Updating");
                return StatusCode(500, ModelState);
            }

            _memoryCache.Remove($"vacancy_{vacancyId}");

            _logger.LogInformation("Vacancy activated successfully");
            return Ok("Vacancy Activated");
        }

        [HttpGet]
        [Authorize(Policy = "EmployerOnly")]
        [Route("GetArchievedVacancies")]
        public async Task<IActionResult> GetArchievedVacancies()
        {
            var cacheKey = "GetArchievedVacancies";

            _logger.LogInformation("Getting archived vacancies");

            if(!_memoryCache.TryGetValue(cacheKey , out var vacanciesDTO))
            {
                if ((await _vacancyRepository.GetArchievedVacancy()) == null)
                {
                    _logger.LogWarning("No archived vacancies found");
                    return NotFound();
                }

                _logger.LogInformation("No Archieved Vacancies in Cache");

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var archivedVacancies = await _vacancyRepository.GetArchievedVacancy();

                var vacancies = archivedVacancies.Where(V => V.EmployerId == userId);

                vacanciesDTO = _mapper.Map<List<VacancyDTO>>(vacancies);

                if (!ModelState.IsValid)
                {
                    _logger.LogInformation("Model State is invalid");
                    return BadRequest(ModelState);
                }

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, vacanciesDTO , cacheOptions);

                _logger.LogInformation("Archieved Vacancies Are Cached Now");
            }
            else
            {
                _logger.LogInformation("Archieved Vacancies Are Already Cached");
            }

            return Ok(vacanciesDTO);
        }

    }

    
}
