using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VuforiaTargetSceneSync
{
    const string MainScenePath = "Assets/Scenes/main.unity";
    const string SourceScenePath = "Assets/Scenes/SampleScene.unity";

    static readonly string[] TargetNames =
    {
        "01_chu-yin",
        "02_hui-ing",
        "03_miss-hsieh",
        "04_sui-en",
        "05_employment",
        "06_get-married",
        "07_learning",
        "08_travel"
    };

    [MenuItem("Tools/When Flowers Bloom/Sync Vuforia Targets To Main")]
    public static void SyncTargetsToMain()
    {
        var mainScene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        var sourceScene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Additive);

        var existingTargets = FindTargetRoots(mainScene);
        foreach (var target in existingTargets)
        {
            Object.DestroyImmediate(target);
        }

        var sourceTargets = FindTargetRoots(sourceScene);
        int copied = 0;

        foreach (var sourceTarget in sourceTargets)
        {
            var clone = Object.Instantiate(sourceTarget);
            clone.name = sourceTarget.name;
            SceneManager.MoveGameObjectToScene(clone, mainScene);
            copied++;
        }

        EditorSceneManager.CloseScene(sourceScene, true);
        EditorSceneManager.MarkSceneDirty(mainScene);
        EditorSceneManager.SaveScene(mainScene);

        Debug.Log($"Copied {copied} Vuforia image target(s) into {MainScenePath}.");
    }

    static List<GameObject> FindTargetRoots(Scene scene)
    {
        var result = new List<GameObject>();

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var targetName in TargetNames)
            {
                var child = FindByName(root.transform, targetName);
                if (child != null && HasVuforiaObserver(child.gameObject))
                {
                    result.Add(child.gameObject);
                }
            }
        }

        return result;
    }

    static Transform FindByName(Transform root, string targetName)
    {
        if (root.name == targetName)
        {
            return root;
        }

        foreach (Transform child in root)
        {
            var match = FindByName(child, targetName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    static bool HasVuforiaObserver(GameObject gameObject)
    {
        foreach (var component in gameObject.GetComponents<Component>())
        {
            if (component == null)
            {
                continue;
            }

            var typeName = component.GetType().FullName;
            if (typeName == "Vuforia.ImageTargetBehaviour" || typeName == "Vuforia.ObserverBehaviour")
            {
                return true;
            }
        }

        return false;
    }
}
