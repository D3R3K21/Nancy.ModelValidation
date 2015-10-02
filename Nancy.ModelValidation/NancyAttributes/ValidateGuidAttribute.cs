using System;

namespace Nancy.ModelValidation
{
    public class ValidateGuidAttribute : ValidateAttribute
    {

        private ValidateGuidAttribute()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessage">The error to return if validation is not successful</param>
        public ValidateGuidAttribute(string errorMessage)
            : base(errorMessage)
        {
        }

        public override bool Valitade<T>(T val)
        {
            return new Guid(val.ToString()) != default(Guid);
        }
    }
}