namespace EmploymentSystemApp.Controllers
{
    using AutoMapper;
    using Infrastructure.Data;
    using ApplicationLayer.DTO;
    using Core.Interfaces;
    using Core.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using ApplicationLayer.Helper;

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly EmploymentSystemDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;

        public UserController(EmploymentSystemDbContext context, IMapper mapper , IConfiguration configuration , ILogger<UserController> logger)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register/applicant")]
        public IActionResult RegisterApplicant([FromBody] RegisterApplicantDTO applicantDTO)
        {
            _logger.LogInformation("Register New Applicant");

            if (applicantDTO == null)
            {
                _logger.LogWarning("Applicant DTO is null");
                return BadRequest(ModelState);
            }
                

            if (_context.Applicants.Any(a => a.Username == applicantDTO.Username))
            {
                _logger.LogWarning($"Applicant with name = {applicantDTO.Username} is already exist");
                ModelState.AddModelError("UserExists", "Username already exists");
                return StatusCode(422, ModelState);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(applicantDTO.Password);
            applicantDTO.Password = hashedPassword;

            var applicant = _mapper.Map<Applicant>(applicantDTO);
            applicant.Role = "Applicant";

            _context.Applicants.Add(applicant);
            _context.SaveChanges();

            _logger.LogInformation("Applicant Registered");
            return Ok("Successfully Registered");
        }

        [HttpPost("register/employer")]
        public IActionResult RegisterEmployer([FromBody] RegisterEmployerDTO employerDTO)
        {
            _logger.LogInformation("Register New Employer");

            if (employerDTO == null)
            {
                _logger.LogWarning("Applicant DTO is null");
                return BadRequest(ModelState);
            }
                

            if (_context.Employers.Any(e => e.Username == employerDTO.Username))
            {
                _logger.LogWarning($"Applicant with name = {employerDTO.Username} is already exist");
                ModelState.AddModelError("UserExists", "Username already exists");
                return StatusCode(422, ModelState);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(employerDTO.Password);
            employerDTO.Password = hashedPassword;

            var employer = _mapper.Map<Employer>(employerDTO);
            employer.Role = "Employer";

            _context.Employers.Add(employer);
            _context.SaveChanges();

            _logger.LogInformation("Employer Registered");
            return Ok("Successfully Registered");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO loginDTO)
        {

            _logger.LogInformation("Login Process");

            var applicant = _context.Applicants.SingleOrDefault(a => a.Username == loginDTO.Username);
            var employer = _context.Employers.SingleOrDefault(e => e.Username == loginDTO.Username);

            
            if (applicant != null && BCrypt.Net.BCrypt.Verify(loginDTO.Password, applicant.Password))
            {
                var token = CreateToken(applicant);
                HttpContext.Session.SetString("jwtToken", token);
                _logger.LogInformation("Logged In");
                return Ok(new { token });
            }
            else if (employer != null && BCrypt.Net.BCrypt.Verify(loginDTO.Password, employer.Password))
            {
                var token = CreateToken(employer);
                HttpContext.Session.SetString("jwtToken", token);
                _logger.LogInformation("Logged In");
                return Ok(new { token });
            }
            else if (employer == null && applicant == null)
            {
                return NotFound();
            } 
            else
            {
                _logger.LogWarning("Password is wrong");
                ModelState.AddModelError("Password Issue", "Password is wrong");
                return StatusCode(422, ModelState);
            }
        }

        private string CreateToken(User user)
        {
            _logger.LogInformation("Create JWT Token");

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name , user.Username),
                new Claim(ClaimTypes.NameIdentifier , user.UserId.ToString()),
                new Claim(ClaimTypes.Role , user.Role)
            };
            _logger.LogInformation($"{claims.Count} claims");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _configuration.GetSection("JwtSettings:JwtKey").Value!));

            var cred = new SigningCredentials(key , SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: cred
                    );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Token Made");
            return jwt;

        }
    }

}
