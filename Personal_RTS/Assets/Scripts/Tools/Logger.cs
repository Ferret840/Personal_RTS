using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tools
{

    public class CustomLogger
    {
        public CustomLogger(string _logFilePath)
        {
            if (!_logFilePath.EndsWith(".log"))
                _logFilePath += ".log";
            LogFilePath = _logFilePath;
            if (!File.Exists(LogFilePath))
                File.Create(LogFilePath).Close();
            WriteLine("New Session Started");
        }

        public string LogFilePath
        {
            get; private set;
        }

        public void WriteLine(object _message)
        {
            using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                writer.WriteLine(DateTime.Now.ToString() + ": " + _message.ToString());
        }

        public static string StringifyInvalidProperties_s(object _obj)
        {
            string all = "";
            all += string.Join("", _obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(
                field => field.GetValue(_obj).Equals(-1) ? "Int " + field.Name + ": " + field.GetValue(_obj) + "\n"
                       : field.GetValue(_obj).Equals(-1f) ? "Float " + field.Name + ": " + field.GetValue(_obj) + "\n"
                       : field.GetValue(_obj).Equals((double)-1) ? "double " + field.Name + ": " + field.GetValue(_obj) + "\n"
                       : field.GetValue(_obj).GetType().IsEnum && field.GetValue(_obj).Equals(Enum.ToObject(field.GetValue(_obj).GetType(), -1)) ? "Enum " + field.Name + ": " + field.GetValue(_obj) + "\n"
                       : field.GetValue(_obj).Equals("") ? "String " + field.Name + ": " + field.GetValue(_obj) + "\n"
                       : ""
                ));
            all += string.Join("", _obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(
                prop => prop.GetValue(_obj).Equals(-1) ? "Int " + prop.Name + ": " + prop.GetValue(_obj) + "\n"
                      : prop.GetValue(_obj).Equals(-1f) ? "Float " + prop.Name + ": " + prop.GetValue(_obj) + "\n"
                      : prop.GetValue(_obj).Equals((double)-1) ? "double " + prop.Name + ": " + prop.GetValue(_obj) + "\n"
                      : prop.GetValue(_obj).GetType().IsEnum && prop.GetValue(_obj).Equals(Enum.ToObject(prop.GetValue(_obj).GetType(), -1)) ? "Enum " + prop.Name + ": " + prop.GetValue(_obj) + "\n"
                      : prop.GetValue(_obj).Equals("") ? "String " + prop.Name + ": " + prop.GetValue(_obj) + "\n"
                      : prop.GetValue(_obj).GetType().IsArray ? StringifyInvalidProperties_s(_obj)
                      : ""
                ));

            foreach (var field in _obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetValue(_obj).GetType().IsGenericType && field.GetValue(_obj).GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    Type keyType = field.GetValue(_obj).GetType().GetGenericArguments()[0];
                    Type valueType = field.GetValue(_obj).GetType().GetGenericArguments()[1];
                    foreach (DictionaryEntry kvp in (IDictionary)field.GetValue(_obj))
                    {
                        if (kvp.Value.Equals(-1)
                            || kvp.Value.Equals(-1f)
                            || kvp.Value.Equals((double)-1)
                            || (kvp.Value.GetType().IsEnum && kvp.Value.Equals(Enum.ToObject(kvp.Value.GetType(), -1)))
                            || kvp.Value.Equals(""))
                            all += string.Format("Key = {0}, Value = {1}\n", kvp.Key, kvp.Value);
                    }
                }
            }
            foreach (var prop in _obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (prop.GetValue(_obj).GetType().IsGenericType && prop.GetValue(_obj).GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    Type keyType = prop.GetValue(_obj).GetType().GetGenericArguments()[0];
                    Type valueType = prop.GetValue(_obj).GetType().GetGenericArguments()[1];
                    foreach (DictionaryEntry kvp in (IDictionary)prop.GetValue(_obj))
                    {
                        if(kvp.Value.Equals(-1)
                            || kvp.Value.Equals(-1f)
                            || kvp.Value.Equals((double)-1)
                            || (kvp.Value.GetType().IsEnum && kvp.Value.Equals(Enum.ToObject(kvp.Value.GetType(), -1)))
                            || kvp.Value.Equals(""))
                            all += string.Format("Key = {0}, Value = {1}\n", kvp.Key, kvp.Value);
                    }
                }
            }

            return all;
        }

        public static void LogAllProperties_s(object _obj, CustomLogger _logger = null)
        {
            string all = "";
            all += StringifyInvalidProperties_s(_obj);

            if (all != "")
            {
                if (_logger != null)
                {
                    _logger.WriteLine(all);
                }
                else
                {
                    Debug.Log(all);
                }
            }
        }
    }

}