using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HexagonImagesGenerator : EditorWindow
{
  const int largeHexagonCircumRadius = 6;
  const int smallHexagonCircumRadius = 4;
  const string largeHexagonShaderName = "M_HexBase_12m";
  const string smallHexagonShaderName = "M_HexBase_8m";

  private UnityEngine.Object[] hexagons;
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
      RenderImageFromSelectedHexagon();
    }

    FocusSelectedHexagon();
  }

  private void SetStyles()
  {
    buttonStyle = new GUIStyle(GUI.skin.button);
    buttonStyle.margin = new RectOffset(15, 15, 15, 15);

    popupStyle = new GUIStyle(EditorStyles.popup);
    popupStyle.margin = new RectOffset(15, 15, 15, 15);
  }

  private void FindAllHexagons()
  {
    hexagons = GameObject.FindObjectsOfType(typeof(GameObject))
                          .Where(gameObject => gameObject
                                                .name
                                                .IndexOf("hex", System.StringComparison.OrdinalIgnoreCase) >= 0)
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

  private void RenderImageFromSelectedHexagon()
  {
    GameObject currentlySelectedHexagon = hexagons[selectedHexagonIndex] as GameObject;
    Renderer[] currentlySelectedHexagonChildren = currentlySelectedHexagon.GetComponentsInChildren<Renderer>();

    Renderer[] hiddenHexagonChildren = RevealPathOfHexagon(currentlySelectedHexagon, currentlySelectedHexagonChildren);
    Renderer[] gameObjectsToHide = GameObject.FindObjectsOfType<Renderer>()
                                    .Except(
                                      new Renderer[] { currentlySelectedHexagon.GetComponent<Renderer>() }
                                        .Union(currentlySelectedHexagonChildren)
                                    )
                                    .ToArray();

    PrepareScene(gameObjectsToHide);
    RenderImage(currentlySelectedHexagon);
    RestoreScene(hiddenHexagonChildren.Union(gameObjectsToHide).ToArray());
  }

  private void PrepareScene(Renderer[] gameObjectsToHide)
  {
    HideGameObjects(gameObjectsToHide);
    HideAllLights();
  }

  private void HideGameObjects(Renderer[] gameObjectsToHide)
  {
    foreach (Renderer renderer in gameObjectsToHide)
    {
      renderer.enabled = false;
    }
  }

  private void HideAllLights()
  {
    GameObject.FindObjectsOfType<Light>()
              .ToList()
              .ForEach(lightSource => lightSource.enabled = false);
  }

  private void RestoreScene(Renderer[] hiddenGameObjects)
  {
    ShowGameObjects(hiddenGameObjects);
    ShowAllLights();
  }

  private void ShowGameObjects(Renderer[] gameObjectsToShow)
  {
    foreach (Renderer renderer in gameObjectsToShow)
    {
      renderer.enabled = true;
    }
  }

  private void ShowAllLights()
  {
    GameObject.FindObjectsOfType<Light>()
              .ToList()
              .ForEach(lightSource => lightSource.enabled = true);
  }

  private Renderer[] RevealPathOfHexagon(GameObject hexagon, Renderer[] hexagonChildren)
  {
    List<Renderer> hiddenHexagonChildren = new List<Renderer>();

    foreach (Renderer renderer in hexagonChildren)
    {
      if (renderer.gameObject != hexagon.gameObject &&
          renderer.transform.position.y - 1 > hexagon.transform.position.y)
      {
        renderer.enabled = false;

        hiddenHexagonChildren.Add(renderer);
      }
    }

    return hiddenHexagonChildren.ToArray();
  }

  private void RenderImage(GameObject hexagon)
  {
    float hexagonalAspectRatio = Mathf.Sqrt(3) / 2;
    int height = 1920;
    int width = System.Convert.ToInt32(height * hexagonalAspectRatio);

    GameObject imageRenderer = new GameObject();
    Camera camera = imageRenderer.AddComponent<Camera>();
    Light light = imageRenderer.AddComponent<Light>();
    RenderTexture renderTexture = new RenderTexture(width, height, 24);
    Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

    camera.aspect = hexagonalAspectRatio;
    camera.orthographic = true;
    camera.orthographicSize = GetHexagonCircumRadius(hexagon);
    camera.targetTexture = renderTexture;
    camera.transform.Rotate(90, 180, 0);

    light.range = 1000;

    imageRenderer.transform.position = hexagon.transform.position + new Vector3(0, 7.5f, 0);

    camera.Render();
    RenderTexture.active = renderTexture;

    screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
    SaveImage(hexagon.name, screenshot);

    camera.targetTexture = null;
    RenderTexture.active = null;
    DestroyImmediate(screenshot);
    DestroyImmediate(renderTexture);
    DestroyImmediate(imageRenderer);
  }

  private int GetHexagonCircumRadius(GameObject hexagon)
  {
    string shaderName = hexagon.GetComponent<Renderer>().sharedMaterial.shader.name;

    if (shaderName == largeHexagonShaderName)
    {
      return largeHexagonCircumRadius;
    }
    else if (shaderName == smallHexagonShaderName)
    {
      return smallHexagonCircumRadius;
    }
    else
    {
      return largeHexagonCircumRadius;
    }
  }

  private void SaveImage(string name, Texture2D screenshot)
  {
    byte[] bytes = screenshot.EncodeToPNG();
    System.IO.FileInfo file = new System.IO.FileInfo($"Generated Map Images/{name}.png");

    file.Directory.Create();

    System.IO.File.WriteAllBytes(file.FullName, bytes);
  }

  [MenuItem("Hexagon Images/Generator")]
  public static void ShowWindow()
  {
    GetWindowWithRect<HexagonImagesGenerator>(new Rect(0, 0, 350, 130), true, "Generate Map Images from Hexagons", true);
  }
}
