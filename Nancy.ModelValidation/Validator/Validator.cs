using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nancy;
using Nancy.ModelBinding;

namespace Nancy.ModelValidation
{
    public static class Validator
    {
        private static Dictionary<Type, Dictionary<PropertyInfo, List<ValidateAttribute>>> _propertyMappings;
        private static Dictionary<Type, Func<NancyModule, NancyValidatorModel, object>> _bindingMappings;


        public static List<Type> ModelTypes;
        public static MethodInfo BindMethodInfo { get; set; }
        public static MethodInfo GenericMethodInfo { get; set; }

        public static void Initialize()
        {
        }

        static Validator()
        {
            BindMethodInfo = typeof(ModuleExtensions).GetMethods().First(p => p.Name == "Bind"
                && p.IsGenericMethod
                && p.GetParameters().Length == 1);

            _propertyMappings = new Dictionary<Type, Dictionary<PropertyInfo, List<ValidateAttribute>>>();
            _bindingMappings = new Dictionary<Type, Func<NancyModule, NancyValidatorModel, object>>();

            GetModelMappings();
            ModelTypes = _propertyMappings.Select(p => p.Key).ToList();
        }


        private static void GetModelMappings()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var types = GetDerivedTypes(assembly, typeof(NancyValidatorModel));

            types.ForEach(modelType =>
            {
                var dictionary = new Dictionary<PropertyInfo, List<ValidateAttribute>>();

                var modelProperties = modelType.GetProperties().Where(m => m.GetCustomAttributes(typeof(ValidateAttribute), true).Length > 0).ToList();

                modelProperties.ForEach(propertyInfo =>
                {
                    var attList = new List<ValidateAttribute>();
                    propertyInfo.CustomAttributes.ToList().ForEach(p =>
                    {
                        attList.Add((ValidateAttribute)Attribute.GetCustomAttribute(propertyInfo, p.AttributeType));
                    });

                    dictionary.Add(propertyInfo, attList);
                });
                _propertyMappings.Add(modelType, dictionary);
            });
        }
        public static dynamic BindModel(this NancyModule mod, NancyValidatorModel model)
        {
            return _bindingMappings[model.DerivedType](mod, model);

        }

        public static void CreateDelegate(this NancyModule mod, NancyValidatorModel model)
        {
            GenericMethodInfo = BindMethodInfo.MakeGenericMethod(model.DerivedType);

            Expression<Func<NancyModule, NancyValidatorModel, object>> func = (o, m) =>
            Convert.ChangeType(GenericMethodInfo.Invoke(BindMethodInfo.DeclaringType, new object[] { o }), m.DerivedType);

            _bindingMappings.Add(model.DerivedType, func.Compile());

        }

        public static List<ValidatorReturnObject> Validate(this NancyValidatorModel model)
        {

            var returnList = new List<ValidatorReturnObject>();
            var modelProperties = _propertyMappings[model.DerivedType];


            var propinfolist = modelProperties.Select(p => p.Key).ToList();

            propinfolist.ForEach(p =>
            {
                var propertyValue = p.GetValue(model, null);

                var attList = modelProperties[p];
                attList.ForEach(x =>
                {

                    returnList.Add(new ValidatorReturnObject
                    {
                        PropertyName = p.Name,
                        PropertyValue = propertyValue,
                        IsValid = x.Valitade(propertyValue),
                        Error = x.ErrorMessage
                    });
                });
            });

            return returnList;
        }


        private static List<Type> GetDerivedTypes(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t) && t != baseType && !t.Name.Contains("`1")).ToList();
        }

    }
}