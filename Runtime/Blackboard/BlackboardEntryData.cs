using System;
using System.Collections.Generic;
using UnityEngine;
using ValueType = Sanctuary.Blackboard.AnyValue.ValueType;

namespace Sanctuary.Blackboard
{
    [Serializable]
    public class BlackboardEntryData : ISerializationCallbackReceiver
    {
        public string keyName;
        public ValueType valueType;
        public AnyValue value;

        // Dispatch table to set different types of value on the blackboard
        protected static readonly Dictionary<ValueType, Action<Blackboard, BlackboardKey, AnyValue>> setValueDispatchTable = new() 
        {
            { ValueType.Int, (blackboard, key, anyValue) => blackboard.SetValue<int>(key, anyValue) },
            { ValueType.Float, (blackboard, key, anyValue) => blackboard.SetValue<float>(key, anyValue) },
            { ValueType.Bool, (blackboard, key, anyValue) => blackboard.SetValue<bool>(key, anyValue) },
            { ValueType.String, (blackboard, key, anyValue) => blackboard.SetValue<string>(key, anyValue) },
            { ValueType.Vector3, (blackboard, key, anyValue) => blackboard.SetValue<Vector3>(key, anyValue) },
            { ValueType.Object, (blackboard, key, anyValue) => blackboard.SetValue<UnityEngine.Object>(key, anyValue) }
        };

        public void SetValueOnBlackboard(Blackboard blackboard) => setValueDispatchTable[value.type](blackboard, blackboard.GetOrAddKey(keyName), value);

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() => value.type = valueType;
    }
}
