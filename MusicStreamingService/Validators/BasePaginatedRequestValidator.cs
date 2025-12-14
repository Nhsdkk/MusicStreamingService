using FluentValidation;
using MusicStreamingService.Requests;

namespace MusicStreamingService.Validators;

public class BasePaginatedRequestValidator<T> : AbstractValidator<T>
    where T : BasePaginatedRequest
{
    protected BasePaginatedRequestValidator()
    {
        RuleFor(x => x.ItemsPerPage)
            .GreaterThan(0)
            .LessThan(100);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0);
    }
}