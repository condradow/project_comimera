using System.ComponentModel.DataAnnotations;
using System.Reflection;
namespace WebFormular.Enum
{
  

    public static class EnumExtensions
    {
        public static string GetDisplayName(this System.Enum value)
        {
            var member = value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault();

            return member?
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName()
                ?? value.ToString();
        }
    }
}
