using Current.Api.DTOs.Users;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

[Authorize]
[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var user = await _userService.GetUserByIdAsync(currentUserId, currentUserId);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut("me/profile")]
    public async Task<ActionResult<UserResponse>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var user = await _userService.UpdateProfileAsync(currentUserId, request);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("me/preferences")]
    public async Task<ActionResult<UserResponse>> UpdatePreferences(
        [FromBody] UpdateUserPreferencesRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var user = await _userService.UpdatePreferencesAsync(currentUserId, request);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var user = await _userService.GetUserByIdAsync(id, currentUserId);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
