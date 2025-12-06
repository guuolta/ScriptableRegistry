# ScriptableRegistry

ScriptableRegistry scans your assets, generates an enum from file names, and auto-registers assets into a ScriptableObject dictionary. It keeps enums and registrations in sync for you, and exposes the registry as an `IReadOnlyDictionary` at runtime.

## Features
- Recursively scans folders with extension filters and ignore-folder settings
- Generates an enum plus a JSON file to keep numeric IDs stable across regenerations
- Auto-registers assets into a ScriptableObject dictionary using the generated enum (`ToDictionary()` returns a copy when you need it)
- One-click generation (`Window > ScriptableRegistry > CreateWindow`) for the registry, custom editor, and missing enum/value scripts
- Bundled samples (Music/Block) to see it working right away
- String building can switch to ZString by defining `USE_ZSTRING` (falls back to `System.Text.StringBuilder`)

## Requirements
- Unity 6000.3 or newer
- Supports UPM local and Git installations

## Installation
- **Local path**: Place this repo at your project root or copy `Packages/com.guuolta.ScriptableRegistry`. In Package Manager choose **Add package from disk...** and select `Packages/com.guuolta.ScriptableRegistry/package.json`, or add it to `manifest.json`:
  ```json
  {
    "dependencies": {
      "com.guuolta.ScriptableRegistry": "file:Packages/com.guuolta.ScriptableRegistry"
    }
  }
  ```
- **Git (UPM)**: In Package Manager choose **Add package from git URL...** and enter, or add to `manifest.json`:
  - `https://github.com/guuolta/ScriptableRegistry.git?path=Packages/com.guuolta.ScriptableRegistry`
  ```json
  {
    "dependencies": {
      "com.guuolta.ScriptableRegistry": "https://github.com/guuolta/ScriptableRegistry.git?path=Packages/com.guuolta.ScriptableRegistry"
    }
  }
  ```

## Quick Start
1. **(Optional) Generate scripts**  
   Open `Window > ScriptableRegistry > CreateWindow`.  
   ![Window](https://github.com/guuolta/ScriptableRegistry/blob/Image/Window.png)  
   Key parameters:
   - Script Name / Save Path / Namespace: Registry ScriptableObject script and location
   - Editor Script Name / Save Path / Namespace: Custom editor script and location
   - Menu Name / File Name: CreateAssetMenu path and asset file name
   - Key Enum Name / Namespace: Enum generated from asset file names
   - Value Class Name / Namespace: Value class stored in the dictionary
   - Target File Class Name: Target asset type (e.g., `GameObject`, `AudioClip`)  
   Auto toggles let you switch from auto-filled values to manual. Key Enum/Value Class are generated if missing or overwritten if they exist.

2. **Create a registry asset**  
   Use the generated CreateAssetMenu entry (e.g., `ScriptableRegistry/...`) to create the ScriptableObject asset.

3. **Inspector setup**  
   ![SO](https://github.com/guuolta/ScriptableRegistry/blob/Image/SO.png)  
   - Folder Path: Folder to scan (no trailing `/`)
   - Enum Path / Enum Namespace / Enum File Name: Output path and name for enum/JSON
   - File Extensions: Target extensions (comma-separated, e.g., `.prefab,.asset`)
   - Ignore Folder Names: Folders to skip
   - Buttons:
     - **Generate Enum / JSON**: Generate enum and JSON (triggers recompilation)
     - **Register Dictionary From Existing Enum**: Register assets into the dictionary using the generated enum
     - **Reset**: Clear the dictionary

4. **Use at runtime**  
   The `Dictionary` property is exposed as `IReadOnlyDictionary`. Call `ToDictionary()` when you need a mutable copy.

## Customization
![Window](https://github.com/guuolta/ScriptableRegistry/blob/Image/Program.png)  
The editor extension is minimal; override these to adjust behavior:
- `GetDefaultParams()`: Provide default enum values and other initial settings
- `CreateValue()`: Define how each dictionary value is created

## Runtime Example
```csharp
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private ScriptableRegistry.Sample.Music.MusicRegisterObject _registry;
    [SerializeField] private AudioSource _audioSource;

    public void Play(ScriptableRegistry.Sample.Music.MusicType type)
    {
        if (_registry.Dictionary.TryGetValue(type, out var clip))
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}
```

## Outputs
- Enum/JSON are saved to `Enum Path` as `{EnumFileName}.cs` / `{EnumFileName}.json`; the JSON keeps numeric IDs stable between generations.
- The registry dictionary uses `SerializableDictionary`, so you can inspect and edit entries in the Inspector.
- Sample output (Block/Music):
  ```
  Assets/Sample/
  ├─ Block/
  │  ├─ Block1.prefab
  │  ├─ Block2.prefab
  │  ├─ Block3.prefab
  │  ├─ IgnoreBlockFolder/…      # ignored folders example
  │  └─ IgnoreBlockFolder2/…     # ignored folders example
  ├─ Music/
  │  ├─ 1_軽いパンチ1.mp3
  │  ├─ K.O..mp3
  │  └─ 剣で斬る1.mp3
  ├─ Scripts/
  │  ├─ Block/
  │  │  ├─ BlockType.cs
  │  │  ├─ BlockType.json
  │  │  ├─ BlockRegisterObject.cs
  │  │  └─ BlockBehaviour.cs
  │  └─ Music/
  │     ├─ MusicType.cs
  │     ├─ MusicType.json
  │     └─ MusicRegisterObject.cs
  └─ ScriptableObject/
     ├─ BlockRegisterObject.asset
     └─ MusicRegisterObject.asset
  ```

## About SerializableDictionary
- Unity cannot serialize `Dictionary` directly, so `SerializableDictionary` is used to keep the registry visible and editable in the Inspector.
- The `Dictionary` property is exposed as `IReadOnlyDictionary` for safe runtime reads. If you need to mutate data, take a copy via `ToDictionary()`.

## Samples
- `Assets/Sample` bundles scripts and assets (Music uses `AudioClip`, Block fetches components from `GameObject`).
- Open `Assets/Scenes/SampleScene.unity` to see it in action.

## Development & Contributions
- Issues and pull requests are welcome for fixes, enhancements, and docs.
- Package sources live under `Packages/com.guuolta.ScriptableRegistry`; editor code is in `Editor/`, runtime code in `Runtime/`.

## License
- MIT License (see `LICENSE`)

## Third-party assets
This repository contains third-party assets.

## Sound effects
Provider: 効果音ラボ (Sound Effect Lab)  
License / Terms of use: https://soundeffect-lab.info/agreement  
Notes: The sound effects are included only as part of this project/sample.  
They are not intended for standalone redistribution.
