using System;

namespace TestModule
{
    public abstract class ValidateAttribute : Attribute
    {
        protected ValidateAttribute()
        {
        }

        protected ValidateAttribute(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public abstract bool Valitade<T>(T val);

        public string ErrorMessage { get; protected set; }
    }
}