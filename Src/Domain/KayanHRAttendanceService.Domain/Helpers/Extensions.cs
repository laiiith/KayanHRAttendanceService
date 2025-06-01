using Newtonsoft.Json;
using System.ComponentModel;
using System.Data;

namespace KayanHRAttendanceService.Domain.Helpers;

public static class Extensions
{
    public static string ToJson(this object objItem, bool AllowNull)
    {
        var json = string.Empty;
        if (AllowNull == false)
        {
            json = JsonConvert.SerializeObject(objItem, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
        else
        {
            json = JsonConvert.SerializeObject(objItem);
        }
        return json;
    }
    public static dynamic JsonToAnyobject<T>(this string _jsonText)
    {
        dynamic obj = JsonConvert.DeserializeObject<T>(_jsonText);

        return obj;
    }
    public static dynamic JsonToAnyListOfobject<T>(this string _jsonText)
    {
        return JsonConvert.DeserializeObject<List<T>>(_jsonText);
    }
    public static T ToAnyType<T>(this object txt)
    {
        if (txt != null)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromInvariantString(txt.ToString());
        }
        return default;
    }
    public static DataTable ToDataTable<T>(this IList<T> data)
    {
        var props = TypeDescriptor.GetProperties(typeof(T));

        using var table = new DataTable();

        for (int i = 0; i < props.Count; i++)
        {
            var prop = props[i];
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }
        if (data != null)
        {
            var values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
        }
        return table;
    }
    public static DataTable ToDataTable<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        var table = new DataTable();

        // Add columns for keys and values
        table.Columns.Add("Key", typeof(TKey));
        table.Columns.Add("Value", typeof(TValue));

        // Add rows from the dictionary
        foreach (var kvp in dictionary)
        {
            table.Rows.Add(kvp.Key, kvp.Value);
        }

        return table;
    }
    public static DataTable ToDataTable<T>(this IEnumerable<T> data)
    {
        var props = TypeDescriptor.GetProperties(typeof(T));
        using var table = new DataTable();

        for (int i = 0; i < props.Count; i++)
        {
            var prop = props[i];
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }
        if (data != null)
        {
            var values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
        }
        return table;
    }
    public static bool IsNotNullOrEmpty(this string txt)
    {
        return !string.IsNullOrWhiteSpace(txt);
    }
    public static List<T> DataReaderMapToList<T>(this IDataReader dr, bool CurrentProperty)
    {
        var list = new List<T>();
        T obj = default;
        while (dr.Read())
        {
            obj = Activator.CreateInstance<T>();
            var Properties = CurrentProperty == true ? obj.GetType().GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) : obj.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo prop in Properties)
            {
                if (dr.IDataRecordHasColumn(prop.Name) == true)
                {
                    if (!Equals(dr[prop.Name], DBNull.Value))
                    {
                        if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        {
                            if (!string.IsNullOrWhiteSpace(dr[prop.Name].ToString()))
                            {
                                prop.SetValue(obj, prop.PropertyType == typeof(DateTime) ? (DateTime)dr[prop.Name] : (DateTime?)dr[prop.Name], null);
                            }
                        }
                        else if (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?))
                        {
                            if (!string.IsNullOrWhiteSpace(dr[prop.Name].ToString()))
                            {
                                if (prop.PropertyType == typeof(double))
                                {
                                    prop.SetValue(obj, dr[prop.Name].ToAnyType<double>(), null);
                                }
                                else if (prop.PropertyType == typeof(double?))
                                {
                                    prop.SetValue(obj, dr[prop.Name].ToAnyType<double?>(), null);
                                }
                            }
                        }
                        else { prop.SetValue(obj, dr[prop.Name], null); }
                    }
                }
            }
            list.Add(obj);
        }
        return list;
    }
    private static bool IDataRecordHasColumn(this IDataRecord dr, string columnName)
    {
        for (int i = 0; i < dr.FieldCount; i++)
        {
            if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                return true;
        }
        return false;
    }
}
