using Current.Api.DTOs.Users;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

// HTTP layer only — receives requests, delegates to IUserService, returns responses
[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    // IUserService is injected by DI — never call "new UserService()" here
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user); // 201 + Location header
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message }); // 409 — duplicate email
        }
    }
}
