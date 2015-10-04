using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Nancy.ModelValidation
{
    public static class Validator
    {
        private static Dictionary<Type, Dictionary<PropertyInfo, List<ValidateAttribute>>> _modelPropertyMappings;
        private static Dictionary<Type, Func<NancyModule, Type, object>> _bindingMappings;
        private static List<Type> _modelTypes;

        private static MethodInfo BindMethodInfo { get; set; }

        public static void Initialize()
        {

            var assembly = Assembly.GetCallingAssembly();
            NancyModule _moduleInstance;
            var moduleType = GetDerivedTypes(assembly, typeof(NancyModule)).First();
            _moduleInstance = (NancyModule)Activator.CreateInstance(moduleType);



            _modelTypes = GetDerivedTypes(assembly, typeof(NancyValidatorModel));

            GetModelPropertyMappings();
            _modelTypes.ForEach(p =>
            {
                CreateDelegate(_moduleInstance, p);
            });

        }

        static Validator()
        {
            _modelPropertyMappings = new Dictionary<Type, Dictionary<PropertyInfo, List<ValidateAttribute>>>();
            _bindingMappings = new Dictionary<Type, Func<NancyModule, Type, object>>();
            _modelTypes = new List<Type>();
            BindMethodInfo = typeof(Nancy.ModelBinding.ModuleExtensions).GetMethods().First(p => p.Name == "Bind"
                && p.IsGenericMethod
                && p.GetParameters().Length == 1);
        }


        private static void GetModelPropertyMappings()
        {
            _modelTypes.ForEach(modelType =>
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
                _modelPropertyMappings.Add(modelType, dictionary);
            });
        }


        private static void CreateDelegate(NancyModule mod, Type modelType)
        {
            Func<NancyModule, Type, object> func = (module, type) =>
            Convert.ChangeType(BindMethodInfo.MakeGenericMethod(modelType).Invoke(BindMethodInfo.DeclaringType, new object[] { module }), type);
            _bindingMappings.Add(modelType, func);

        }

        private static List<Type> GetDerivedTypes(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t) && t != baseType && !t.Name.Contains("`1")).ToList();
        }


        #region Public Extensions

        public static List<ValidatorReturnObject> Validate(this NancyValidatorModel model)
        {
            var returnList = new List<ValidatorReturnObject>();
            var modelProperties = _modelPropertyMappings[model.GetType()];


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

        public static dynamic BindModel(this NancyModule mod, Type modelType)
        {

            return _bindingMappings[modelType](mod, modelType);

        }

        #endregion


    }
}