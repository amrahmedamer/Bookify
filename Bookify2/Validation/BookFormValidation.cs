namespace Bookify.Validation
{
    public class BookFormValidation : AbstractValidator<BookFormViewModel>
    {
        public BookFormValidation()
        {
            RuleFor(x => x.Title)
                .MaximumLength(100)
                .WithMessage(Errors.MaxLength)
                .Matches(RegexPattern.CharactersOnly_Eng)
                .WithMessage(Errors.OnlyEnglishLetters);

            RuleFor(x => x.Publisher)
                .MaximumLength(100)
                .WithMessage(Errors.MaxLength);

            RuleFor(x => x.Hall)
                .MaximumLength(50)
                .WithMessage(Errors.MaxLength);
        }
    }
}
