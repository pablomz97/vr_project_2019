using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HexagonImagesGenerator : EditorWindow
{
  private Object[] hexagons;
  private int selectedHexagonIndex = 0;
  private GUIStyle buttonStyle;
  private GUIStyle popupStyle;

  public void OnGUI()
  {
    SetStyles();

    FindAllHexagons();
    RefreshDropdown();

    if (GUILayout.Button("Move To Selected Hexagon", buttonStyle))
    {
      FocusSelectedHexagon();
      SceneView.FrameLastActiveSceneView();
    }

    EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);

    if (GUILayout.Button("Render Image from Selected Hexagon", buttonStyle))
    {
      GameObject cameraHolder = new GameObject();
      Camera camera = cameraHolder.AddComponent<Camera>();
      RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);

      camera.targetTexture = renderTexture;
      camera.transform.localPosition = (hexagons[selectedHexagonIndex] as GameObject).transform.position + new Vector3(0, 10, 0);

      Texture2D screenshot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);

      camera.Render();

      RenderTexture.active = renderTexture;

      screenshot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);

      camera.targetTexture = null;
      RenderTexture.active = null;

      DestroyImmediate(renderTexture);

      byte[] bytes = screenshot.EncodeToPNG();
      string filename = "test.png";

      System.IO.File.WriteAllBytes(filename, bytes);

      // DestroyImmediate(camera);
    }

    FocusSelectedHexagon();
  }

  private void SetStyles()
  {
    buttonStyle = new GUIStyle(GUI.skin.button);
    buttonStyle.margin = new RectOffset(15, 15, 10, 15);

    popupStyle = new GUIStyle(EditorStyles.popup);
    popupStyle.margin = new RectOffset(15, 15, 15, 15);
  }

  private void FindAllHexagons()
  {
    hexagons = GameObject.FindObjectsOfType(typeof(GameObject))
                          .Where(gameObject => gameObject.name.IndexOf("hex", System.StringComparison.OrdinalIgnoreCase) >= 0)
                          .ToArray();
  }

  private void RefreshDropdown()
  {
    selectedHexagonIndex = EditorGUILayout.Popup("Available Hexagons",
                                                  selectedHexagonIndex,
                                                  hexagons.Select(hexagon => hexagon.name).ToArray(),
                                                  popupStyle);
  }

  private void FocusSelectedHexagon()
  {
    Selection.activeGameObject = (hexagons[selectedHexagonIndex] as GameObject).gameObject;
  }

  [MenuItem("Hexagon Images/Generator")]
  public static void ShowWindow()
  {
    GetWindow<HexagonImagesGenerator>(true, "Generate Map Images from Hexagons", true);
  }
}