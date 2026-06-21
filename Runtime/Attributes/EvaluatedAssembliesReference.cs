using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Sanctuary.Attributes 
{
    [CreateAssetMenu(fileName = "Evaluated Assemblies Reference", menuName = "Sanctuary/Evaluated Assemblies Reference")]
    public class EvaluatedAssembliesReference : ScriptableObject
    {
        [Header("Completion Evaluation")]
        public float evaluation = 0f;
        public float percentage = 0f;

        [Header("Evaluated Assemblies")]
        public List<string> evaluatedAssemblies = new List<string>();

        [ContextMenu("Run Test")]
        public void RunTest()
        {
            // Get the completion evaluation for the assembly
            evaluation = CompletionExtensions.GetCompletionEvaluation();

            // Scale the evaluation to a percentage
            percentage = evaluation.ScaleToPercentage();
        }

        private void Reset() => evaluatedAssemblies = CompletionExtensions.GetEvaluatedAssemblies().Select(assembly => assembly.GetName().Name).ToList();
    }
}
