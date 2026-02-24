using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartWinners.Models;
using SmartWinners.Services;

namespace SmartWinners.Controllers;

[Route("WebHook")]
[ApiController]
public class WebHookController(PlisioService plisioService, ILogger<WebHookController> logger) : ControllerBase
{
    [HttpPost("Plisio")]
    public async Task<IActionResult> ProcessPlisioWebhook()
    {
        string json;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            json = await reader.ReadToEndAsync();
        }

        logger.LogInformation("Received Plisio webhook: {Json}", json);

        PlisioWebhookRequest? model;
        try
        {
            model = JsonConvert.DeserializeObject<PlisioWebhookRequest>(json);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize Plisio webhook payload");
            return BadRequest("Invalid payload format");
        }

        if (model is null)
        {
            logger.LogWarning("Plisio webhook model is null");
            return BadRequest("Invalid payload");
        }

        // Only process completed or mismatch (overpaid) transactions
        if (model.status.ToLower() is not ("completed" or "mismatch"))
        {
            logger.LogInformation("Ignoring Plisio webhook with status: {Status}", model.status);
            return Ok("Status ignored");
        }

        // Verify the webhook signature
        var data = JObject.Parse(json);
        data.Property("verify_hash")?.Remove();
        var jsonWithoutHash = data.ToString();

        if (!plisioService.VerifyWebhookSignature(jsonWithoutHash, model.verify_hash))
        {
            logger.LogWarning("Invalid Plisio webhook signature for TxnId: {TxnId}", model.txn_id);
            return Unauthorized("Invalid signature");
        }

        // Process the payment
        var result = await plisioService.ProcessWebhookPaymentAsync(model);

        if (result.AlreadyProcessed)
        {
            logger.LogInformation("Transaction {TxnId} was already processed", model.txn_id);
            return Ok("Already processed");
        }

        if (!result.Success)
        {
            logger.LogError("Failed to process Plisio payment: {Error}", result.ErrorMessage);
            return StatusCode(500, result.ErrorMessage);
        }

        logger.LogInformation("Successfully processed Plisio webhook for TxnId: {TxnId}, EntityId: {EntityId}",
            model.txn_id, result.EntityId);

        return Ok();
    }
}