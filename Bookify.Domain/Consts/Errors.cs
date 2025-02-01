namespace Bookify.Domain.Consts
{
    public static class Errors
    {
        public const string MaxLength = "{PropertyName} cannot be more than {MaxLength} charachter!";
        public const string Required = "{0} field is required.";
        public const string MaxMinLength = "The {PropertyName} must be at least {MinLength} and at max {MaxLength} characters long";
        public const string Duplicated = "Another record with the same {0} name already exists!";
        public const string DuplicatedBook = "Book with the same title is already exists with the same author!";
        public const string NoTAllowedExtention = "Only .jpg, .jpeg, .png files are allowed!";
        public const string MaxSize = "File cannot be more than 2 MB!";
        public const string NotAllowFutureDate = "Date Cannot be in the future!";
        public const string Rang = "{PropertyName} should be between {From} and {To} !";
        public const string ConfirmPasswordNotMatch = "The password and confirmation password do not match.";
        public const string weakPassword = "passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long.";
        public const string InvalidUsername = "Username can only contain letters or digits.";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";
        public const string PhoneNumber = "Please enter number valid.";
        public const string EgyptNationalId = "Please enter a valid national ID number For Egypt such as (30001011234567).";
        public const string InvalidSerialNumber = "Invalid serial number";
        public const string NotAvailableForRental = "this book/copy is not available for rental.";
        public const string BlackListedSubscriber = "this subscriber is blacklisted.";
        public const string InactiveSubscriber = "this subscriber is inactive.";
        public const string MaxCopiesReached = "this subscriber reached the max number for rentals.";
        public const string CopyIsRental = "this copy is already rentaled.";
        public const string RentalNotAllowedBlacklisted = "rental cannot be extended for  subscriber blacklisted.";
        public const string RentalNotAllowedInactive = "rental cannot be extended for this subscriber before renwal.";
        public const string ExtendedNotAllowed = "rental cannot be extended.";
        public const string PenaltyPaid = "Penalty should be paid.";
        public const string InvalidEndDate = "Invalid End Date.";
        public const string InvalidStartDate = "Invalid Start Date.";
    }
}
