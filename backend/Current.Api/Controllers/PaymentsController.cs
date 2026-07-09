using Current.Api.DTOs.Payments;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

[Authorize]
[ApiController]
[Route("payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUserService;

    public PaymentsController(
        IPaymentService paymentService,
        ICurrentUserService currentUserService)
    {
        _paymentService = paymentService;
        _currentUserService = currentUserService;
    }

    [HttpPost("send")]
    public async Task<ActionResult<PaymentReceiptResponse>> Send([FromBody] SendPaymentRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var receipt = await _paymentService.SendPaymentAsync(request, currentUserId);
            return Ok(receipt);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
