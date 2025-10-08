using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApp.Helpers.Validation.File
{
    /// <summary>
    /// Validation attribute that enforces the maximum allowed file size (in bytes) for uploads.
    /// Supports server‐side and unobtrusive client‐side validation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MaxFileSizeAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly int _maxBytes;
        /// <summary>
        /// Initializes an attribute that checks that the filesize of the file being uploaded is in the rage of acceptable.
        /// </summary>
        /// <param name="maxBytes"></param>
        public MaxFileSizeAttribute(int maxBytes) => _maxBytes = maxBytes;

        /// <summary>
        /// Check if an object is valid
        /// </summary>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var file = value as IFormFile;
            if (file != null && file.Length > _maxBytes)
                return new ValidationResult($"Maximum file size — {_maxBytes/1024/1024} МБ.");
            return ValidationResult.Success;
        }
        
        /// <summary>
        /// For client validation
        /// </summary>
        /// <param name="context"></param>
        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-maxfilesize", ErrorMessage ?? $"Maximum {_maxBytes/1024/1024} МБ.");
            MergeAttribute(context.Attributes, "data-val-maxfilesize-maxfilesize", _maxBytes.ToString());
        }

        private bool MergeAttribute(IDictionary<string, string> attrs, string key, string value)
        {
            if (attrs.ContainsKey(key)) return false;
            attrs.Add(key, value);
            return true;
        }
    }
}