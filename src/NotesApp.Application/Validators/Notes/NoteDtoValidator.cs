using FluentValidation;
using MongoDB.Bson;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Common;

namespace NotesApp.Application.Validators.Notes
{
    public class NoteDtoValidator : AbstractValidator<NoteDto>
    {
        public NoteDtoValidator()
        {

            RuleSet(Constants.NoteDtoUpdateRuleSet, () =>
            {
                RuleFor(x => x.Id)
                    .Must(IsValidObjectId).WithMessage(ResponseMessages.InvalidNoteId);
            });

            RuleSet(Constants.NoteDtoUpdateRuleSet, () =>
            {
                RuleFor(x => x.Id).Empty().WithMessage(ResponseMessages.InvalidNoteId);
            });


            RuleFor(x => x.Title).NotEmpty().WithMessage(ResponseMessages.TitleRequired);
            RuleFor(x => x.Description).NotEmpty().WithMessage(ResponseMessages.DescriptionRequired);
            RuleFor(x => x.Priority).NotEmpty().WithMessage(ResponseMessages.PriorityRequired);
            RuleFor(x => x.Status).NotEmpty().WithMessage(ResponseMessages.StatusRequired);
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ResponseMessages.UserIdRequired)
                .Must(IsValidObjectId).WithMessage(ResponseMessages.InvalidUserId);
        }
        public bool IsValidObjectId(string id)
        {
            return ObjectId.TryParse(id, out _);
        }
    }

}
