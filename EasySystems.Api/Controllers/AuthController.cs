using EasySystems.Application.Dtos;
using EasySystems.Domain.Entities;
using EasySystems.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasySystems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AuthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        var emailExists = await _dbContext.UserAccounts
            .AnyAsync(x => x.Email == request.Email);

        if (emailExists)
        {
            return BadRequest("Email already exists.");
        }

        var user = new UserAccount
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
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
}