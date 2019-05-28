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

    if (GUILayout.Button("Move To Selected Hexagon", buttonStyle)) {
      FocusSelectedHexagon();
      SceneView.FrameLastActiveSceneView();
    }

    EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);

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
                          .Where(gameObject => gameObject.name.Contains("hex_base"))
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