using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nancy.ModelValidation
{
    public class ValidateRegexAttribute : ValidateAttribute
    {
        private readonly Regex _regex;
        private ValidateRegexAttribute()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessage">The error to return if not successful</param>
        /// <param name="regexPattern">The pattern to check the property against</param>
        public ValidateRegexAttribute(string errorMessage, string regexPattern)
            : base(errorMessage)
        {
            _regex = new Regex(@regexPattern);
        }


        public override bool Valitade<T>(T val)
        {
            if (val != null && !(val is string))
            {
                return false;
            }
            else if (val == null)
            {
                return _regex.Match(string.Empty.Trim()).Success;
            }
            else
            {
                return _regex.Match((val as string).Trim()).Success;
            }
        }

    }
}