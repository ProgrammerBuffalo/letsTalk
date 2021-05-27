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
            if (text.Length < 7 || text.Length > 14) return new ValidationResult(false, "max 7-14 symbols");
            Regex regex = new Regex("^[\\w,!,#,$,%,^,&,*,@]{7,14}$");
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
            if (text.Length < 2 || text.Length > 20) return new ValidationResult(false, "max 2-20 symbols");
            Regex regex = new Regex("^[\\w]{2,20}$");
            if (regex.IsMatch(text)) return new ValidationResult(true, "");
            else return new ValidationResult(false, "name can contain only letters");
            return new ValidationResult(true, "");
        }
    }
}
