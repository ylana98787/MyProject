using FluentValidation;

namespace Api.Contracts;

public sealed class SaveBookRequestValidator : AbstractValidator<SaveBookRequest>
{
    public SaveBookRequestValidator()
    {
        RuleFor(x => x.Isbn)
            .NotEmpty().WithMessage("ISBN обов'язковий.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Назва обов'язкова.")
            .MaximumLength(200).WithMessage("Назва має бути до 200 символів.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1450, DateTime.UtcNow.Year)
            .WithMessage($"Рік має бути в межах від 1450 до {DateTime.UtcNow.Year}.");
    }
}
