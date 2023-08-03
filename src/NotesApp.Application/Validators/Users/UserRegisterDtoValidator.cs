using FluentValidation;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;

namespace NotesApp.Application.Validators.Users
{
    public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
    {
        public UserRegisterDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(ResponseMessages.EmailRequired);
            RuleFor(x => x.Password).NotEmpty().WithMessage(ResponseMessages.PasswordRequired);
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(ResponseMessages.FirstNameRequired);
            RuleFor(x => x.LastName).NotEmpty().WithMessage(ResponseMessages.LastNameRequired);
        }
    }
}
