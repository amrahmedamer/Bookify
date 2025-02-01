namespace Bookify.Validation
{
    public class SubscriberFormValidation : AbstractValidator<SubscriberFormViewModel>
    {
        public SubscriberFormValidation()
        {
            RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage(Errors.MaxLength)
            .Matches(RegexPattern.DenySpecialCharacters)
            .WithMessage(Errors.DenySpecialCharacters);

            RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage(Errors.MaxLength)
            .Matches(RegexPattern.DenySpecialCharacters)
            .WithMessage(Errors.DenySpecialCharacters);

            RuleFor(x => x.NationalId)
            .MaximumLength(20)
            .WithMessage(Errors.MaxLength)
            .Matches(RegexPattern.egyptianIdPattern)
            .WithMessage(Errors.EgyptNationalId);

            RuleFor(x => x.Email)
            .MaximumLength(150)
            .EmailAddress();

            RuleFor(x => x.MobileNumber)
              .MaximumLength(15)
              .WithMessage(Errors.MaxLength)
              .Matches(RegexPattern.PhoneNumber)
              .WithMessage(Errors.PhoneNumber);

            RuleFor(x => x.Address)
              .MaximumLength(500)
              .WithMessage(Errors.MaxLength);
        }
    }
}
