using System;
using System.Linq;

namespace Nancy.ModelValidation
{
    public class ValidatorModule : NancyModule
    {
        public NancyValidatorModel ResolvedModel { get; set; }

        public ValidatorModule()
        {
            Before += ValidateBeforeRequest;
        }

        public ValidatorModule(string path)
            : base(path)
        {
            Before += ValidateBeforeRequest;
        }


        protected virtual Response ValidateBeforeRequest(NancyContext context)
        {
            Response response = null;
            object modelType;

            if (!context.Items.TryGetValue("RequestModel", out modelType)) return response;

            var model = this.BindModel((Type)modelType);
            var validationResponse = model.Validate();
            var allValidCheck = validationResponse.All(p => p.IsValid);
            ResolvedModel = model;

            if (allValidCheck) return response;

            response = Response.AsJson(new
            {
                Model = ResolvedModel,
                InvalidProperties = validationResponse.Where(p => !p.IsValid).ToList()
            });
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