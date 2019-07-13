using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class HexagonImagesGenerator : EditorWindow
{
  private Renderer[] currentlyHiddenGameObjects;
  private Hexagon[] hexagons;
  public enum RenderModes { Scene, SceneDepth, WeightedSceneDepth };
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
    hexagons = GameObject.FindGameObjectsWithTag("HexTile")
                          .Where(gameObject => !gameObject.name.Contains("dummy"))
                          .Select(gameObject => new Hexagon(gameObject))
                          .ToArray();

    Array.Sort(hexagons, (firstHexagon, secondHexagon) => String.Compare(firstHexagon.Name, secondHexagon.Name));
  }

  private void RefreshDropdown()
  {
    string[] hexagonNames = hexagons.Select(hexagon => hexagon.Name).ToArray();

    selectedHexagonIndex = EditorGUILayout.Popup("Available Hexagons",
                                                  selectedHexagonIndex,
                                                  hexagonNames,
                                                  popupStyle);
  }

  private void RenderImageOfSelectedHexagon()
  {
    Hexagon currentlySelectedHexagon = hexagons[selectedHexagonIndex];

    HideAllLights();

    currentlySelectedHexagon.RenderImage(CurrentRenderMode);

    ShowAllLights();
  }

  private void HideAllLights()
  {
    GameObject.FindObjectsOfType<Light>()
              .ToList()
              .ForEach(lightSource => lightSource.enabled = false);
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
    private OrderedDictionary hexagonSymbolOffsets = new OrderedDictionary() {
        { "c01", new Vector3(1.35f, 0, -1.1f) },
        { "c02", new Vector3(-0.85f, 0, 0.5f) },
        { "c03", new Vector3(-0.25f, 0, 0) },
        { "c05", new Vector3(0.2f, 0, -0.6f) },
        { "c06", new Vector3(-0.05f, 0, -0.45f) },
        { "c07", new Vector3(0.75f, 0, -0.75f) },
        { "c08", new Vector3(0.5f, 0, -1.25f) },
        { "c09", new Vector3(-0.1f, 0, -0.95f) },
        { "c10", new Vector3(-0.1f, 0, -0.75f) },
        { "c11", new Vector3(-0.2f, 0, -1.25f) },
        { "r01", new Vector3(0.15f, 0, -0.3f) },
        { "r02_v2", new Vector3(1.15f, 0, 0.5f) },
        { "r02", new Vector3(0.25f, 0, 0) }
      };
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

      GameObject hexagonSymbolContainer;
      GameObject imageRenderer = new GameObject();
      Camera camera = imageRenderer.AddComponent<Camera>();
      RenderTexture intermediateTexture = new RenderTexture(width, height, 24);
      RenderTexture renderTexture = new RenderTexture(width, height, 24);
      Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

      imageRenderer.transform.position = this.GameObject.transform.position + new Vector3(0, 10, 0);

      RevealPath();
      ConfigureCamera(ref camera, ref hexagonalAspectRatio);
      RenderHexagonSymbol(ref imageRenderer, out hexagonSymbolContainer);

      if (renderMode == RenderModes.Scene)
      {
        camera.targetTexture = renderTexture;
      }
      else if (renderMode == RenderModes.SceneDepth)
      {
        imageRenderer.AddComponent<MapImageCamera>().UseDepthShader();

        camera.depthTextureMode = DepthTextureMode.Depth;
        camera.farClipPlane = 11.15f;
        camera.nearClipPlane = 0;
        camera.targetTexture = renderTexture;
      }
      else
      {
        camera.targetTexture = intermediateTexture;
        camera.Render();

        MapImageCamera mapImageCamera = imageRenderer.AddComponent<MapImageCamera>();
        mapImageCamera.UseWeightedDepthShader();
        mapImageCamera.material.SetTexture("_MainTex", intermediateTexture);

        camera.depthTextureMode = DepthTextureMode.Depth;
        camera.farClipPlane = 11.10f;
        camera.nearClipPlane = 0;
        camera.targetTexture = renderTexture;
      }

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
      DestroyImmediate(hexagonSymbolContainer);
    }

    private void RevealPath()
    {
      List<Renderer> hiddenHexagonChildren = new List<Renderer>();

      foreach (Renderer renderer in Children)
      {
        if (renderer.gameObject != this.GameObject &&
            renderer.transform.position.y - 1.25 > this.GameObject.transform.position.y)
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

    private void ConfigureCamera(ref Camera camera, ref float hexagonalAspectRatio)
    {
      camera.aspect = hexagonalAspectRatio;
      camera.orthographic = true;
      camera.orthographicSize = this.CircumRadius;
      camera.transform.Rotate(90, 180, 0);
    }

    private void RenderHexagonSymbol(ref GameObject imageRenderer, out GameObject hexagonSymbolContainer)
    {
      float yDirectionAlteration = 0.1f;
      Vector2 hexagonSymbolSize = new Vector2(3, 3);

      hexagonSymbolContainer = new GameObject();
      TextMeshPro hexagonSymbol = hexagonSymbolContainer.AddComponent<TextMeshPro>();
      Renderer teleportArea = this.Children.Where(renderer => renderer.gameObject.name == "tpArea").First();

      hexagonSymbol.color = Color.white;
      hexagonSymbol.alignment = TextAlignmentOptions.Center;
      hexagonSymbol.font = (TMP_FontAsset)AssetDatabase.LoadAssetAtPath("Assets/Materials/Bravura SDF.asset", typeof(TMP_FontAsset));
      hexagonSymbol.fontSharedMaterial.SetTexture(ShaderUtilities.ID_FaceTex, (Texture)AssetDatabase.LoadAssetAtPath("Assets/Materials/Ground/T_rock_01_D.png", typeof(Texture)));
      hexagonSymbol.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.1f);
      hexagonSymbol.fontSharedMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, 0.1f);
      hexagonSymbol.rectTransform.sizeDelta = new Vector2(3, 3);
      hexagonSymbol.text = "▲";

      hexagonSymbolContainer.transform.SetParent(imageRenderer.transform, false);
      hexagonSymbolContainer.transform.position = this.GameObject.transform.position + GetHexagonSymbolOffset() + new Vector3(0, yDirectionAlteration, 0);
      hexagonSymbolContainer.transform.rotation.Set(0, 0, 0, 0);
    }

    private Vector3 GetHexagonSymbolOffset()
    {
      foreach (DictionaryEntry hexagonSymbolOffset in hexagonSymbolOffsets)
      {
        if (this.Name.Contains((string)hexagonSymbolOffset.Key))
        {
          return (Vector3)hexagonSymbolOffset.Value;
        }
      }

      return Vector3.zero;
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
