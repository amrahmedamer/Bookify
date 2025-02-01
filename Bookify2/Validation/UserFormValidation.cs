namespace Bookify.Validation
{
    public class UserFormValidation : AbstractValidator<UserFormViewModel>
    {
        public UserFormValidation()
        {
            RuleFor(x => x.FullName).MaximumLength(100)
            .WithMessage(Errors.MaxLength)
            .Matches(RegexPattern.CharactersOnly_Eng)
            .WithMessage(Errors.OnlyEnglishLetters);

            RuleFor(x => x.FullName).MaximumLength(20)
                .WithMessage(Errors.MaxLength)
                .Matches(RegexPattern.Username)
                .WithMessage(Errors.InvalidUsername);

            RuleFor(x => x.Email).MaximumLength(200)
                .WithMessage(Errors.MaxLength);

            RuleFor(x => x.Password)
                .Length(8, 100)
                .WithMessage(Errors.MaxMinLength)
               .Matches(RegexPattern.Password)
               .WithMessage(Errors.weakPassword);

            RuleFor(x => x.ConfirmPassword)
                .Equal(e => e.Password)
                .WithMessage(Errors.ConfirmPasswordNotMatch);
        }
    }
}
