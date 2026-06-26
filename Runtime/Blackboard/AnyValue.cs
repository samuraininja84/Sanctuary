using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sanctuary.Blackboard
{
    [Serializable]
    public struct AnyValue
    {
        public ValueType type;

        // Storage for different types of values
        // Add more types as needed, but remember to add them to the dispatch table above and the custom Editor
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue;
        public Vector3 vector3Value;
        public Object objectValue;

        // Implicit conversion operators to convert AnyValue to different types
        public static implicit operator int(AnyValue value) => value.ConvertValue<int>();
        public static implicit operator float(AnyValue value) => value.ConvertValue<float>();
        public static implicit operator bool(AnyValue value) => value.ConvertValue<bool>();
        public static implicit operator string(AnyValue value) => value.ConvertValue<string>();
        public static implicit operator Vector3(AnyValue value) => value.ConvertValue<Vector3>();
        public static implicit operator Object(AnyValue value) => value.ConvertValue<Object>();

        // Helper methods for safe type conversions of the value types without the cost of boxing
        private readonly T AsBool<T>(bool value) => typeof(T) == typeof(bool) && value is T correctType ? correctType : default;
        private readonly T AsInt<T>(int value) => typeof(T) == typeof(int) && value is T correctType ? correctType : default;
        private readonly T AsFloat<T>(float value) => typeof(T) == typeof(float) && value is T correctType ? correctType : default;
        private readonly T AsVector3<T>(Vector3 value) => typeof(T) == typeof(Vector3) && value is T correctType ? correctType : default;
        private readonly T AsObject<T>(Object value) => typeof(T) == typeof(Object) && value is T correctType ? correctType : default;

        private readonly T ConvertValue<T>()
        {
            // Use a switch expression to determine the type of the stored value and convert it to the requested type
            return type switch
            {
                // Dispatch table for converting the stored value to the requested type
                ValueType.Int => AsInt<T>(intValue),
                ValueType.Float => AsFloat<T>(floatValue),
                ValueType.Bool => AsBool<T>(boolValue),
                ValueType.String => (T)(object)stringValue,
                ValueType.Vector3 => AsVector3<T>(vector3Value),
                ValueType.Object => AsObject<T>(objectValue),

                // Handle unsupported types
                _ => throw new NotSupportedException($"Not supported value type: {typeof(T)}")
            };
        }

        public enum ValueType 
        { 
            Int, 
            Float, 
            Bool, 
            String, 
            Vector3,
            Object
        }
    }
}
