using System;

namespace Nancy.ModelValidation
{
    public abstract class NancyValidatorModel
    {
        public Type DerivedType { get; set; }

        protected NancyValidatorModel()
        {
        }
        public static T Bind<T>(T obj) where T : class
        {
            var type = obj.GetType();
            return Activator.CreateInstance(type) as T;
        }
    }
}