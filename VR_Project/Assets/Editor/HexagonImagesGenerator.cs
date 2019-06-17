﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HexagonImagesGenerator : EditorWindow
{
  private Renderer[] currentlyHiddenGameObjects;
  private Hexagon[] hexagons;
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
      hexagons[selectedHexagonIndex].Focus();
      SceneView.FrameLastActiveSceneView();
    }

    EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);

    if (GUILayout.Button("Render Image from Selected Hexagon", buttonStyle))
    {
      RenderImageFromSelectedHexagon();
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
    hexagons = GameObject.FindObjectsOfType(typeof(GameObject))
                          .Where(gameObject => gameObject
                                                .name
                                                .IndexOf("hex", System.StringComparison.OrdinalIgnoreCase) >= 0)
                          .Select(gameObject => new Hexagon(gameObject))
                          .ToArray();
  }

  private void RefreshDropdown()
  {
    selectedHexagonIndex = EditorGUILayout.Popup("Available Hexagons",
                                                  selectedHexagonIndex,
                                                  hexagons.Select(hexagon => hexagon.Name).ToArray(),
                                                  popupStyle);
  }

  private void RenderImageFromSelectedHexagon()
  {
    Hexagon currentlySelectedHexagon = hexagons[selectedHexagonIndex];

    PrepareScene();

    currentlySelectedHexagon.RenderImage();

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

  [MenuItem("Hexagon Images/Generator")]
  public static void ShowWindow()
  {
    GetWindowWithRect<HexagonImagesGenerator>(new Rect(0, 0, 350, 130), true, "Generate Map Images from Hexagons", true);
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
        string shaderName = this.GameObject.GetComponent<Renderer>().sharedMaterial.shader.name;

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

    public void RenderImage()
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
      camera.orthographicSize = this.CircumRadius;
      camera.targetTexture = renderTexture;
      camera.transform.Rotate(90, 180, 0);

      light.range = 1000;

      imageRenderer.transform.position = this.GameObject.transform.position + new Vector3(0, 7.5f, 0);

      RevealPath();

      camera.Render();
      RenderTexture.active = renderTexture;

      screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

      SaveImage(screenshot);

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

      foreach (Renderer renderer in this.Children)
      {
        if (renderer.gameObject != this.GameObject &&
            renderer.transform.position.y - 1 > this.GameObject.transform.position.y)
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

    private void SaveImage(Texture2D screenshot)
    {
      byte[] bytes = screenshot.EncodeToPNG();
      System.IO.FileInfo file = new System.IO.FileInfo($"Generated Map Images/{this.Name}.png");

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
