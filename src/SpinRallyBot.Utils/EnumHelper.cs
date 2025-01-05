using System.Reflection;

namespace SpinRallyBot;

public static class EnumHelper {
    /// <summary>
    ///     Gets an attribute on an enum field value
    /// </summary>
    /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
    /// <param name="enumVal">The enum value</param>
    /// <returns>The attribute of type T that exists on the enum value</returns>
    /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
    public static T? GetAttributeOfType<T>(this Enum enumVal) where T : Attribute {
        Type type = enumVal.GetType();
        MemberInfo[] memInfo = type.GetMember(enumVal.ToString());
        Attribute? attribute = memInfo[0].GetCustomAttribute(typeof(T), false);
        return (T?)attribute;
    }

    public static T[] GetAttributesOfType<T>(this Enum enumVal) where T : Attribute {
        Type type = enumVal.GetType();
        MemberInfo[] memInfo = type.GetMember(enumVal.ToString());
        object[] attribute = memInfo[0].GetCustomAttributes(typeof(T), false);
        return (T[])attribute;
    }
}
