using System.Collections.Generic;
using UnityEngine;

namespace Sanctuary.Blackboard
{
    [CreateAssetMenu(fileName = "New Blackboard Data", menuName = "Sanctuary/Blackboard/Blackboard Data")]
    public class BlackboardData : ScriptableObject
    {
        public List<BlackboardEntryData> entries = new();

        public void SetValuesOnBlackboard(Blackboard blackboard) => entries.ForEach(entry => entry.SetValueOnBlackboard(blackboard));
    }
}
