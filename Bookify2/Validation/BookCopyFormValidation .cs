namespace Bookify.Validation
{
    public class BookCopyFormValidation : AbstractValidator<BookCopyFormViewModel>
    {
        public BookCopyFormValidation()
        {
            RuleFor(x => x.EditionNumber).InclusiveBetween(1, 1000).WithMessage(Errors.Rang);
        }
    }
}
