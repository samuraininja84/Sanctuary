using System;
using System.Reflection;
using UnityEditor;

namespace Sanctuary.Editor
{
    //[InitializeOnLoad]
    //public class CustomAddTab
    //{
    //    static CustomAddTab()
    //    {
    //        Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
    //        Type hostView = assembly.GetType("UnityEditor.HostView");
    //        FieldInfo k_PaneTypes = hostView.GetField("k_PaneTypes", BindingFlags.Static | BindingFlags.NonPublic);

    //        k_PaneTypes.SetValue(null, new Type[]
    //        {
    //            typeof(SceneView),
    //            assembly.GetType("UnityEditor.GameView"),
    //            assembly.GetType("UnityEditor.InspectorWindow"),
    //            assembly.GetType("UnityEditor.SceneHierarchyWindow"),
    //            assembly.GetType("UnityEditor.ProjectBrowser"),
    //            assembly.GetType("UnityEditor.ConsoleWindow"),
    //            assembly.GetType("UnityEditor.ProfilerWindow"),
    //            assembly.GetType("UnityEditor.AnimationWindow"),
    //            typeof(SanctuaryEditor),
    //        });
    //    }
    //}
}