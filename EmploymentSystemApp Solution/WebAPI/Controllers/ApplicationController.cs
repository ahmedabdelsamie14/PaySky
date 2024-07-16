using AutoMapper;
using ApplicationLayer.DTO;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using ApplicationLayer.Helper;
using Microsoft.Extensions.Caching.Memory;
using static System.Net.Mime.MediaTypeNames;

namespace EmploymentSystemApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ApplicationController> _logger;
        private readonly IMemoryCache _memoryCache;

        public ApplicationController(IApplicationRepository applicationRepository , IMapper mapper , ILogger<ApplicationController> logger , IMemoryCache memoryCache) 
        {
            _applicationRepository = applicationRepository;
            _mapper = mapper;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplications()
        {
            _logger.LogInformation("Get All Applications In The System");
            var cacheKey = "GetAllApplications";

            if (_memoryCache.TryGetValue(cacheKey, out List<ApplicationDTO> applicationsDTO))
            {
                _logger.LogInformation("Applications Are In Memory Cache");
            }
            else
            {
                var applications = await _applicationRepository.GetAllApplications();
                applicationsDTO = _mapper.Map<List<ApplicationDTO>>(applications);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, applicationsDTO, cacheOptions);

                _logger.LogInformation("Applications Are Cached Now");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model State Is Not Valid");
                return BadRequest(ModelState);
            }

            return Ok(applicationsDTO);
        }

        [HttpGet("{applicationId}")]
        public async Task<IActionResult> GetApplicationById(int applicationId)
        {
            _logger.LogInformation($"Get Application With Id = {applicationId}");
            var cacheKey = $"application_{applicationId}";

            if (!_memoryCache.TryGetValue(cacheKey, out ApplicationDTO applicationDTO))
            {
                _logger.LogInformation($"Application With Id = {applicationId} is not in the cache");

                if (!await _applicationRepository.isApplicationExists(applicationId))
                {
                    _logger.LogWarning($"Application With Id = {applicationId} is not found");
                    return NotFound();
                }

                var application = await _applicationRepository.GetApplicationById(applicationId);
                applicationDTO = _mapper.Map<ApplicationDTO>(application);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, applicationDTO, cacheEntryOptions);

                _logger.LogInformation($"Application With Id = {applicationId} is cached now");
            }
            else
            {
                _logger.LogInformation($"Application With Id = {applicationId} is already in cache");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model State is not valid");
                return BadRequest(ModelState);
            }

            return Ok(applicationDTO);
        }

        [HttpGet("by-applicantName/{applicantName}")]
        public async Task<IActionResult> GetApplicationsByApplicantName(string applicantName)
        {
            _logger.LogInformation($"Get Applications for Applicant With Name = {applicantName}");
            var cacheKey = $"applications_applicant_{applicantName}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<ApplicationDTO> applicationsDTO))
            {
                _logger.LogInformation($"Applications for Applicant With Name = {applicantName} are not in the cache");

                if (!await _applicationRepository.isApplicationExists(applicantName))
                {
                    _logger.LogWarning($"Applicant With Name = {applicantName} is not found");
                    return NotFound();
                }

                var applications = await _applicationRepository.GetApplicationsByApplicantName(applicantName);
                applicationsDTO = _mapper.Map<List<ApplicationDTO>>(applications);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, applicationsDTO, cacheEntryOptions);

                _logger.LogInformation($"Applications for Applicant With Name = {applicantName} are cached now");
            }
            else
            {
                _logger.LogInformation($"Applications for Applicant With Name = {applicantName} are already in cache");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model State is not valid");
                return BadRequest(ModelState);
            }

            return Ok(applicationsDTO);
        }

        [HttpGet("by-applictionData/{applicationDate}")]
        public async Task<IActionResult> GetApplicationByDate(DateTime applicationDate)
        {
            _logger.LogInformation($"Get Application With Date = {applicationDate}");

            var cacheKey = $"application_date_{applicationDate:yyyyMMdd}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<ApplicationDTO> applicationDTO))
            {
                _logger.LogInformation($"Application With Date = {applicationDate} is not in the cache");

                if (!await _applicationRepository.isApplicationExists(applicationDate))
                {
                    _logger.LogWarning($"Application With Date = {applicationDate} is not found");
                    return NotFound();
                }

                var applications = await _applicationRepository.GetApplicationsByApplicationDate(applicationDate);
                applicationDTO = _mapper.Map<List<ApplicationDTO>>(applications);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(cacheKey, applicationDTO, cacheEntryOptions);

                _logger.LogInformation($"Application With Date = {applicationDate} is cached now");
            }
            else
            {
                _logger.LogInformation($"Application With Date = {applicationDate} is already in cache");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model State is not valid");
                return BadRequest(ModelState);
            }

            return Ok(applicationDTO);
        }
    }
}
