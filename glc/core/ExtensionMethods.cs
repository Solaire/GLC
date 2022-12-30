using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Logger;

/// <summary>
/// class for extension and helper methods
/// </summary>
public static class CExtensionMethods
{
    /// <summary>
    /// Return a new HashSet of type T from the IEnumerable
    /// </summary>
    /// <typeparam name="T">Type specifier for source and return</typeparam>
    /// <param name="source">Source of data to be saves as a HashSet</param>
    /// <param name="comparer">Hashset comparison function. Defaults to null</param>
    /// <returns>New HashSet of type T containing source data</returns>
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
    {
        return new HashSet<T>(source, comparer);
    }

    /// <summary>
    /// Get enum description
    /// </summary>
    /// <returns>description string</returns>
    /// <param name="enum">Enum</param>
    public static T GetDescription<T>(this Enum source) where T : Attribute
    {
        var enumType = source.GetType();
        var name = Enum.GetName(enumType, source);
        return enumType.GetField(name).GetCustomAttributes(false).OfType<T>().SingleOrDefault();
        /*
        try
        {
            FieldInfo field = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attr = (DescriptionAttribute[])field.GetCustomAttributes(
                    typeof(DescriptionAttribute), false);

            if(attr != null && attr.Length > 0) return attr[0].Description;
        }
        catch(Exception e)
        {
            CLogger.LogError(e);
        }
        Type type = source.GetType();
        string output = type.GetEnumName(source);
        if(!string.IsNullOrEmpty(output))
            return output;
        return source.ToString();
        */
    }

    public static T GetValueFromDescription<T>(string description, T defaultValue = default(T)) where T : Enum
    {
        foreach(var field in typeof(T).GetFields())
        {
            if(Attribute.GetCustomAttribute(field,
            typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if(string.Equals(attribute.Description, description, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null);
                }
            }
            else
            {
                if(string.Equals(field.Name, description, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null);
                }
            }
        }

        //throw new ArgumentException("Not found.", nameof(description));
        return default(T);
    }
}