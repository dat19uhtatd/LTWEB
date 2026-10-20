using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QLGiaSuLopHoc.Models.Validation
{
    /// <summary>
    /// Custom Validation Attribute: thời điểm phải lớn hơn hiện tại tối thiểu N giờ.
    /// Hỗ trợ cả kiểm tra phía server (IsValid) lẫn phía client (IClientModelValidator + jQuery Unobtrusive).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class FutureDateAttribute : ValidationAttribute, IClientModelValidator
    {
        public int MinHoursAhead { get; set; } = 1;

        public override bool IsValid(object? value)
        {
            if (value is DateTime dateTime)
                return dateTime > DateTime.Now.AddHours(MinHoursAhead);
            return false;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            Merge(context.Attributes, "data-val", "true");
            Merge(context.Attributes, "data-val-futuredate", ErrorMessage ?? "Thời gian không hợp lệ");
            Merge(context.Attributes, "data-val-futuredate-hours", MinHoursAhead.ToString());
        }

        private static void Merge(IDictionary<string, string> attributes, string key, string value)
        {
            if (!attributes.ContainsKey(key)) attributes.Add(key, value);
        }
    }
}
