using Current.Api.Common.Exceptions;
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
    public async Task<ActionResult<PaymentReceiptResponse>> Send(
        [FromBody] SendPaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var receipt = await _paymentService.SendPaymentAsync(
                request,
                currentUserId,
                idempotencyKey ?? string.Empty);
            return Ok(receipt);
        }
        catch (PaymentException ex)
        {
            return BadRequest(new PaymentErrorResponse
            {
                Code = ex.Code,
                Message = ex.Message
            });
        }
    }

    [HttpGet("sent")]
    public async Task<ActionResult<IReadOnlyList<PaymentHistoryItemResponse>>> GetSent()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var payments = await _paymentService.GetSentPaymentsAsync(currentUserId);
        return Ok(payments);
    }

    [HttpGet("received")]
    public async Task<ActionResult<IReadOnlyList<PaymentHistoryItemResponse>>> GetReceived()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var payments = await _paymentService.GetReceivedPaymentsAsync(currentUserId);
        return Ok(payments);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<PaymentHistoryItemResponse>>> GetHistory()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var payments = await _paymentService.GetPaymentHistoryAsync(currentUserId);
        return Ok(payments);
    }

    [HttpGet("{transactionId:guid}")]
    public async Task<ActionResult<PaymentHistoryItemResponse>> GetReceipt(Guid transactionId)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var receipt = await _paymentService.GetPaymentReceiptAsync(transactionId, currentUserId);

        if (receipt is null)
        {
            return NotFound();
        }

        return Ok(receipt);
    }
}
