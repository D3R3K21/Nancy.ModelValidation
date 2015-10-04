using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nancy.ModelValidation
{
    public class ValidatorRouteBuilder : Nancy.NancyModule.RouteBuilder
    {
        public ValidatorRouteBuilder(string method, NancyModule parentModule)
            : base(method, parentModule)
        {
        }

        public Func<dynamic, CancellationToken, Task<dynamic>> this[string endpointName, string path, Type modelType]
        {
            set
            {
                AddRoute(endpointName, path, ctx =>
                {
                    #region ModelValidation
                    if (modelType.BaseType == typeof(NancyValidatorModel))
                    {
                        ctx.Items.Add("RequestModel", modelType);
                    }
                    else
                    {
                        throw new Exception("Type argument in endpoint must be derived from NancyValidatorModel");
                    }

                    #endregion

                    return true;
                }, value);
            }
        }
    }
}
