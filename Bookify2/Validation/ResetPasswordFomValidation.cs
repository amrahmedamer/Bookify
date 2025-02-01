namespace Bookify.Validation
{
    public class ResetPasswordFomValidation : AbstractValidator<ResetPasswordFomViewModel>
    {
        public ResetPasswordFomValidation()
        {
            RuleFor(x => x.Password)
                .Length(8, 100)
                .WithMessage(Errors.MaxMinLength)
                .Matches(RegexPattern.Password)
                .WithMessage(Errors.weakPassword);

            RuleFor(x => x.ConfirmPassword)
                .Equal(e=>e.Password)
                .WithMessage(Errors.ConfirmPasswordNotMatch);
        }
    }
}
