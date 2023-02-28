﻿using System;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace VM.Common
{
    public partial class ReflectionUtils
    {

        /// <summary>
        /// Binding Flags constant to be reused for all Reflection access methods.
        /// </summary>
        public const BindingFlags MemberAccess =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;


        /// <summary>
        /// Retrieve a property value from an object dynamically. This is a simple version
        /// that uses Reflection calls directly. It doesn't support indexers.
        /// </summary>
        /// <param name="instance">Object to make the call on</param>
        /// <param name="property">Property to retrieve</param>
        /// <returns>Object - cast to proper type</returns>
        public static object GetProperty(object instance, string property)
        {
            return instance.GetType().GetProperty(property, ReflectionUtils.MemberAccess).GetValue(instance, null);
        }

        /// <summary>
        /// Retrieve a field dynamically from an object. This is a simple implementation that's
        /// straight Reflection and doesn't support indexers.
        /// </summary>
        /// <param name="Object">Object to retreve Field from</param>
        /// <param name="Property">name of the field to retrieve</param>
        /// <returns></returns>
        public static object GetField(object Object, string Property)
        {
            return Object.GetType().GetField(Property, ReflectionUtils.MemberAccess).GetValue(Object);
        }

        /// <summary>
        /// Parses Properties and Fields including Array and Collection references.
        /// Used internally for the 'Ex' Reflection methods.
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="Property"></param>
        /// <returns></returns>
        private static object GetPropertyInternal(object Parent, string Property)
        {
            if (Property == "this" || Property == "me")
                return Parent;

            object Result = null;
            string PureProperty = Property;
            string Indexes = null;
            bool IsArrayOrCollection = false;

            // Deal with Array Property
            if (Property.IndexOf("[") > -1)
            {
                PureProperty = Property.Substring(0, Property.IndexOf("["));
                Indexes = Property.Substring(Property.IndexOf("["));
                IsArrayOrCollection = true;
            }

            // Get the member
            MemberInfo Member = Parent.GetType().GetMember(PureProperty, ReflectionUtils.MemberAccess)[0];
            if (Member.MemberType == MemberTypes.Property)
                Result = ((PropertyInfo)Member).GetValue(Parent, null);
            else
                Result = ((FieldInfo)Member).GetValue(Parent);

            if (IsArrayOrCollection)
            {
                Indexes = Indexes.Replace("[", string.Empty).Replace("]", string.Empty);

                if (Result is Array)
                {
                    int Index = -1;
                    int.TryParse(Indexes, out Index);
                    Result = CallMethod(Result, "GetValue", Index);
                }
                else if (Result is ICollection)
                {
                    if (Indexes.StartsWith("\""))
                    {
                        // String Index
                        Indexes = Indexes.Trim('\"');
                        Result = CallMethod(Result, "get_Item", Indexes);
                    }
                    else
                    {
                        // assume numeric index
                        int Index = -1;
                        int.TryParse(Indexes, out Index);
                        Result = CallMethod(Result, "get_Item", Index);
                    }
                }

            }

            return Result;
        }

        /// <summary>
        /// Parses Properties and Fields including Array and Collection references.
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="Property"></param>
        /// <returns></returns>
        private static object SetPropertyInternal(object Parent, string Property, object Value)
        {
            if (Property == "this" || Property == "me")
                return Parent;

            object Result = null;
            string PureProperty = Property;
            string Indexes = null;
            bool IsArrayOrCollection = false;

            // Deal with Array Property
            if (Property.IndexOf("[") > -1)
            {
                PureProperty = Property.Substring(0, Property.IndexOf("["));
                Indexes = Property.Substring(Property.IndexOf("["));
                IsArrayOrCollection = true;
            }

            if (!IsArrayOrCollection)
            {
                // Get the member
                MemberInfo Member = Parent.GetType().GetMember(PureProperty, ReflectionUtils.MemberAccess)[0];
                if (Member.MemberType == MemberTypes.Property)
                    ((PropertyInfo)Member).SetValue(Parent, Value, null);
                else
                    ((FieldInfo)Member).SetValue(Parent, Value);
                return null;
            }
            else
            {
                // Get the member
                MemberInfo Member = Parent.GetType().GetMember(PureProperty, ReflectionUtils.MemberAccess)[0];
                if (Member.MemberType == MemberTypes.Property)
                    Result = ((PropertyInfo)Member).GetValue(Parent, null);
                else
                    Result = ((FieldInfo)Member).GetValue(Parent);
            }
            if (IsArrayOrCollection)
            {
                Indexes = Indexes.Replace("[", string.Empty).Replace("]", string.Empty);

                if (Result is Array)
                {
                    int Index = -1;
                    int.TryParse(Indexes, out Index);
                    Result = CallMethod(Result, "SetValue", Value, Index);
                }
                else if (Result is ICollection)
                {
                    if (Indexes.StartsWith("\""))
                    {
                        // String Index
                        Indexes = Indexes.Trim('\"');
                        Result = CallMethod(Result, "set_Item", Indexes, Value);
                    }
                    else
                    {
                        // assume numeric index
                        int Index = -1;
                        int.TryParse(Indexes, out Index);
                        Result = CallMethod(Result, "set_Item", Index, Value);
                    }
                }
            }

            return Result;
        }

        /// <summary>
        /// Returns a property or field value using a base object and sub members including . syntax.
        /// For example, you can access: oCustomer.oData.Company with (this,"oCustomer.oData.Company")
        /// This method also supports indexers in the Property value such as:
        /// Customer.DataSet.Tables["Customers"].Rows[0]
        /// </summary>
        /// <param name="Parent">Parent object to 'start' parsing from. Typically this will be the Page.</param>
        /// <param name="Property">The property to retrieve. Example: 'Customer.Entity.Company'</param>
        /// <returns></returns>
        public static object GetPropertyEx(object Parent, string Property)
        {
            Type Type = Parent.GetType();

            int lnAt = Property.IndexOf(".");
            if (lnAt < 0)
            {
                // Complex parse of the property    
                return GetPropertyInternal(Parent, Property);
            }

            // Walk the . syntax - split into current object (Main) and further parsed objects (Subs)
            string Main = Property.Substring(0, lnAt);
            string Subs = Property.Substring(lnAt + 1);

            // Retrieve the next . section of the property
            object Sub = GetPropertyInternal(Parent, Main);

            // Now go parse the left over sections
            return GetPropertyEx(Sub, Subs);
        }

        /// <summary>
        /// Sets the property on an object. This is a simple method that uses straight Reflection 
        /// and doesn't support indexers.
        /// </summary>
        /// <param name="Object">Object to set property on</param>
        /// <param name="Property">Name of the property to set</param>
        /// <param name="Value">value to set it to</param>
        public static void SetProperty(object Object, string Property, object Value)
        {
            Object.GetType().GetProperty(Property, ReflectionUtils.MemberAccess).SetValue(Object, Value, null);
        }

        /// <summary>
        /// Sets the field on an object. This is a simple method that uses straight Reflection 
        /// and doesn't support indexers.
        /// </summary>
        /// <param name="Object">Object to set property on</param>
        /// <param name="Property">Name of the field to set</param>
        /// <param name="Value">value to set it to</param>
        public static void SetField(object Object, string Property, object Value)
        {
            Object.GetType().GetField(Property, ReflectionUtils.MemberAccess).SetValue(Object, Value);
        }

        /// <summary>
        /// Sets a value on an object. Supports . syntax for named properties
        /// (ie. Customer.Entity.Company) as well as indexers.
        /// </summary>
        /// <param name="Object Parent">
        /// Object to set the property on.
        /// </param>
        /// <param name="String Property">
        /// Property to set. Can be an object hierarchy with . syntax and can 
        /// include indexers. Examples: Customer.Entity.Company, 
        /// Customer.DataSet.Tables["Customers"].Rows[0]
        /// </param>
        /// <param name="Object Value">
        /// Value to set the property to
        /// </param>
        public static object SetPropertyEx(object parent, string property, object value)
        {
            Type Type = parent.GetType();

            // no more .s - we got our final object
            int lnAt = property.IndexOf(".");
            if (lnAt < 0)
            {
                SetPropertyInternal(parent, property, value);
                return null;
            }

            // Walk the . syntax
            string Main = property.Substring(0, lnAt);
            string Subs = property.Substring(lnAt + 1);

            object Sub = GetPropertyInternal(parent, Main);

            SetPropertyEx(Sub, Subs, value);

            return null;
        }

        /// <summary>
        /// Calls a method on an object dynamically. This version requires explicit
        /// specification of the parameter type signatures.
        /// </summary>
        /// <param name="instance">Instance of object to call method on</param>
        /// <param name="method">The method to call as a stringToTypedValue</param>
        /// <param name="parameterTypes">Specify each of the types for each parameter passed. 
        /// You can also pass null, but you may get errors for ambiguous methods signatures
        /// when null parameters are passed</param>
        /// <param name="parms">any variable number of parameters.</param>        
        /// <returns>object</returns>
        public static object CallMethod(object instance, string method, Type[] parameterTypes, params object[] parms)
        {
            if (parameterTypes == null && parms.Length > 0)
                // Call without explicit parameter types - might cause problems with overloads    
                // occurs when null parameters were passed and we couldn't figure out the parm type
                return instance.GetType().GetMethod(method, ReflectionUtils.MemberAccess | BindingFlags.InvokeMethod).Invoke(instance, parms);
            else
                // Call with parameter types - works only if no null values were passed
                return instance.GetType().GetMethod(method, ReflectionUtils.MemberAccess | BindingFlags.InvokeMethod, null, parameterTypes, null).Invoke(instance, parms);
        }

        /// <summary>
        /// Calls a method on an object dynamically. 
        /// 
        /// This version doesn't require specific parameter signatures to be passed. 
        /// Instead parameter types are inferred based on types passed. Note that if 
        /// you pass a null parameter, type inferrance cannot occur and if overloads
        /// exist the call may fail. if so use the more detailed overload of this method.
        /// </summary> 
        /// <param name="instance">Instance of object to call method on</param>
        /// <param name="method">The method to call as a stringToTypedValue</param>
        /// <param name="parameterTypes">Specify each of the types for each parameter passed. 
        /// You can also pass null, but you may get errors for ambiguous methods signatures
        /// when null parameters are passed</param>
        /// <param name="parms">any variable number of parameters.</param>        
        /// <returns>object</returns>
        public static object CallMethod(object instance, string method, params object[] parms)
        {
            // Pick up parameter types so we can match the method properly
            Type[] parameterTypes = null;
            if (parms != null)
            {
                parameterTypes = new Type[parms.Length];
                for (int x = 0; x < parms.Length; x++)
                {
                    // if we have null parameters we can't determine parameter types - exit
                    if (parms[x] == null)
                    {
                        parameterTypes = null;  // clear out - don't use types        
                        break;
                    }
                    parameterTypes[x] = parms[x].GetType();
                }
            }
            return CallMethod(instance, method, parameterTypes, parms);
        }

        /// <summary>
        /// Calls a method on an object with extended . syntax (object: this Method: Entity.CalculateOrderTotal)
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="method"></param>
        /// <param name="params"></param>
        /// <returns></returns>
        public static object CallMethodEx(object parent, string method, params object[] parms)
        {
            Type Type = parent.GetType();

            // no more .s - we got our final object
            int lnAt = method.IndexOf(".");
            if (lnAt < 0)
            {
                return ReflectionUtils.CallMethod(parent, method, parms);
            }

            // Walk the . syntax
            string Main = method.Substring(0, lnAt);
            string Subs = method.Substring(lnAt + 1);

            object Sub = GetPropertyInternal(parent, Main);

            // Recurse until we get the lowest ref
            return CallMethodEx(Sub, Subs, parms);
        }



        /// <summary>
        /// Creates an instance from a type by calling the parameterless constructor.
        /// 
        /// Note this will not work with COM objects - continue to use the Activator.CreateInstance
        /// for COM objects.
        /// <seealso>Class wwUtils</seealso>
        /// </summary>
        /// <param name="typeToCreate">
        /// The type from which to create an instance.
        /// </param>
        /// <returns>object</returns>
        public static object CreateInstanceFromType(Type typeToCreate, params object[] args)
        {
            if (args == null)
            {
                Type[] Parms = Type.EmptyTypes;
                return typeToCreate.GetConstructor(Parms).Invoke(null);
            }

            return Activator.CreateInstance(typeToCreate, args);
        }




        /// <summary>
        /// Creates an instance of a type based on a string. Assumes that the type's
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object CreateInstanceFromString(string typeName, params object[] args)
        {
            object instance = null;
            Type type = null;

            try
            {
                type = GetTypeFromName(typeName);
                if (type == null)
                    return null;

                instance = Activator.CreateInstance(type, args);
            }
            catch
            {
                return null;
            }

            return instance;
        }

        /// <summary>
        /// Helper routine that looks up a type name and tries to retrieve the
        /// full type reference in the actively executing assemblies.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetTypeFromName(string typeName)
        {

            Type type = null;

            // try to find manually
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = ass.GetType(typeName, false);

                if (type != null)
                    break;

            }
            return type;
        }


        /// <summary>
        /// Creates a COM instance from a ProgID. Loads either
        /// Exe or DLL servers.
        /// </summary>
        /// <param name="progId"></param>
        /// <returns></returns>
        public static object CreateComInstance(string progId)
        {
            Type type = Type.GetTypeFromProgID(progId);
            if (type == null)
                return null;

            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Converts a type to string if possible. This method supports an optional culture generically on any value.
        /// It calls the ToString() method on common types and uses a type converter on all other objects
        /// if available
        /// </summary>
        /// <param name="rawValue">The Value or Object to convert to a string</param>
        /// <param name="culture">Culture for numeric and DateTime values</param>
        /// <returns>string</returns>
        public static string TypedValueToString(object rawValue, CultureInfo culture)
        {
            Type ValueType = rawValue.GetType();
            string Return = null;

            if (ValueType == typeof(string))
                Return = rawValue.ToString();
            else if (ValueType == typeof(int) || ValueType == typeof(decimal) ||
                ValueType == typeof(double) || ValueType == typeof(float))
                Return = string.Format(culture.NumberFormat, "{0}", rawValue);
            else if (ValueType == typeof(DateTime))
                Return = string.Format(culture.DateTimeFormat, "{0}", rawValue);
            else if (ValueType == typeof(bool))
                Return = rawValue.ToString();
            else if (ValueType == typeof(byte))
                Return = rawValue.ToString();
            else if (ValueType.IsEnum)
                Return = rawValue.ToString();
            else
            {
                // Any type that supports a type converter
                TypeConverter converter = TypeDescriptor.GetConverter(ValueType);
                if (converter != null && converter.CanConvertTo(typeof(string)))
                    Return = converter.ConvertToString(null, culture, rawValue);
                else
                    // Last resort - just call ToString() on unknown type
                    Return = rawValue.ToString();
            }

            return Return;
        }

        /// <summary>
        /// Converts a type to string if possible. This method uses the current culture for numeric and DateTime values.
        /// It calls the ToString() method on common types and uses a type converter on all other objects
        /// if available.
        /// </summary>
        /// <param name="rawValue">The Value or Object to convert to a string</param>
        /// <param name="Culture">Culture for numeric and DateTime values</param>
        /// <returns>string</returns>
        public static string TypedValueToString(object rawValue)
        {
            return TypedValueToString(rawValue, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Turns a string into a typed value. Useful for auto-conversion routines
        /// like form variable or XML parsers.
        /// <seealso>Class wwUtils</seealso>
        /// </summary>
        /// <param name="sourceString">
        /// The string to convert from
        /// </param>
        /// <param name="targetType">
        /// The type to convert to
        /// </param>
        /// <param name="culture">
        /// Culture used for numeric and datetime values.
        /// </param>
        /// <returns>object. Throws exception if it cannot be converted.</returns>
        public static object StringToTypedValue(string sourceString, Type targetType, CultureInfo culture)
        {
            object Result = null;

            bool isEmpty = false;
            if (sourceString == string.Empty)
                isEmpty = true;

            if (targetType == typeof(string))
                Result = sourceString;
            else if (targetType == typeof(Int32) || targetType == typeof(int))
            {
                if (isEmpty)
                    Result = 0;
                else
                    Result = Int32.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(Int64))
            {
                if (isEmpty)
                    Result = (Int64)0;
                else
                    Result = Int64.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(Int16))
            {
                if (isEmpty)
                    Result = (Int16)0;
                else
                    Result = Int16.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(decimal))
            {
                if (isEmpty)
                    Result = 0M;
                else
                    Result = decimal.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(DateTime))
            {
                if (isEmpty)
                    Result = DateTime.MinValue;
                else
                    Result = Convert.ToDateTime(sourceString, culture.DateTimeFormat);
            }
            else if (targetType == typeof(byte))
            {
                if (isEmpty)
                    Result = 0;
                else
                    Result = Convert.ToByte(sourceString);
            }
            else if (targetType == typeof(double))
            {
                if (isEmpty)
                    Result = 0F;
                else
                    Result = Double.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(Single))
            {
                if (isEmpty)
                    Result = 0F;
                else
                    Result = Single.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(bool))
            {
                if (sourceString.ToLower() == "true" || sourceString.ToLower() == "on" || sourceString == "1")
                    Result = true;
                else
                    Result = false;
            }
            else if (targetType.IsEnum)
                Result = Enum.Parse(targetType, sourceString);
            else if (targetType == typeof(byte[]))
            {
                // TODO: Convert HexBinary string to byte array
                Result = null;
            }
            // Handle nullables explicitly since type converter won't handle conversions
            // properly for things like decimal separators currency formats etc.
            // Grab underlying type and pass value to that
            else if (targetType.Name.StartsWith("Nullable`"))
            {
                if (sourceString.ToLower() == "null")
                    Result = null;
                else
                {
                    targetType = Nullable.GetUnderlyingType(targetType);
                    Result = StringToTypedValue(sourceString, targetType);
                }
            }
            else
            {
                TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                    Result = converter.ConvertFromString(null, culture, sourceString);
                else
                {
                    Debug.Assert(false, "Type Conversion not handled in StringToTypedValue for " +
                                                    targetType.Name + " " + sourceString);
                    throw (new ApplicationException("Type Conversion not handled in StringToTypedValue for " + targetType.Name));
                }
            }

            return Result;
        }

        /// <summary>
        /// Turns a string into a typed value. Useful for auto-conversion routines
        /// like form variable or XML parsers.
        /// </summary>
        /// <param name="sourceString">The input string to convert</param>
        /// <param name="targetType">The Type to convert it to</param>
        /// <returns>object reference. Throws Exception if type can not be converted</returns>
        public static object StringToTypedValue(string sourceString, Type targetType)
        {
            return StringToTypedValue(sourceString, targetType, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a List of KeyValuePair object
        /// </summary>
        /// <param name="enumeration"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> GetEnumList(Type enumType)
        {
            string[] enumStrings = Enum.GetNames(enumType);
            List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
            foreach (string enumString in enumStrings)
            {
                items.Add(new KeyValuePair<string, string>(enumString, StringHelper.FromCamelCase(enumString)));
            }
            return items;
        }

        #region COM Reflection Routines

        /// <summary>
        /// Retrieve a dynamic 'non-typelib' property
        /// </summary>
        /// <param name="instance">Object to make the call on</param>
        /// <param name="property">Property to retrieve</param>
        /// <returns></returns>
        public static object GetPropertyCom(object instance, string property)
        {
            return instance.GetType().InvokeMember(property, ReflectionUtils.MemberAccess | BindingFlags.GetProperty | BindingFlags.GetField, null,
                                                instance, null);
        }


        /// <summary>
        /// Returns a property or field value using a base object and sub members including . syntax.
        /// For example, you can access: oCustomer.oData.Company with (this,"oCustomer.oData.Company")
        /// </summary>
        /// <param name="parent">Parent object to 'start' parsing from.</param>
        /// <param name="property">The property to retrieve. Example: 'oBus.oData.Company'</param>
        /// <returns></returns>
        public static object GetPropertyExCom(object parent, string property)
        {

            Type Type = parent.GetType();

            int lnAt = property.IndexOf(".");
            if (lnAt < 0)
            {
                if (property == "this" || property == "me")
                    return parent;

                // Get the member
                return parent.GetType().InvokeMember(property, ReflectionUtils.MemberAccess | BindingFlags.GetProperty | BindingFlags.GetField, null,
                    parent, null);
            }

            // Walk the . syntax - split into current object (Main) and further parsed objects (Subs)
            string Main = property.Substring(0, lnAt);
            string Subs = property.Substring(lnAt + 1);

            object Sub = parent.GetType().InvokeMember(Main, ReflectionUtils.MemberAccess | BindingFlags.GetProperty | BindingFlags.GetField, null,
                parent, null);

            // Recurse further into the sub-properties (Subs)
            return ReflectionUtils.GetPropertyExCom(Sub, Subs);
        }

        /// <summary>
        /// Sets the property on an object.
        /// </summary>
        /// <param name="Object">Object to set property on</param>
        /// <param name="Property">Name of the property to set</param>
        /// <param name="Value">value to set it to</param>
        public static void SetPropertyCom(object Object, string Property, object Value)
        {
            Object.GetType().InvokeMember(Property, ReflectionUtils.MemberAccess | BindingFlags.SetProperty | BindingFlags.SetField, null, Object, new object[1] { Value });
            //GetProperty(Property,ReflectionUtils.MemberAccess).SetValue(Object,Value,null);
        }

        /// <summary>
        /// Sets the value of a field or property via Reflection. This method alws 
        /// for using '.' syntax to specify objects multiple levels down.
        /// 
        /// ReflectionUtils.SetPropertyEx(this,"Invoice.LineItemsCount",10)
        /// 
        /// which would be equivalent of:
        /// 
        /// Invoice.LineItemsCount = 10;
        /// </summary>
        /// <param name="Object Parent">
        /// Object to set the property on.
        /// </param>
        /// <param name="String Property">
        /// Property to set. Can be an object hierarchy with . syntax.
        /// </param>
        /// <param name="Object Value">
        /// Value to set the property to
        /// </param>
        public static object SetPropertyExCom(object parent, string property, object value)
        {
            Type Type = parent.GetType();

            int lnAt = property.IndexOf(".");
            if (lnAt < 0)
            {
                // Set the member
                parent.GetType().InvokeMember(property, ReflectionUtils.MemberAccess | BindingFlags.SetProperty | BindingFlags.SetField, null,
                    parent, new object[1] { value });

                return null;
            }

            // Walk the . syntax - split into current object (Main) and further parsed objects (Subs)
            string Main = property.Substring(0, lnAt);
            string Subs = property.Substring(lnAt + 1);


            object Sub = parent.GetType().InvokeMember(Main, ReflectionUtils.MemberAccess | BindingFlags.GetProperty | BindingFlags.GetField, null,
                parent, null);

            return SetPropertyExCom(Sub, Subs, value);
        }


        /// <summary>
        /// Wrapper method to call a 'dynamic' (non-typelib) method
        /// on a COM object
        /// </summary>
        /// <param name="params"></param>
        /// 1st - Method name, 2nd - 1st parameter, 3rd - 2nd parm etc.
        /// <returns></returns>
        public static object CallMethodCom(object instance, string method, params object[] parms)
        {
            return instance.GetType().InvokeMember(method, ReflectionUtils.MemberAccess | BindingFlags.InvokeMethod, null, instance, parms);
        }
        #endregion

    }


    /// <summary>
    /// LateBoundMethod is a generic method signature that is passed an instance
    /// and an array of parameters and returns an object. It basically can be 
    /// used to call any method.
    /// 
    /// </summary>
    /// <param name="target">The instance that the dynamic method is called on</param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public delegate object LateBoundMethod(object target, object[] arguments);


    /// <summary>
    /// This class creates a generic method delegate from a MethodInfo signature
    /// converting the method call into a LateBoundMethod delegate call. Using
    /// this class allows making repeated calls very quickly.
    /// 
    /// Note: this class will be very inefficient for individual dynamic method
    /// calls - compilation of the expression is very expensive up front, so using
    /// this delegate factory makes sense only if you re-use the dynamicly loaded
    /// method repeatedly.
    /// 
    /// Entirely based on Nate Kohari's blog post:
    /// http://kohari.org/2009/03/06/fast-late-bound-invocation-with-expression-trees/
    /// </summary>
    public static class DelegateFactory
    {

        /// <summary>
        /// Creates a LateBoundMethod delegate from a MethodInfo structure
        /// Basically creates a dynamic delegate instance (code) on the fly.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static LateBoundMethod Create(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            MethodCallExpression call = Expression.Call(
                Expression.Convert(instanceParameter, method.DeclaringType),
                method,
                CreateParameterExpressions(method, argumentsParameter));

            Expression<LateBoundMethod> lambda = Expression.Lambda<LateBoundMethod>(
                Expression.Convert(call, typeof(object)),
                instanceParameter,
                argumentsParameter);

            return lambda.Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)),
                    parameter.ParameterType)).ToArray();
        }


        /// <summary>
        /// Creates a LateBoundMethod from type methodname and parameter signature that
        /// is turned into a MethodInfo structure and then parsed into a dynamic delegate
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        public static LateBoundMethod Create(Type type, string methodName, params Type[] parameterTypes)
        {
            return Create(type.GetMethod(methodName, parameterTypes));
        }

    }
}
