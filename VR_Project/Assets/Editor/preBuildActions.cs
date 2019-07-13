using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class preBuildActions
{
  public static string[] scenes = { "Assets/Scenes/caveMain.unity" };

  [MenuItem("Tools/Bake Scenes")]
  public static void buildLighting()
  {
    Debug.Log("building lighting...");
    //Lightmapping.BakeMultipleScenes(scenes);
    
    foreach (string scene in scenes)
    {
        Debug.Log("building scene: " + scene);
        EditorSceneManager.OpenScene(scene);
        Lightmapping.ClearLightingDataAsset();
        Lightmapping.Bake();
        //StaticOcclusionCulling.Compute();
        //EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
    }
    Debug.Log("lighting build done, saving assets... ");
    AssetDatabase.SaveAssets();
    Debug.Log("build finished");
  }
}