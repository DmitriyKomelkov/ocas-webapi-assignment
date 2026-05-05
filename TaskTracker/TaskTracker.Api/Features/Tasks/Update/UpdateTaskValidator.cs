using FastEndpoints;
using FluentValidation;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Api.Features.Tasks.Update;

internal sealed class UpdateTaskValidator : Validator<UpdateTaskRequest>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(TaskItem.MaxTitleLength)
                .WithMessage($"Title must be {TaskItem.MaxTitleLength} characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(TaskItem.MaxDescriptionLength)
                .WithMessage($"Description must be {TaskItem.MaxDescriptionLength} characters or fewer.");

        RuleFor(x => x.Status).IsInEnum();
    }
}
