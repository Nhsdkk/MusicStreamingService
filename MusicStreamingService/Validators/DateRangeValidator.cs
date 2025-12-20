using FluentValidation;
using MusicStreamingService.Data.Utils;

namespace MusicStreamingService.Validators;

public class DateRangeValidator : AbstractValidator<DateRange>
{
    public DateRangeValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Start <= x.End)
            .When(x => x.Start is not null && x.End is not null)
            .WithMessage("Start date must be less than or equal to end date.");

        RuleFor(x => x.Start)
            .NotEmpty()
            .When(x => x.Start is not null);
        
        RuleFor(x => x.End)
            .NotEmpty()
            .When(x => x.End is not null);
    }
}