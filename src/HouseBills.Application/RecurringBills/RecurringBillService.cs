using HouseBills.Application.Bills;
using HouseBills.Application.Common;
using HouseBills.Application.Persistence;
using HouseBills.Domain;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HouseBills.Application.RecurringBills;

internal sealed class RecurringBillService(
    IRecurringBillRepository repository,
    IPayeeRepository payees,
    ICategoryRepository categories,
    IClock clock,
    IOptions<BillingOptions> options,
    ILogger<RecurringBillService> logger) : IRecurringBillService
{
    public async Task<IReadOnlyList<RecurringBillDto>> ListAsync(CancellationToken cancellationToken)
    {
        var today = clock.Today;
        var items = await repository.ListAsync(cancellationToken);
        return items
            .Select(i => i with
            {
                NextDueDate = i.IsActive
                    ? new RecurrenceSchedule(i.Frequency, i.StartDate, i.EndDate).NextDueDateOnOrAfter(today)
                    : null,
            })
            .ToList();
    }

    public async Task<Result<int>> SaveAsync(SaveRecurringBillRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        FieldValidation.Text(errors, request.Name, RecurringBill.NameMaxLength, "Name", required: true);
        FieldValidation.Text(errors, request.Notes, RecurringBill.NotesMaxLength, "Notes", required: false);
        FieldValidation.Amount(errors, request.Amount, "Amount");
        if (!Enum.IsDefined(request.Frequency))
        {
            errors.Add("Select a frequency.");
        }

        if (request.EndDate < request.StartDate)
        {
            errors.Add("End date must not be before the first due date.");
        }

        await BillReferenceValidation.ValidateAsync(errors, request.PayeeId, request.CategoryId, payees, categories, cancellationToken);
        if (FieldValidation.ToError(errors) is { } validationError)
        {
            return validationError;
        }

        var schedule = new RecurrenceSchedule(request.Frequency, request.StartDate, request.EndDate);
        if (request.Id is not { } id)
        {
            var template = new RecurringBill(request.Name, request.PayeeId, request.CategoryId, request.Amount, schedule, request.Notes);
            await repository.AddAsync(template, cancellationToken);
            return template.Id;
        }

        var existing = await repository.GetAsync(id, cancellationToken);
        if (existing is null)
        {
            return Error.NotFound("The recurring bill no longer exists.");
        }

        existing.Update(request.Name, request.PayeeId, request.CategoryId, request.Amount, schedule, request.Notes);
        var result = await repository.TryUpdateAsync(existing, request.RowVersion, cancellationToken);
        return result.IsSuccess ? id : result.Error!;
    }

    public async Task<Result> SetActiveAsync(int id, bool isActive, byte[] rowVersion, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAsync(id, cancellationToken);
        if (existing is null)
        {
            return Error.NotFound("The recurring bill no longer exists.");
        }

        existing.SetActive(isActive, clock.Today);
        return await repository.TryUpdateAsync(existing, rowVersion, cancellationToken);
    }

    public Task<Result> DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken)
    {
        return repository.TryDeleteAsync(id, rowVersion, cancellationToken);
    }

    public async Task<int> GenerateUpcomingBillsAsync(CancellationToken cancellationToken)
    {
        var upTo = clock.Today.AddDays(options.Value.GenerationLookaheadDays);
        var templates = await repository.ListActiveAsync(cancellationToken);
        var created = 0;
        foreach (var template in templates)
        {
            var expectedRowVersion = template.RowVersion;
            var bills = template.GenerateBills(upTo);
            if (bills.Count == 0)
            {
                continue;
            }

            try
            {
                await repository.SaveGeneratedBillsAsync(template, expectedRowVersion, bills, cancellationToken);
                created += bills.Count;
            }
            catch (ConcurrencyConflictException)
            {
                logger.LogInformation("Recurring bill {RecurringBillId} was changed concurrently; skipped bill generation.", template.Id);
            }
        }

        if (created > 0)
        {
            logger.LogInformation("Generated {BillCount} bills up to {UpTo}.", created, upTo);
        }

        return created;
    }
}