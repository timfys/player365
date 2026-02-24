using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartWinners.Configuration;
using SmartWinners.PaymentSystem.StartAJob;

namespace SmartWinners.Services.Payment.Handlers;

/// <summary>
/// Activates the ladder bonus system for a user.
/// Triggered by /purchase-coins?bonus=welcome - ONLY creates Serial 0 (welcome bonus) if not exists.
/// This is a one-time activation that starts the ladder progression.
/// </summary>
public class WelcomeLadderBonusHandler(
    IBusinessApiService businessApiService,
    IPlayerClub365ApiService playerClub365ApiService,
    IOptions<LadderBonusOptions> ladderBonusOptions,
    ILogger<WelcomeLadderBonusHandler> logger) : IPaymentSuccessHandler
{
    private readonly List<LadderStepOptions> _steps = ladderBonusOptions.Value.Steps;

    public int Order => 10; // Run early

    public bool ShouldHandle(PaymentSuccessContext context)
    {
        // Only for deposits with "welcome" metadata flag
        return context.PaymentType == PaymentWindowType.Deposit 
            && context.Metadata.TryGetValue("bonus", out var bonus) 
            && bonus == "welcome_ladder";
    }

    public async Task HandleAsync(PaymentSuccessContext context, CancellationToken cancellationToken = default)
    {
        // Get existing ladder bonuses for this user
        var bonusesResult = await businessApiService.EntityBonusesGetAsync(new EntityBonusesGet
        {
            Filter = new Dictionary<string, string>
            {
                ["isDeleted"] = "=0",
                ["ParentRecordID"] = $"={context.EntityId}",
                ["CustomField201"] = $"={(int)BonusType.Ladder}",
            },
            LimitFrom = 0,
            LimitCount = 50
        }, null);

        var existingBonuses = bonusesResult.Value?.Data?
            .Where(b => b.Type == BonusType.Ladder && (b.IsDeleted ?? 0) == 0)
            .ToList() ?? [];

        // Check if user already has the welcome bonus (Serial 0)
        var hasWelcomeBonus = existingBonuses.Any(b => 
            b.Serial.HasValue && Convert.ToInt32(Math.Round(b.Serial.Value)) == 0);

        if (hasWelcomeBonus)
        {
            logger.LogInformation("User {EntityId} already has welcome bonus (Serial 0), skipping activation", 
                context.EntityId);
            return;
        }


        var welcomeStep = _steps.FirstOrDefault(s => s.Serial == 0);
        if (welcomeStep == null)
        {
            logger.LogWarning("Welcome step (Serial 0) not found in configuration");
            return;
        }

        // Check if deposit meets minimum amount for welcome bonus
        if (context.Amount < welcomeStep.MinDepositAmount)
        {
            logger.LogInformation("Deposit amount {Amount} is less than minimum {MinAmount} for welcome bonus, skipping",
                context.Amount, welcomeStep.MinDepositAmount);
            return;
        }

        // Calculate bonus value: deposit amount * multiplier
        var bonusValue = context.Amount * welcomeStep.Multiplier;

        // Create welcome bonus (Serial 0)
        var result = await playerClub365ApiService.EntityBonusesUpdateAsync(new EntityBonusesUpdate
        {
            RecordId = 0,
            EntityId = context.EntityId,
            BonusType = BonusType.Ladder,
            Serial = 0,
            Value = bonusValue,
            CreatedDateCustom = DateTime.UtcNow,
            Used = 0,
            IsDeleted = false
        });

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create welcome bonus for EntityId {EntityId}: {Error}",
                context.EntityId, result.Error?.Message);
        }
        else
        {
            logger.LogInformation("Created welcome bonus (Serial 0, value {Value} = {Amount} * {Multiplier}) for EntityId {EntityId} - Ladder activated!", 
                bonusValue, context.Amount, welcomeStep.Multiplier, context.EntityId);
        }
    }
}

/// <summary>
/// Awards ladder deposit bonuses on each deposit.
/// Only works if user has activated the ladder (has welcome bonus Serial 0).
/// Checks the next unused ladder step and awards bonus if deposit meets minimum amount.
/// </summary>
public class DepositBonusHandler(
    IBusinessApiService businessApiService,
    IPlayerClub365ApiService playerClub365ApiService,
    IOptions<LadderBonusOptions> ladderBonusOptions,
    ILogger<DepositBonusHandler> logger) : IPaymentSuccessHandler
{
    private readonly List<LadderStepOptions> _steps = ladderBonusOptions.Value.Steps;

    public int Order => 20;

    public bool ShouldHandle(PaymentSuccessContext context)
    {
        // Only for deposits with "welcome_ladder" metadata flag
        return context.PaymentType == PaymentWindowType.Deposit 
            && context.Metadata.TryGetValue("bonus", out var bonus) 
            && bonus == "ladder";
    }

    public async Task HandleAsync(PaymentSuccessContext context, CancellationToken cancellationToken = default)
    {
        // Get existing ladder bonuses for this user
        var bonusesResult = await businessApiService.EntityBonusesGetAsync(new EntityBonusesGet
        {
            Filter = new Dictionary<string, string>
            {
                ["isDeleted"] = "=0",
                ["ParentRecordID"] = $"={context.EntityId}",
                ["CustomField201"] = $"={(int)BonusType.Ladder}",
            },
            LimitFrom = 0,
            LimitCount = 50
        }, null);

        if (!bonusesResult.IsSuccess)
        {
            logger.LogWarning("Unable to fetch ladder bonuses for EntityId {EntityId}: {Error}", 
                context.EntityId, bonusesResult.Error?.Message);
            return;
        }

        var existingBonuses = bonusesResult.Value?.Data?
            .Where(b => b.Type == BonusType.Ladder && (b.IsDeleted ?? 0) == 0)
            .ToList() ?? [];

        // Check if user has welcome bonus (Serial 0) - ladder must be activated
        var hasWelcomeBonus = existingBonuses.Any(b => 
            b.Serial.HasValue && Convert.ToInt32(Math.Round(b.Serial.Value)) == 0);

        if (!hasWelcomeBonus)
        {
            logger.LogDebug("User {EntityId} has no welcome bonus, ladder not activated - skipping deposit bonus", 
                context.EntityId);
            return;
        }

        // Check if all existing bonuses are fully used (Used == ValueUsd)
        var unusedBonus = existingBonuses.FirstOrDefault(b => 
            b.ValueUsd.HasValue && (!b.Used.HasValue || b.Used.Value != b.ValueUsd.Value));
        
        if (unusedBonus != null)
        {
            logger.LogDebug("User {EntityId} has unused bonus (Serial {Serial}, Used={Used}, ValueUsd={ValueUsd}) - must use before getting next bonus",
                context.EntityId, unusedBonus.Serial, unusedBonus.Used, unusedBonus.ValueUsd);
            return;
        }

        // Get all awarded serials
        var awardedSerials = existingBonuses
            .Where(b => b.Serial.HasValue)
            .Select(b => Convert.ToInt32(Math.Round(b.Serial!.Value)))
            .ToHashSet();

        // Find the next available ladder step (skip Serial 0 as it's the welcome bonus)
        var nextStep = _steps
            .Where(s => s.Serial > 0) // Skip welcome bonus
            .FirstOrDefault(s => !awardedSerials.Contains(s.Serial));

        if (nextStep == null)
        {
            logger.LogInformation("User {EntityId} has completed all ladder steps", context.EntityId);
            return;
        }

        // Check if deposit meets minimum amount for this step
        if (context.Amount < nextStep.MinDepositAmount)
        {
            logger.LogDebug("Deposit amount {Amount} is less than minimum {MinAmount} for {Step}, no bonus awarded",
                context.Amount, nextStep.MinDepositAmount, nextStep.StepText);
            return;
        }

        // Calculate bonus value: deposit amount * multiplier
        var bonusValue = context.Amount * nextStep.Multiplier;

        // Award the bonus for this ladder step
        logger.LogInformation("Awarding {Step} ({Percentage}) bonus of {BonusValue} (= {Amount} * {Multiplier}) to EntityId {EntityId}",
            nextStep.StepText, nextStep.BonusPercentageText, bonusValue, context.Amount, nextStep.Multiplier, context.EntityId);

        var result = await playerClub365ApiService.EntityBonusesUpdateAsync(new EntityBonusesUpdate
        {
            RecordId = 0,
            EntityId = context.EntityId,
            BonusType = BonusType.Ladder,
            Serial = nextStep.Serial,
            Value = bonusValue,
            CreatedDateCustom = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddHours(24),
            Used = 0,
            IsDeleted = false
        });

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create ladder bonus ({Step}) for EntityId {EntityId}: {Error}",
                nextStep.StepText, context.EntityId, result.Error?.Message);
        }
        else
        {
            logger.LogInformation("Created ladder bonus {Step} (Serial {Serial}, value {Value}) for EntityId {EntityId}", 
                nextStep.StepText, nextStep.Serial, bonusValue, context.EntityId);
        }
    }
}

/// <summary>
/// Simple welcome bonus handler - awards 150% of deposit amount.
/// Triggered by /purchase-coins?bonus=welcome - one-time bonus for new users.
/// </summary>
public class WelcomeBonusHandler(
    IBusinessApiService businessApiService,
    IPlayerClub365ApiService playerClub365ApiService,
    ILogger<WelcomeBonusHandler> logger) : IPaymentSuccessHandler
{
    private const decimal WelcomeMultiplier = 1.5m;
    private const int WelcomeBonusSerial = 100; // Distinct serial for simple welcome bonus

    public int Order => 5; // Run very early

    public bool ShouldHandle(PaymentSuccessContext context)
    {
        // Only for deposits with "welcome" metadata flag
        return context.PaymentType == PaymentWindowType.Deposit 
            && context.Metadata.TryGetValue("bonus", out var bonus) 
            && bonus == "welcome";
    }

    public async Task HandleAsync(PaymentSuccessContext context, CancellationToken cancellationToken = default)
    {
        // Check if user already has the welcome bonus (Serial 100)
        var bonusesResult = await businessApiService.EntityBonusesGetAsync(new EntityBonusesGet
        {
            Filter = new Dictionary<string, string>
            {
                ["isDeleted"] = "=0",
                ["ParentRecordID"] = $"={context.EntityId}",
                ["CustomField201"] = $"={(int)BonusType.WellocomeOffer}",
                ["CustomField202"] = $"={WelcomeBonusSerial}",
            },
            LimitFrom = 0,
            LimitCount = 1
        }, null);

        var existingWelcomeBonus = bonusesResult.Value?.Data?
            .FirstOrDefault(b => b.Type == BonusType.WellocomeOffer && (b.IsDeleted ?? 0) == 0);

        if (existingWelcomeBonus != null)
        {
            logger.LogInformation("User {EntityId} already has welcome bonus (Serial {Serial}), skipping", 
                context.EntityId, WelcomeBonusSerial);
            return;
        }

        // Calculate bonus value: deposit amount * 1.5
        var bonusValue = context.Amount * WelcomeMultiplier;

        // Create welcome bonus
        var result = await playerClub365ApiService.EntityBonusesUpdateAsync(new EntityBonusesUpdate
        {
            RecordId = 0,
            EntityId = context.EntityId,
            BonusType = BonusType.WellocomeOffer,
            Serial = WelcomeBonusSerial,
            Value = bonusValue,
            CreatedDateCustom = DateTime.UtcNow,
            Used = 0,
            IsDeleted = false
        });

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create welcome bonus for EntityId {EntityId}: {Error}",
                context.EntityId, result.Error?.Message);
        }
        else
        {
            logger.LogInformation("Created welcome bonus (value {Value} = {Amount} * {Multiplier}) for EntityId {EntityId}", 
                bonusValue, context.Amount, WelcomeMultiplier, context.EntityId);
        }
    }
}
