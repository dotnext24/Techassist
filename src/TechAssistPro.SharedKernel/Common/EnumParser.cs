
namespace TechAssistPro.SharedKernel.Common
{
   public static class EnumParser
{
    public static TEnum Parse<TEnum>(string value)
        where TEnum : struct, Enum
        => Enum.Parse<TEnum>(value, ignoreCase: true);
}
}