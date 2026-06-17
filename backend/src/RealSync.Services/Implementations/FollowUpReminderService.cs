using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Core.Models;
using RealSync.Data.Context;
using RealSync.Services.Options;

namespace RealSync.Services.Implementations;

public class FollowUpReminderService : IFollowUpReminderService
{
    private readonly RealSyncDbContext _context;
    private readonly FollowUpReminderOptions _options;
    private readonly ILogger<FollowUpReminderService> _logger;

    public FollowUpReminderService(
        RealSyncDbContext context,
        IOptions<FollowUpReminderOptions> options,
        ILogger<FollowUpReminderService> logger)
    {
        _context = context;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FollowUpReminderProcessResult> ProcessDueRemindersAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var result = new FollowUpReminderProcessResult();

        var dueLeads = await _context.Leads
            .AsNoTracking()
            .Where(l =>
                l.NextFollowUpAt != null &&
                l.NextFollowUpAt <= now &&
                l.AssignedToId != null &&
                l.Status != "Won" &&
                l.Status != "Lost")
            .OrderBy(l => l.NextFollowUpAt)
            .Take(_options.BatchSize)
            .Select(l => new DueFollowUpLead(
                l.Id,
                l.FullName,
                l.AssignedToId!.Value,
                l.NextFollowUpAt!.Value))
            .ToListAsync(cancellationToken);

        result.Scanned = dueLeads.Count;

        foreach (var lead in dueLeads)
        {
            try
            {
                var sent = await TryDispatchReminderAsync(lead, cancellationToken);
                if (sent)
                    result.Sent++;
                else
                    result.Skipped++;
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                DetachAddedEntities();
                result.Skipped++;
                _logger.LogDebug(ex, "Follow-up reminder dispatch already exists for lead {LeadId} at {ScheduledFor}", lead.Id, lead.ScheduledFor);
            }
            catch (Exception ex)
            {
                DetachAddedEntities();
                result.Failed++;
                _logger.LogError(ex, "Failed to dispatch follow-up reminder for lead {LeadId}", lead.Id);
            }
        }

        return result;
    }

    private async Task<bool> TryDispatchReminderAsync(DueFollowUpLead lead, CancellationToken cancellationToken)
    {
        var exists = await _context.FollowUpReminderDispatches
            .AnyAsync(d => d.LeadId == lead.Id && d.ScheduledFor == lead.ScheduledFor, cancellationToken);

        if (exists)
            return false;

        var now = DateTime.UtcNow;
        var notification = new Notification
        {
            UserId = lead.AssignedToId,
            Title = "Đến giờ chăm sóc lead",
            Message = $"Đã đến lịch chăm sóc lead {lead.FullName}.",
            Type = NotificationType.Lead,
            Link = $"/leads/{lead.Id}",
            Data = JsonSerializer.Serialize(new
            {
                eventType = "FollowUpDue",
                leadId = lead.Id,
                scheduledFor = lead.ScheduledFor,
                assignedToId = lead.AssignedToId
            })
        };

        var dispatch = new FollowUpReminderDispatch
        {
            LeadId = lead.Id,
            ScheduledFor = lead.ScheduledFor,
            NotificationId = notification.Id,
            SentAt = now
        };

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _context.Notifications.Add(notification);
            _context.FollowUpReminderDispatches.Add(dispatch);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        return true;
    }

    private void DetachAddedEntities()
    {
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
            entry.State = EntityState.Detached;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var baseException = exception.GetBaseException();
        var message = baseException.Message;

        return message.Contains("2601", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("2627", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record DueFollowUpLead(
        Guid Id,
        string FullName,
        Guid AssignedToId,
        DateTime ScheduledFor);
}
