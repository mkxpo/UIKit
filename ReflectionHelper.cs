using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace Core {

    /// <summary>
    /// Вспомогательный класс для работы с данными о типах времени исполнения.
    /// Используется для получение списков редактируемых свойств объектов и их атрибутов.
    /// </summary>
    public static class ReflectionHelper {

        static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        static readonly ConcurrentDictionary<Type, Attribute[]> typeAttributeCache = new ConcurrentDictionary<Type, Attribute[]>();
        static readonly ConcurrentDictionary<PropertyInfo, Attribute[]> propertyAttributeCache = new ConcurrentDictionary<PropertyInfo, Attribute[]>();
        static readonly Dictionary<PropertyInfo, Func<object, string>> propertyFormatMethodsCache = new Dictionary<PropertyInfo, Func<object, string>>();

        public static string GetTypeName(Type type) {
            DisplayNameAttribute attr = GetAttribute<DisplayNameAttribute>(type);
            if (attr != null) {
                return attr.DisplayName;
            }
            return type.Name;
        }

        public static string GetTypeName(object obj) {
            return GetTypeName(obj.GetType());
        }

        public static PropertyInfo[] GetVisibleProperties(Type type) {            
            return GetTypeProperties(type)
                .Where(p => IsPropertyVisible(p))
                .OrderBy(p => GetPropertyOrder(p))
                .ToArray();
        }

        public static PropertyInfo[] GetVisibleProperties(object obj) {
            return GetVisibleProperties(obj.GetType());
        }

        public static string GetPropertyName(PropertyInfo propertyInfo) {
            DisplayAttribute attr1 = GetAttribute<DisplayAttribute>(propertyInfo);
            if (attr1 != null && !string.IsNullOrEmpty(attr1.Name)) {
                return attr1.Name;
            }
            DisplayNameAttribute attr2 = GetAttribute<DisplayNameAttribute>(propertyInfo);
            if (attr2 != null) {
                return attr2.DisplayName;
            }
            return propertyInfo.Name;
        }

        public static string GetPropertyDisplayFormat(PropertyInfo propertyInfo) {
            DisplayFormatAttribute attr = GetAttribute<DisplayFormatAttribute>(propertyInfo);
            if (attr != null) {
                return attr.DataFormatString;
            }
            return "";
        }

        public static int GetPropertyOrder(PropertyInfo propertyInfo) {
            DisplayAttribute attr = GetAttribute<DisplayAttribute>(propertyInfo);
            if (attr != null) {
                int? order = attr.GetOrder();
                if (order != null) {
                    return order.Value;
                }
            }
            return Int32.MaxValue;
        }

        public static string GetPropertyTabTitle(PropertyInfo propertyInfo) {
            var attr = GetAttribute<TabAttribute>(propertyInfo);
            if (attr != null) {
                return attr.TabTitle;
            }
            return null;
        }

        public static int GetPropertyTextMaxLength(PropertyInfo propertyInfo) {
            MaxLengthAttribute attr = GetAttribute<MaxLengthAttribute>(propertyInfo);
            if (attr != null) {
                return attr.Length;
            }
            return 0;
        }

        public static bool IsPropertyRequired(PropertyInfo propertyInfo) {
            return HasAttribute(propertyInfo, typeof(RequiredAttribute));
        }

        public static bool IsPropertyNullable(PropertyInfo propertyInfo) {
            Type type = propertyInfo.PropertyType;
            return (type.IsValueType && Nullable.GetUnderlyingType(type) != null);
        }

        public static bool IsPropertyVisible(PropertyInfo propertyInfo) {
            bool? allowRead = SecurityHelper.IsReadAllowed(propertyInfo);
            return (HasAttribute(propertyInfo, typeof(DisplayAttribute))
                || HasAttribute(propertyInfo, typeof(DisplayNameAttribute))) && (allowRead == true || allowRead == null);
        }

        public static bool IsPropertyReadonly(PropertyInfo propertyInfo) {
            bool? allowEdit = SecurityHelper.IsEditAllowed(propertyInfo);
            if (allowEdit == false) {
                return true;
            }
            if (!propertyInfo.CanWrite) {
                return true;
            }
            ReadOnlyAttribute attr = GetAttribute<ReadOnlyAttribute>(propertyInfo);
            if (attr == null) {
                return false;
            }
            return attr.IsReadOnly;
        }        

        public static FieldVisibility GetVisibility(PropertyInfo propertyInfo) {
            var attr = GetAttribute<VisibilityAttribute>(propertyInfo);
            if (attr == null) {
                return FieldVisibility.Both;
            }
            return attr.Visibility;
        }

        public static SortDirectionAttribute GetSortDirection(PropertyInfo propertyInfo) {
            return GetAttribute<SortDirectionAttribute>(propertyInfo);
        }

        public static bool HasAttribute(PropertyInfo propertyInfo, Type attributeType) {
            var attrs = GetPropertyAttributes(propertyInfo);
            return attrs.Any(a => attributeType.IsAssignableFrom(a.GetType()));
        }

        public static TAttribute GetAttribute<TAttribute>(PropertyInfo propertyInfo) where TAttribute: Attribute {
            var attrs = GetPropertyAttributes(propertyInfo);
            return (TAttribute)attrs.FirstOrDefault(a => typeof(TAttribute).IsAssignableFrom(a.GetType()));
        }

        public static bool HasAttribute(Type type, Type attributeType) {
            Attribute[] attrs = GetTypeAttributes(type);
            return attrs.Any(a => attributeType.IsAssignableFrom(a.GetType()));
        }

        public static TAttribute GetAttribute<TAttribute>(Type type) where TAttribute : Attribute {
            Attribute[] attrs = GetTypeAttributes(type);
            return (TAttribute)attrs.FirstOrDefault(a => typeof(TAttribute).IsAssignableFrom(a.GetType()));
        }

        static Attribute[] GetTypeAttributes(Type type) {
            return typeAttributeCache.GetOrAdd(type, t => t.GetCustomAttributes().ToArray());
        }

        static PropertyInfo[] GetTypeProperties(Type type) {
            return propertyCache.GetOrAdd(type, t => t.GetProperties().ToArray());
        }

        static Attribute[] GetPropertyAttributes(PropertyInfo propertyInfo) {
            return propertyAttributeCache.GetOrAdd(propertyInfo, t => t.GetCustomAttributes().ToArray());
        }

        public static int GetObjectID(object obj) {
            PropertyInfo keyProperty = GetKeyProperty(obj.GetType());
            if (keyProperty != null) {
                return (int)keyProperty.GetValue(obj);
            } else {
                return GetPropertyValue<int>(obj, "ID", -1);
            }
        }

        static PropertyInfo GetKeyProperty(Type objectType) {
            return GetTypeProperties(objectType).FirstOrDefault(t => HasAttribute(t, typeof(KeyAttribute)));
        }

        public static Type GetEditorFormType(Type objectType, out int outWidth, out int outHeight, out string outTitle, out bool? outCenterToParent) {
            var attr = GetAttribute<EditorFormAttribute>(objectType);
            if (attr == null) {
                outWidth = 0;
                outHeight = 0;
                outTitle = null;
                outCenterToParent = null;
                return null;
            }
            outWidth = attr.Width;
            outHeight = attr.Height;
            outTitle = attr.Title;
            outCenterToParent = attr.CenterToParent;
            if (attr.FormType != null) {
                return attr.FormType;
            } else {
                if (attr.FormTypeName != null) {
                    return Type.GetType(attr.FormTypeName, true);
                } else {
                    return null;
                }
            }
        }

        public static Type GetListFormType(Type objectType, out int outWidth, out int outHeight, out string outTitle) {
            var attr = GetAttribute<ListFormAttribute>(objectType);
            if (attr == null) {
                outWidth = 0;
                outHeight = 0;
                outTitle = null;
                return null;
            }
            outWidth = attr.Width;
            outHeight = attr.Height;
            outTitle = attr.Title;
            if (attr.FormType != null) {
                return attr.FormType;
            } else {
                if (attr.FormTypeName != null) {
                    return Type.GetType(attr.FormTypeName, true);
                } else {
                    return null;
                }
            }
        }

        public static T GetPropertyValue<T>(object obj, string propertyName, T defaultValue) {
            if (obj == null) {
                return defaultValue;
            }
            PropertyInfo pi = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pi == null) {
                return defaultValue;
            }
            return (T)pi.GetValue(obj);
        }
        
        public static string FormatPropertyValue(PropertyInfo property, object propertyValue) {
            if (propertyValue == null) {
                return "";
            }
            Func<object, string> formatter;
            if (!propertyFormatMethodsCache.TryGetValue(property, out formatter)) {
                string format = ReflectionHelper.GetPropertyDisplayFormat(property);
                if (!string.IsNullOrEmpty(format)) {
                    MethodInfo mi = property.PropertyType.GetMethod("ToString", new Type[] { typeof(string) });
                    if (mi != null) {
                        formatter = (obj) => {
                            return (string)mi.Invoke(obj, new object[] { format });
                        };
                    }
                }
                if (formatter == null) {
                    formatter = (obj) => {
                        return obj.ToString();
                    };
                }
                propertyFormatMethodsCache[property] = formatter;
            }
            return formatter(propertyValue);
        }

        public static string GetDefaultStringRepresentation(object obj) {
            if (obj == null) {
                return "(нет)";
            }
            PropertyInfo nameProperty = GetDefaultProperty(obj.GetType());
            if (nameProperty != null) {
                return nameProperty.GetValue(obj)?.ToString();
            } else {
                PropertyInfo keyProperty = GetKeyProperty(obj.GetType());
                string keyValue;
                string typeName = GetTypeName(obj);
                if (keyProperty != null) {
                    keyValue = FormatPropertyValue(keyProperty, keyProperty.GetValue(obj));
                } else {
                    keyValue = "";
                }
                return $"{typeName} {keyValue}";
            }
        }

        public static PropertyInfo GetDefaultProperty(Type type) {
            return type.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
        }
    }
}
