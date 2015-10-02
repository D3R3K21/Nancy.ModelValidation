namespace Nancy.ModelValidation
{
    public class ValidatorReturnObject
    {
        public string PropertyName { get; set; }
        public object PropertyValue { get; set; }
        public bool IsValid { get; set; }
        public string Error { get; set; }
    }
}