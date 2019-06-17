using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HexagonImagesGenerator : EditorWindow
{
  private Renderer[] currentlyHiddenGameObjects;
  private Hexagon[] hexagons;
  public enum RenderModes { Scene, SceneDepth };
  private int selectedHexagonIndex = 0;
  private int selectedRenderModeIndex = 0;
  private GUIStyle buttonStyle;
  private GUIStyle popupStyle;

  public RenderModes CurrentRenderMode
  {
    get
    {
      return (RenderModes)Enum.GetValues(typeof(RenderModes)).GetValue(selectedRenderModeIndex);
    }
  }

  public void OnGUI()
  {
    SetStyles();

    selectedRenderModeIndex = EditorGUILayout.Popup("Render Mode",
                                                    selectedRenderModeIndex,
                                                    Enum.GetNames(typeof(RenderModes)),
                                                    popupStyle);

    EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);

    FindAllHexagons();
    RefreshDropdown();

    if (GUILayout.Button("Move To Selected Hexagon", buttonStyle))
    {
      hexagons[selectedHexagonIndex].Focus();
      SceneView.FrameLastActiveSceneView();
    }

    EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);

    if (GUILayout.Button("Render Image of Selected Hexagon", buttonStyle))
    {
      RenderImageOfSelectedHexagon();
    }

    if (GUILayout.Button("Render Images of all Hexagons", buttonStyle))
    {
      for (int i = 0; i < hexagons.Length; i++)
      {
        selectedHexagonIndex = i;
        RenderImageOfSelectedHexagon();
      }
    }

    hexagons[selectedHexagonIndex].Focus();
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
    hexagons = GameObject.FindGameObjectsWithTag("Hexagon")
                          .Select(gameObject => new Hexagon(gameObject))
                          .ToArray();
  }

  private void RefreshDropdown()
  {
    string[] hexagonNames = hexagons.Select(hexagon => hexagon.Name).ToArray();

    Array.Sort(hexagonNames, (firstName, secondName) => String.Compare(firstName, secondName));

    selectedHexagonIndex = EditorGUILayout.Popup("Available Hexagons",
                                                  selectedHexagonIndex,
                                                  hexagonNames,
                                                  popupStyle);
  }

  private void RenderImageOfSelectedHexagon()
  {
    Hexagon currentlySelectedHexagon = hexagons[selectedHexagonIndex];

    PrepareScene();

    currentlySelectedHexagon.RenderImage(CurrentRenderMode);

    RestoreScene();
  }

  private void PrepareScene()
  {
    Hexagon currentlySelectedHexagon = hexagons[selectedHexagonIndex];
    Renderer[] gameObjectsToHide = GameObject.FindObjectsOfType<Renderer>()
                                    .Except(
                                      new Renderer[] { currentlySelectedHexagon.GameObject.GetComponent<Renderer>() }
                                        .Union(currentlySelectedHexagon.Children)
                                    )
                                    .ToArray();

    HideGameObjects(gameObjectsToHide);
    HideAllLights();

    this.currentlyHiddenGameObjects = gameObjectsToHide;
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

  private void RestoreScene()
  {
    ShowGameObjects(this.currentlyHiddenGameObjects);
    ShowAllLights();

    this.currentlyHiddenGameObjects = Array.Empty<Renderer>();
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

  public static GameObject GetChildWithTag(GameObject parent, string tag)
  {
    for (int i = 0; i < parent.transform.childCount; i++)
    {
      if (parent.transform.GetChild(i).tag.CompareTo(tag) == 0)
      {
        return parent.transform.GetChild(i).gameObject;
      }
    }

    return null;
  }

  [MenuItem("Hexagon Images/Generator")]
  public static void ShowWindow()
  {
    GetWindowWithRect<HexagonImagesGenerator>(new Rect(0, 0, 350, 205), true, "Generate Map Images for Hexagons", true);
  }

  private class Hexagon
  {
    private UnityEngine.Object hexagon;
    private List<Renderer> hiddenChildren = new List<Renderer>();
    private List<Renderer> visibleChildren = new List<Renderer>();

    public Hexagon(UnityEngine.Object gameObject) => hexagon = gameObject;

    public Renderer[] Children
    {
      get
      {
        return this.GameObject.GetComponentsInChildren<Renderer>();
      }
    }

    public int CircumRadius
    {
      get
      {
        const int largeHexagonCircumRadius = 6;
        const int smallHexagonCircumRadius = 4;
        const string largeHexagonShaderName = "M_HexBase_12m";
        const string smallHexagonShaderName = "M_HexBase_8m";

        GameObject hexagonPlatform = GetChildWithTag(this.GameObject, "HexagonPlatform") ?? this.GameObject;
        string shaderName = hexagonPlatform.GetComponent<Renderer>().sharedMaterial.shader.name;

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
    }

    public GameObject GameObject
    {
      get { return hexagon as GameObject; }
    }

    public string Name
    {
      get { return hexagon.name; }
    }

    public void Focus()
    {
      Selection.activeGameObject = this.GameObject;
    }

    public void RenderImage(RenderModes renderMode)
    {
      float hexagonalAspectRatio = Mathf.Sqrt(3) / 2;
      int height = 1920;
      int width = System.Convert.ToInt32(height * hexagonalAspectRatio);

      GameObject imageRenderer = new GameObject();
      Camera camera = imageRenderer.AddComponent<Camera>();
      Light light = imageRenderer.AddComponent<Light>();
      RenderTexture renderTexture = new RenderTexture(width, height, 24);
      Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

      if (renderMode == RenderModes.SceneDepth)
      {
        imageRenderer.AddComponent<DepthCamera>();

        camera.depthTextureMode = DepthTextureMode.Depth;
        camera.farClipPlane = 7.5f;
        camera.nearClipPlane = 0f;
      }

      camera.aspect = hexagonalAspectRatio;
      camera.orthographic = true;
      camera.orthographicSize = this.CircumRadius;
      camera.targetTexture = renderTexture;
      camera.transform.Rotate(90, 180, 0);

      light.range = 1000;

      imageRenderer.transform.position = this.GameObject.transform.position + new Vector3(0, 7.5f, 0);

      RevealPath();

      camera.Render();
      RenderTexture.active = renderTexture;

      screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

      SaveImage(screenshot, renderMode);

      ConcealPath();

      camera.targetTexture = null;
      RenderTexture.active = null;
      DestroyImmediate(screenshot);
      DestroyImmediate(renderTexture);
      DestroyImmediate(imageRenderer);
    }

    private void RevealPath()
    {
      List<Renderer> hiddenHexagonChildren = new List<Renderer>();

      foreach (Renderer renderer in Children)
      {
        if (renderer.gameObject != this.GameObject &&
            renderer.transform.position.y - 1 > this.GameObject.transform.position.y ||
            renderer.gameObject.name.IndexOf("shroom", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            renderer.gameObject.name.IndexOf("crystal", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
          renderer.enabled = false;

          this.hiddenChildren.Add(renderer);
        }
        else
        {
          this.visibleChildren.Add(renderer);
        }
      }
    }

    private void SaveImage(Texture2D screenshot, RenderModes renderMode)
    {
      byte[] bytes = screenshot.EncodeToPNG();
      System.IO.FileInfo file = new System.IO.FileInfo($"Generated Map Images/{this.Name}_{renderMode.ToString()}.png");

      file.Directory.Create();

      System.IO.File.WriteAllBytes(file.FullName, bytes);
    }

    private void ConcealPath()
    {
      foreach (Renderer renderer in this.hiddenChildren)
      {
        renderer.enabled = true;
      }

      this.hiddenChildren.Clear();
    }
  }
}
