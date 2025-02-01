namespace Bookify.Validation
{
    public class AuthorFormValidation : AbstractValidator<AuthorFormViewModel>
    {
        public AuthorFormValidation()
        {
                RuleFor(x=>x.Name).MaximumLength(20).WithMessage(Errors.MaxLength)
                .Matches(RegexPattern.CharactersOnly_Eng).WithMessage(Errors.OnlyEnglishLetters);
        }
    }
}
