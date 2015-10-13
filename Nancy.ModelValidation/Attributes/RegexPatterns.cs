namespace Nancy.ModelValidation
{
    public static class RegexPatterns
    {
        public const string PhoneNumber = @"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}";
        public const string Alpha = @"^[A-Za-z ]+$";
        public const string Alphanumeric = @"^[A-Za-z0-9 ]+$";
    }
}