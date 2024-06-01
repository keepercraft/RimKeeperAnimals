using System;
using System.Reflection;
using Verse;

namespace Keepercraft.RimKeeperAnimals.Extensions
{
    public static class GenericExtension
    {
        public static void SetPrivateField(this object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
            else
            {
                Log.Error("[RimKeeperAnimals] SetPrivateField:" + fieldName);
            }
        }

        public static void SetPrivateProperty(this object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            PropertyInfo fieldInfo = type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
            else
            {
                Log.Error("[RimKeeperAnimals] SetPrivateProperty:" + fieldName);
            }
        }

        public static void SetPrivateStaticField(this object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
            else
            {
                Log.Error("[RimKeeperAnimals] SetPrivateStaticField:" + fieldName);
            }
        }

        public static T GetPrivateField<T>(this object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                return (T)fieldInfo.GetValue(obj);
            }
            else
            {
                Log.Error("[RimKeeperAnimals] GetPrivateField:" + fieldName);
                return default;
            }
        }

        public static T GetPrivateStaticField<T>(this object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);

            if (fieldInfo != null)
            {
                return (T)fieldInfo.GetValue(obj);
            }
            else
            {
                Log.Error("[RimKeeperAnimals] GetPrivateStaticField:" + fieldName);
                return default;
            }
        }

        public static T GetPrivateProperty<T>(this object obj, string fieldName)
        {
            Type type = obj.GetType();
            PropertyInfo fieldInfo = type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                return (T)fieldInfo.GetValue(obj);
            }
            else
            {
                Log.Error("[RimKeeperAnimals] GetPrivateProperty:" + fieldName);
                return default;
            }
        }

        public static T GetPrivateStaticProperty<T>(this object obj, string fieldName)
        {
            Type type = obj.GetType();
            PropertyInfo fieldInfo = type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Static);

            if (fieldInfo != null)
            {
                return (T)fieldInfo.GetValue(obj);
            }
            else
            {
                Log.Error("[RimKeeperAnimals] GetPrivateProperty:" + fieldName);
                return default;
            }
        }

        public static T GetPrivateMethod<T>(this object obj, string methodName, params object[] methodParams)
        {
            Type type = obj.GetType();

            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo != null)
            {
                return (T)methodInfo.Invoke(obj, methodParams);
            }
            else
            {
                Log.Error("[RimKeeperAnimals] GetPrivateMethod:" + methodName);
                return default;
            }
        }
    }
}
