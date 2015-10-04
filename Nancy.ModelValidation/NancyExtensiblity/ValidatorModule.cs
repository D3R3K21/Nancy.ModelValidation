using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nancy.ModelValidation
{
    public class ValidatorModule : NancyModule
    {
        public NancyValidatorModel ResolvedModel { get; set; }
        public ValidatorModule()
        {
            this.Before += ValidateBeforeRequest;
        }
        public ValidatorModule(string path)
            : base(path)
        {
            this.Before += ValidateBeforeRequest;
        }


        protected virtual Response ValidateBeforeRequest(NancyContext context)
        {
            Response response = null;
            object modelType = null;
            Type typedModel = null;

            if (context.Items.TryGetValue("RequestModel", out modelType))
            {
                typedModel = (Type)modelType;
                var model = this.BindModel(typedModel);
                var validationResponse = model.Validate();
                var allValidCheck = validationResponse.All(p => p.IsValid);
                ResolvedModel = model;
                if (!allValidCheck)
                {
                    response = Response.AsJson(new
                    {
                        Model = ResolvedModel,
                        InvalidProperties = validationResponse.Where(p => !p.IsValid).ToList()
                    });
                }

            }


            return response;
        }

        public new ValidatorRouteBuilder Get
        {
            get { return new ValidatorRouteBuilder("GET", this); }
        }

        public new ValidatorRouteBuilder Post
        {
            get { return new ValidatorRouteBuilder("POST", this); }
        }

        public new ValidatorRouteBuilder Delete
        {
            get { return new ValidatorRouteBuilder("DELETE", this); }
        }

        public new ValidatorRouteBuilder Put
        {
            get { return new ValidatorRouteBuilder("PUT", this); }
        }
        public new ValidatorRouteBuilder Options
        {
            get { return new ValidatorRouteBuilder("OPTIONS", this); }
        }

        public new ValidatorRouteBuilder Patch
        {
            get { return new ValidatorRouteBuilder("PATCH", this); }
        }
    }
}
