# Project Structure and Style Guidelines

## Naming Conventions
Asset names should follow the Prefix_BaseName_Suffix pattern where the pre-and suffixes are used to indicate type and variant, this makes it easier to search and parse asset names

| Asset Type              | Prefix     | Suffix     | Notes                            |
| ----------------------- | ---------- | ---------- | -------------------------------- |
| Scene                   |            |            | Should be in Assets/Scenes       |
| Prefab                  | PF_        |            |                                  |
| Material                | M_         |            |                                  |
| Static Mesh             | SM         |            |                                  |
| Skeletal Mesh           | SK_        |            |                                  |
| Texture (Diffuse/Albedo)| T_         | _D         |                                  |
| Texture (Normal)        | T_         | _N         |                                  |
| Texture (Smoothness)    | T_         | _S         |                                  |
| Texture (Metallic)      | T_         | _M         |                                  |
| Texture (Emissive)      | T_         | _E         |                                  |
| Particle System         | PS_        |            |                                  |
| Shader                  | S_         |            |                                  |
| Animation               | AN_        |            |                                  |
| Sound Wave              | A_         |            |                                  |


## Directory Structure
All Assets should be placecd in an appropriate subfolder of the Assets directory which corresponds to their type eg
- Assets/Prefabs
- Assets/Meshes
- Assets/Materials
- Assets/Sounds
- Assets/Scenes
- ...

If an assets is uniquely associated with another asset (eg. a texture that is only used as a part of one material, a mesh which is part of exactly one prefab, etc.) then it should be placed in a subfolder together with its parent asset
Example:

```
+-- Assets
    +-- Prefabs
    |   +-- SomeObject
    |   |   +-- PF_SomeObject
    |   |   +-- M_SomeObject
    |   |   +-- T_SomeObject_D
    |   |   +-- T_SomeObject_N
    +-- Materials
    |   +-- SomeMaterial
    |   |   |-- M_SomeMaterial
    |   |   |-- T_SomeMaterial_D
    
```


## Style
Code Style should follow the default style of Unity which looks like the following

```cs
class thisIsAClass
{
    public string[] someVariable = { "bla", "blubb" };

    //This is a comment
    public static void someFunction()
    {    
        foreach (string a in someVariable)
        {
            Debug.Log(a);
        }
    }
}
```
