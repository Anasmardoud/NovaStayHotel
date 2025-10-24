// Ignore Spelling: Fixup

using System.Collections.Immutable;
using System.Reflection;

namespace NovaStayHotel;
public static class ObjectExtensions
{
    #region Fixup String Properties
    public static void FixupStringProperties(this object target)
    {
        var stringProperties = (from x in target.GetType().GetRuntimeProperties()
                                where x.CanRead && x.CanWrite
                                   && x.PropertyType == typeof(string)
                                   && x.GetMethod != null && x.GetMethod.IsPublic && !x.GetMethod.IsStatic
                                select x).ToImmutableArray();
        foreach (var property in stringProperties)
        {
            var value = (string?)property.GetValue(target, null);
            if (value == null)
                continue;
            var fixedValue = value.Trim();
            fixedValue = fixedValue.Replace((char)160, ' '); // Replace breaking space with normal space
            fixedValue = fixedValue.Replace((char)173, '-'); // Replace soft hyphen with normal hyphen
            fixedValue = fixedValue.Replace(((char)2).ToString(), ""); // Remove start of text (not supported in XML serialization)
            if (fixedValue == "")
                fixedValue = null;
            property.SetValue(target, fixedValue, null);
        }
    }
    #endregion
}
