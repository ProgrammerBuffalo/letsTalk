using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Client.Utility
{
    public class AuthorizationRules : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string text = value.ToString();
            if (text.Length < 5 || text.Length > 15) return new ValidationResult(false, App.Current.Resources["NameErrorLength"].ToString());
            Regex regex = new Regex("^[\\w,!,#,$,%,^,&,*,@]{5,15}$");
            if (regex.IsMatch(text)) return new ValidationResult(true, "");
            else return new ValidationResult(false, App.Current.Resources["PasswordErrorValidSymbols"].ToString());
        }
    }

    public class NameRules : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string text = value.ToString();
            if (text.Length < 4 || text.Length > 12) return new ValidationResult(false, App.Current.Resources["NameErrorLength"].ToString());
            Regex regex = new Regex("^[\\w]{4,12}$");
            if (regex.IsMatch(text)) return new ValidationResult(true, "");
            else return new ValidationResult(false, App.Current.Resources["NameErrorValidSymbols"].ToString());
        }
    }
}
