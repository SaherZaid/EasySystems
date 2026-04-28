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

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var requests = await (
            from storeRequest in _dbContext.StoreRequests
            join user in _dbContext.UserAccounts
                on storeRequest.UserAccountId equals user.Id
            orderby storeRequest.CreatedAtUtc descending
            select new
            {
                storeRequest.Id,
                storeRequest.UserAccountId,
                CustomerFullName = user.FirstName + " " + user.LastName,
                CustomerEmail = user.Email,
                CustomerPhoneNumber = user.PhoneNumber,
                storeRequest.StoreName,
                storeRequest.BusinessType,
                storeRequest.PackageName,
                storeRequest.Notes,
                storeRequest.Status,
                storeRequest.CreatedAtUtc,
                Answers = _dbContext.StoreQuestionAnswers
                    .Where(a => a.StoreRequestId == storeRequest.Id)
                    .Select(a => new
                    {
                        a.Question,
                        a.Answer
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(requests);
    }


    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateStoreRequestStatusDto request)
    {
        var allowedStatuses = new[]
        {
            "Pending",
            "In Review",
            "Approved",
            "In Progress",
            "Completed",
            "Rejected"
        };

        if (!allowedStatuses.Contains(request.Status))
            return BadRequest("Invalid status.");

        var storeRequest = await _dbContext.StoreRequests
            .FirstOrDefaultAsync(x => x.Id == id);

        if (storeRequest is null)
            return NotFound("Store request not found.");

        storeRequest.Status = request.Status;

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Status updated successfully.",
            storeRequest.Id,
            storeRequest.Status
        });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var requests = await _dbContext.StoreRequests
            .Where(x => x.UserAccountId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.StoreName,
                x.BusinessType,
                x.PackageName,
                x.Status,
                x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(requests);
    }
}