namespace Bookify.Validation
{
    public class CategoryFormValidation : AbstractValidator<CategoryFormViewModel>
    {
        public CategoryFormValidation()
        {
                RuleFor(x=>x.CategoryName).MaximumLength(50).WithMessage(Errors.MaxLength)
                .Matches(RegexPattern.CharactersOnly_Eng).WithMessage(Errors.OnlyEnglishLetters);
        }
    }
}
