using AutoMapper;
using ApplicationLayer.DTO;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ApplicationLayer.Helper;
using Microsoft.Extensions.Caching.Memory;

namespace EmploymentSystemApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "EmployerOnly")]
    public class VacancyController : ControllerBase
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<VacancyController> _logger;
        private readonly IMemoryCache _memoryCache;

        public VacancyController(IVacancyRepository vacancyRepository , IMapper mapper , ILogger<VacancyController> logger , IMemoryCache memoryCache)
        {
            _vacancyRepository = vacancyRepository;
            _mapper = mapper;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAllVacancies()
        {
            _logger.LogInformation($"Get All Vacancies That Employer With Id = {int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))} Posted");

            var cacheKey = "GetAllVacancies";
            if(!_memoryCache.TryGetValue(cacheKey , out var vacanciesDTO))
            { 
                var vacancies = (await _vacancyRepository.GetAllVacancies()).Where(V => V.EmployerId == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
                vacanciesDTO = _mapper.Map<List<VacancyDTO>>(vacancies);

                if(vacanciesDTO == null)
                {
                    _logger.LogInformation("No Vacancies Founded");
                    return NotFound();
                }

                _logger.LogInformation("Vacancies Not in Cache");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model State is invalid");
                    return BadRequest(ModelState);
                }

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, vacanciesDTO, cacheOptions);

                _logger.LogInformation("Vacancies Are Cached Now");
            }
            else
            {
                _logger.LogInformation("Vacancies Are Already In Cache");
            }

            return Ok(vacanciesDTO);
        }

        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetVacancyById(int id)
        {
            _logger.LogInformation($"Get Vacancy With Id = {id} that posted by Curred LoggedIn Employer = {int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))}");

            var cacheKey = $"Vacancy_{id}";
            if(!_memoryCache.TryGetValue(cacheKey , out var vacancyDTO))
            {
                if (!await _vacancyRepository.isVacancyExists(id))
                {
                    _logger.LogWarning("Vacancy Is Not Found");
                    return NotFound();
                }

                if (!((await _vacancyRepository.GetVacancyById(id)).EmployerId == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))))
                {
                    _logger.LogWarning("Your Are Not Allowed To See This Vacancy");
                    return Unauthorized();
                }

                _logger.LogInformation($"Vacancy with Id = {id} is not cached");

                var vacancy = await _vacancyRepository.GetVacancyById(id);
                vacancyDTO = _mapper.Map<VacancyDTO>(vacancy);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model State is not valid");
                    return BadRequest(ModelState);
                }

                var cacheOptions = new MemoryCacheEntryOptions().
                    SetSlidingExpiration(TimeSpan.FromSeconds(30)).
                    SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, vacancyDTO, cacheOptions);
                _logger.LogInformation($"Vacancy with Id = {id} is cached now");

            }
            else
            {
                _logger.LogInformation($"Vacancy with Id = {id} is already cached");
            }
      
            return Ok(vacancyDTO);
        }

        [HttpGet("by-title/{title}")]
        public async Task<IActionResult> GetVacancyByTitle(string title)
        {
            _logger.LogInformation($"Get Vacancy With title = {title} that posted by Curred LoggedIn Employer = {int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))}");


            var cacheKey = $"Vacancy_{title}";
            if (!_memoryCache.TryGetValue(cacheKey, out var vacancyDTO))
            {
                if (!await _vacancyRepository.isVacancyExists(title))
                {
                    _logger.LogWarning("Vacancy Is Not Found");
                    return NotFound();
                }

                if (!((await _vacancyRepository.GetVacancyByTitle(title)).EmployerId == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))))
                {
                    _logger.LogWarning("Your Are Not Allowed To See This Vacancy");
                    return Unauthorized();
                }

                _logger.LogInformation($"Vacancy with Title = {title} is not cached");

                var vacancy = await _vacancyRepository.GetVacancyByTitle(title);
                vacancyDTO = _mapper.Map<VacancyDTO>(vacancy);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model State is not valid");
                    return BadRequest(ModelState);
                }

                var cacheOptions = new MemoryCacheEntryOptions().
                    SetSlidingExpiration(TimeSpan.FromSeconds(30)).
                    SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, vacancyDTO, cacheOptions);
                _logger.LogInformation($"Vacancy with Title = {title} is cached now");

            }
            else
            {
                _logger.LogInformation($"Vacancy with Title = {title} is already cached");
            }

            return Ok(vacancyDTO);
        }

        

        
    }
}
