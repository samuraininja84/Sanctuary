using System;
using System.Linq;
using System.Collections.Generic;

using Assembly = System.Reflection.Assembly;
using MethodInfo = System.Reflection.MethodInfo;
using BindingFlags = System.Reflection.BindingFlags;

namespace Sanctuary.Attributes 
{
    public class CompletionEvaluationAttribute : Attribute { }

    public static class CompletionExtensions
    {
        private static IEnumerable<Assembly> assemblies = null;
        private static IEnumerable<MethodInfo> methods = null;

        private static BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly HashSet<string> internalAssemblyPrefixes = new()
        {
            "Unity.",
            "UnityEditor.",
            "UnityEngine.",
            "JetBrains.",
            "System.",
            "Microsoft.",
            "Mono.",
            "ICSharpCode.",
            "Newtonsoft."
        };

        private static readonly HashSet<string> internalAssemblyNames = new()
        {
            "Bee.BeeDriver",
            "ExCSS.Unity",
            "Mono.Security",
            "mscorlib",
            "netstandard",
            "Newtonsoft.Json",
            "nunit.framework",
            "ReportGeneratorMerged",
            "Unrelated",
            "SyntaxTree.VisualStudio.Unity.Bridge",
            "SyntaxTree.VisualStudio.Unity.Messaging"
        };

        public static IEnumerable<Assembly> GetUserCreatedAssemblies(this AppDomain appDomain)
        {
            // Iterate through all assemblies in the AppDomain
            foreach (var assembly in appDomain.GetAssemblies())
            {
                // Skip dynamic assemblies
                if (assembly.IsDynamic) continue;

                // Get the assembly name
                string assemblyName = assembly.GetName().Name;

                // Skip editor assemblies
                if (assemblyName.Contains("Editor")) continue;

                // Skip internal/system assemblies by prefix
                if (internalAssemblyPrefixes.Any(prefix => assemblyName.Contains(prefix))) continue;

                // Skip internal/system assemblies
                if (internalAssemblyNames.Contains(assemblyName)) continue;

                // Yield return user-created assembly
                yield return assembly;
            }
        }

        public static IEnumerable<MethodInfo> GetEvaluatedMethods(this Assembly assembly)
        {
            // Helper method to check if a method has the CompletionEvaluationAttribute
            bool HasAttribute(MethodInfo methodInfo) => methodInfo.GetCustomAttributes(typeof(CompletionEvaluationAttribute), false).Length > 0;

            // Get all methods with the CompletionEvaluationAttribute in the assembly
            return assembly.GetTypes().SelectMany(type => type.GetMethods(flags)).Where(HasAttribute);;
        }

        public static IEnumerable<Assembly> GetEvaluatedAssemblies() => AppDomain.CurrentDomain.GetUserCreatedAssemblies().Where(assembly => assembly.GetEvaluatedMethods().Any());

        private static void CollectMethods()
        {
            // Get all user-created assemblies
            if (assemblies == null)
            {
                // Get user-created assemblies in the current AppDomain
                assemblies = AppDomain.CurrentDomain.GetUserCreatedAssemblies();

                // Iterate through each assembly to find methods with the attribute
                foreach (var assembly in assemblies)
                {
                    // Get methods with the CompletionEvaluationAttribute in the current assembly
                    var assemblyMethods = assembly.GetEvaluatedMethods();

                    // If there are no methods in this assembly, continue to the next
                    if (!assemblyMethods.Any()) continue;

                    // Accumulate methods from all assemblies
                    if (methods == null) methods = assemblyMethods;
                    else methods = methods.Concat(assemblyMethods);
                }
            }
        }

        public static float GetCompletionEvaluation()
        {
            // Collect methods with CompletionEvaluationAttribute if not already collected
            CollectMethods();

            // If no methods found, return default completion value of 0
            if (methods == null || !methods.Any()) return 0f;

            // Calculate the completion evaluation based on the methods found
            return methods.Average(method => (float)method.Invoke(null, null));
        }

        public static float ScaleToPercentage(this float value) => Math.Clamp(value * 100f, 0f, 100f);

        public static float AsPercentage() => GetCompletionEvaluation().ScaleToPercentage();

        [CompletionEvaluation]
        public static float TestCompletionEvaluation0() => 0;

        [CompletionEvaluation]
        public static float TestCompletionEvaluation1() => 1;
    }
}
