using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SharedLib;
using SharedLib.Models;

namespace AddressBookWebService.Validation
{
    public class AddressExistAttribute : ValidationAttribute, IClientModelValidator
    {
        protected override ValidationResult IsValid (object value, ValidationContext validationContext)
        {
            string addressValue = value.ToString();
            bool result = Address.IsIotaAddress(addressValue);
            if (result) return ValidationResult.Success;
            else return new ValidationResult("Addres not found!");
        }

        void IClientModelValidator.AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-addressexist", "Addres not found!");
        }
    }
}
