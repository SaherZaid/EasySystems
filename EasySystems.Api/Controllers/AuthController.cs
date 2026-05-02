using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EasySystems.Application.Dtos;
using EasySystems.Domain.Entities;
using EasySystems.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EasySystems.Api.Services;

namespace EasySystems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;

    public AuthController(
    AppDbContext dbContext,
    IConfiguration configuration,
    EmailService emailService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _emailService = emailService;
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _dbContext.UserAccounts
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                FullName = x.FirstName + " " + x.LastName,
                x.Email,
                x.Role,
                x.IsEmailVerified
            })
            .ToListAsync();

        return Ok(users);
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        var emailExists = await _dbContext.UserAccounts
            .AnyAsync(x => x.Email == request.Email);

        if (emailExists)
            return BadRequest("Email already exists.");

        var user = new UserAccount
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Role = "User",
            IsEmailVerified = false
        };

        _dbContext.UserAccounts.Add(user);
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "User registered successfully.",
            userId = user.Id
        });
    }

    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode([FromBody] string email)
    {
        var user = await _dbContext.UserAccounts
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user is null)
        {
            await Task.Delay(800);

            return Ok(new
            {
                message = "If this email exists, a verification code has been sent."
            });
        }

        var recentCode = await _dbContext.EmailVerificationCodes
    .Where(x =>
        x.UserAccountId == user.Id &&
        x.CreatedAtUtc > DateTime.UtcNow.AddSeconds(-30))
    .OrderByDescending(x => x.CreatedAtUtc)
    .FirstOrDefaultAsync();

        if (recentCode is not null)
        {
            return BadRequest("Please wait 30 seconds before requesting another code.");
        }



        var code = new Random().Next(100000, 999999).ToString();

        var verification = new EmailVerificationCode
        {
            UserAccountId = user.Id,
            Code = code,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1),
            IsUsed = false
        };

        _dbContext.EmailVerificationCodes.Add(verification);
        await _dbContext.SaveChangesAsync();
        await _emailService.SendVerificationCode(email, code);

        return Ok(new
        {
            message = "If this email exists, a verification code has been sent."
        });
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode(VerifyCodeRequest request)
    {
        var user = await _dbContext.UserAccounts
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user is null)
            return NotFound("User not found.");

        var verification = await _dbContext.EmailVerificationCodes
            .Where(x =>
                x.UserAccountId == user.Id &&
                x.Code == request.Code &&
                !x.IsUsed)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (verification is null)
        {
            var failedAttempts = await _dbContext.EmailVerificationCodes
                .CountAsync(x =>
                    x.UserAccountId == user.Id &&
                    x.IsUsed == false &&
                    x.CreatedAtUtc > DateTime.UtcNow.AddMinutes(-10));

            if (failedAttempts >= 5)
            {
                return BadRequest("Too many failed attempts. Try again in 10 minutes.");
            }

            return BadRequest("Invalid verification code.");
        }

        if (verification.ExpiresAtUtc < DateTime.UtcNow)
            return BadRequest("Verification code has expired.");

        verification.IsUsed = true;
        user.IsEmailVerified = true;

        await _dbContext.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            message = "Email verified successfully.",
            userId = user.Id,
            email = user.Email,
            role = user.Role,
            token,
            claims = new[]
    {
        $"Role: {user.Role}",
        $"UserId: {user.Id}",
        $"Email: {user.Email}"
    }
        });


    }

    [HttpPost("assign-task")]
[Authorize(Roles = "Admin,SuperAdmin")]
public async Task<IActionResult> AssignTask(AssignTaskRequest request)
{
    await _emailService.SendCustomEmail(
        request.Email,
        "🔥 New EasySystems Task Assigned",
        $@"
<h2>New Project Assigned To You</h2>

<p><strong>Project:</strong> {request.Project}</p>
<p><strong>Customer:</strong> {request.Customer}</p>

<p>Please login to EasySystems CRM and handle this request.</p>");

    return Ok();
}

public class AssignTaskRequest
{
    public string Email { get; set; } = "";
    public string Project { get; set; } = "";
    public string Customer { get; set; } = "";
}


    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("update-role")]
    public async Task<IActionResult> UpdateRole(UpdateUserRoleRequest request)
    {
        var user = await _dbContext.UserAccounts
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user is null)
            return NotFound("User not found.");

        user.Role = request.Role;

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "User role updated successfully.",
            email = user.Email,
            role = user.Role
        });
    }

    private string GenerateJwtToken(UserAccount user)
    {
        var jwtKey = _configuration["Jwt:Key"]!;
        var jwtIssuer = _configuration["Jwt:Issuer"]!;
        var jwtAudience = _configuration["Jwt:Audience"]!;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}