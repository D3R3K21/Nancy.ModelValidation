using System;

namespace Nancy.ModelValidation
{
    public class UserModel : IntegrateModel<UserModel>
    {
        public UserModel()
        {
        }
        [ValidateRegex("This is a Regex error message on UserModel.PhoneNumber", RegexPatterns.PhoneNumber)]
        public string PhoneNumber { get; set; }
        //required bool values will need to be nullable to verify its been set and not defaulted to false
        [ValidateRequired("This is a Required error message on UserModel.UserName")]
        public bool? SomeBoolProperty { get; set; }

        [ValidateGuid("This is a Guid error message on UserModel.Id")]
        public Guid Id { get; set; }

        [ValidateRegex("This is a Regex error message on UserModel.UserName", RegexPatterns.Alphanumeric)]
        [ValidateRequired("This is a Required error message on UserModel.UserName")]
        public string UserName { get; set; }

    }
}