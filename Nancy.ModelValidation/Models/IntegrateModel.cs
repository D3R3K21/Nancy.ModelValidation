using System;

namespace Nancy.ModelValidation
{
    public class IntegrateModel<T> : NancyValidatorModel
    {
        public IntegrateModel()
        {
            DerivedType = typeof(T);
        }

    }
}