using System.Security.Claims;
using EasySystems.Application.Dtos;
using EasySystems.Domain.Entities;
using EasySystems.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasySystems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoreRequestsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public StoreRequestsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStoreRequestDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var storeRequest = new StoreRequest
        {
            UserAccountId = userId,
            StoreName = request.StoreName,
            BusinessType = request.BusinessType,
            PackageName = request.PackageName,
            Notes = request.Notes,
            Status = "Pending"
        };

        _dbContext.StoreRequests.Add(storeRequest);
        await _dbContext.SaveChangesAsync();

        foreach (var answer in request.Answers)
        {
            _dbContext.StoreQuestionAnswers.Add(new StoreQuestionAnswer
            {
                StoreRequestId = storeRequest.Id,
                Question = answer.Question,
                Answer = answer.Answer
            });
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Store request created successfully.",
            storeRequestId = storeRequest.Id
        });
    }


    [Authorize(Roles = "SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var requests = await _dbContext.StoreRequests
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.UserAccountId,
                x.StoreName,
                x.BusinessType,
                x.PackageName,
                x.Notes,
                x.Status,
                x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(requests);
    }
}