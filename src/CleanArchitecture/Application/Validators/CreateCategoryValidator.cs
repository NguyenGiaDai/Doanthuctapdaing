using CleanArchitecture.Application.DTO;
using FluentValidation;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDTO>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(200);
    }
}
