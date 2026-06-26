using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sanctuary.Attributes 
{
    public class EvaluatedAssembliesProcessor : ScriptableWizard
    {
        [Header("Completion Evaluation")]
        public float evaluation = 0f;
        public float percentage = 0f;

        [Header("Evaluated Assemblies")]
        public List<string> evaluatedAssemblies = new();

        [MenuItem("Tools/Sanctuary//Evaluated Assemblies Reference")]
        private static void CreateWizard() => DisplayWizard("Evaluated Assemblies Processor", typeof(EvaluatedAssembliesProcessor));

        public void OnWizardCreate()
        {
            // Get the completion evaluation for the assembly
            evaluation = CompletionExtensions.GetCompletionEvaluation();

            // Scale the evaluation to a percentage
            percentage = evaluation.ScaleToPercentage();

            // Log the completion percentage and the evaluated assemblies
            Debug.Log($"Completion Evaluation: {percentage}% ({evaluation}) in {string.Join(", ", evaluatedAssemblies)}");
        }

        private void Reset() => evaluatedAssemblies = CompletionExtensions.GetEvaluatedAssemblies().Select(assembly => assembly.GetName().Name).ToList();
    }
}
