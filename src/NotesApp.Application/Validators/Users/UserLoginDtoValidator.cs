using FluentValidation;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;

namespace NotesApp.Application.Validators.Users
{
    public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(ResponseMessages.EmailRequired).EmailAddress().WithMessage("Not valid Email");
            RuleFor(x => x.Password).NotEmpty().WithMessage(ResponseMessages.PasswordRequired);
        }
    }
}
