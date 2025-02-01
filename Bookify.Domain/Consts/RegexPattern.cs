namespace Bookify.Domain.Consts
{
    public static class RegexPattern
    {
        public const string Password = "(?=(.*[0-9]))(?=.*[\\!@#$%^&*()\\\\[\\]{}\\-_+=~`|:;\"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}";
        public const string Username = "^[a-zA-Z0-9-._@+]*$";
        public const string CharactersOnly_Eng = "^[a-zA-Z-_ ]*$";
        public const string CharactersOnly_Ar = "^[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FF ]*$";
        public const string NumbersAndChrOnly_ArEng = "^(?=.*[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FFa-zA-Z])[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FFa-zA-Z0-9 _-]+$";
        public const string DenySpecialCharacters = "^[^<>!#%$]*$";
        public const string PhoneNumber = "^01[0125][0-9]{8}$";
        public const string egyptianIdPattern = @"^([23])\d{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])\d{7}$";
    }
}
