using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nancy.ModelBinding;

namespace Nancy.ModelValidation
{
    public static class Validator
    {
        private static Dictionary<Type, Dictionary<PropertyInfo, List<ValidateAttribute>>> _modelPropertyMappings;
        private static Dictionary<Type, Func<NancyModule, NancyValidatorModel, object>> _bindingMappings;
        private static List<Type> _modelTypes;

        private static MethodInfo BindMethodInfo { get; set; }
        private static NancyModule _moduleInstance;

        public static void Initialize()
        {
            
            var assembly = Assembly.GetCallingAssembly();
            
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
            _bindingMappings = new Dictionary<Type, Func<NancyModule, NancyValidatorModel, object>>();
            _modelTypes = new List<Type>();
            BindMethodInfo = typeof(ModuleExtensions).GetMethods().First(p => p.Name == "Bind"
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
            var genericMethodInfo = BindMethodInfo.MakeGenericMethod(modelType);

            Expression<Func<NancyModule, NancyValidatorModel, object>> func = (o, m) =>
            Convert.ChangeType(genericMethodInfo.Invoke(BindMethodInfo.DeclaringType, new object[] { o }), m.DerivedType);

            _bindingMappings.Add(modelType, func.Compile());

        }

        private static List<Type> GetDerivedTypes(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t) && t != baseType && !t.Name.Contains("`1")).ToList();
        }


        #region Public Extensions

        public static List<ValidatorReturnObject> Validate(this NancyValidatorModel model)
        {

            var returnList = new List<ValidatorReturnObject>();
            var modelProperties = _modelPropertyMappings[model.DerivedType];


            var propinfolist = modelProperties.Select(p => p.Key).ToList();

            propinfolist.ForEach(p =>
            {
                //reflection used in validation
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

        public static dynamic BindModel(NancyValidatorModel model)
        {
            var modelType = model.GetType();
            var mapping = _bindingMappings[modelType];



            return _bindingMappings[model.DerivedType](_moduleInstance, model);

        }

        #endregion


    }
}