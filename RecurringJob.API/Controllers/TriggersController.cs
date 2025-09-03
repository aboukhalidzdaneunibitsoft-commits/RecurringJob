using Microsoft.AspNetCore.Mvc;
using RecurringJob.API.DTOs;
using RecurringJob.API.Entitys;
using RecurringJob.API.IServices;

namespace RecurringJob.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TriggersController : ControllerBase
{
    private readonly ITimeTriggerService _triggerService;

    public TriggersController(ITimeTriggerService triggerService)
    {
        _triggerService = triggerService;
    }

    [HttpPost]
    public async Task<ActionResult<TimeTrigger>> CreateTrigger([FromBody] TriggerAddDTO request)
    {
        var trigger = await _triggerService.CreateTriggerAsync(
            request.Name,
            request.TimeExpression,
            request.MethodName,
            request.TriggerType,
            request.Parameters
        );

        return CreatedAtAction(nameof(GetTrigger), new { id = trigger.Id }, trigger);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TimeTrigger>> GetTrigger(int id)
    {
        var trigger = await _triggerService.GetTriggerAsync(id);
        return trigger == null ? NotFound() : Ok(trigger);
    }

    [HttpGet]
    public async Task<ActionResult<List<TimeTrigger>>> GetTriggers()
    {
        var triggers = await _triggerService.GetTriggersAsync();
        return Ok(triggers);
    }

    [HttpGet("{id}/logs")]
    public async Task<ActionResult<List<TriggerExecutionLog>>> GetExecutionLogs(int id)
    {
        var logs = await _triggerService.GetExecutionLogsAsync(id);
      return Ok(logs);
    }   
}