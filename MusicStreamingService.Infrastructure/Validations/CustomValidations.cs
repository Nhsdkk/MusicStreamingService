using FluentValidation;

namespace MusicStreamingService.Infrastructure.Validations;

public static class CustomValidations
{
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .MinimumLength(10).WithMessage("Password must be at least 10 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

    public static IRuleBuilderOptions<T, DateTime> Before<T>(
        this IRuleBuilder<T, DateTime> ruleBuilder,
        DateTime other) =>
        ruleBuilder
            .Must(date => date < other)
            .WithMessage($"Date must be before {other:yyyy-MM-dd}.");
}