using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApp.Helpers.Validation.File
{
    /// <summary>
    /// Validation attribute that restricts uploaded files to a specified set of extensions.
    /// Supports server‐side and unobtrusive client‐side validation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AllowedExtensionsAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly HashSet<string> _exts;
        /// <summary>
        /// Initializes an attribute that checks that the extension of the file being uploaded is in the list of valid extensions.
        /// </summary>
        /// <param name="exts"></param>
        public AllowedExtensionsAttribute(string[] exts)
            => _exts = new HashSet<string>(exts, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Check if an object is valid
        /// </summary>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var ext = Path.GetExtension(file.FileName);
                if (!_exts.Contains(ext))
                    return new ValidationResult($"Invalid file format. Valid: {string.Join(", ", _exts)}");
            }
            return ValidationResult.Success;
        }
        
        /// <summary>
        /// For client validation
        /// </summary>
        /// <param name="context"></param>
        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-allowedextensions", ErrorMessage ?? "Invalid file format.");
            MergeAttribute(context.Attributes, "data-val-allowedextensions-allowedextensions", string.Join(",", _exts));
        }

        private bool MergeAttribute(IDictionary<string, string> attrs, string key, string value)
        {
            if (attrs.ContainsKey(key)) return false;
            attrs.Add(key, value);
            return true;
        }
    }
}