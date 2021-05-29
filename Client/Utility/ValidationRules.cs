using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Client.Utility
{
    //валидация для пароля и логина(от 7 до 14 символов буквы цифры и спец символы в любом порядке)
    public class AuthorizationRules : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string text = value.ToString();
            if (text.Length < 5 || text.Length > 15) return new ValidationResult(false, "Must be in the range of 5-15 symbols");
            Regex regex = new Regex("^[\\w,!,#,$,%,^,&,*,@]{5,15}$");
            if (regex.IsMatch(text)) return new ValidationResult(true, "");
            else return new ValidationResult(false, "valid symbols (!,#,$,%,^,&,*,@)");
            return new ValidationResult(true, "");
        }
    }

    public class NameRules : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string text = value.ToString();
            if (text.Length < 4 || text.Length > 12) return new ValidationResult(false, "The name must be in the range of 4-12 letters");
            Regex regex = new Regex("^[\\w]{4,12}$");
            if (regex.IsMatch(text)) return new ValidationResult(true, "");
            else return new ValidationResult(false, "name can contain only letters");
            return new ValidationResult(true, "");
        }
    }
}
