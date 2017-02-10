# probuilder-vr

ProBuilder in the matrix.

## Super Quick Start

- Download the [EditorVR version of Unity 5.4.3](http://rebrand.ly/EditorVR-build)
- Do **not** download the EditorVR Unity package (ProBuilderVR uses a snapshot from the development branch)
- Download [ProBuilderVR-with-dependencies.unitypackage](https://github.com/procore3d/probuilder-vr/releases/tag/v0.1.0b0).
- Open new project in Unity 5.4.3-vr
- Import ProBuilderVR-with-dependencies.unitypackage
- Open EditorVR (`Control + Shift + E`)

## Installing from Git - Quick Start

Theses steps are only necessary if you're interested in keeping up to date with the day to day changes of ProBuilderVR.

If you're only interested in releases, follow the Super Quick Start steps.

- Install [git lfs](https://git-lfs.github.com/)
- Clone repository `git clone https://github.com/procore3d/probuilder-vr.git`
- Navigate to `probuilder-vr/Assets` (open Terminal / Git bash / etc, `cd Assets`)
- Clone EditorVR `git lfs clone --recursive -b development https://github.com/Unity-Technologies/EditorVR`
- Download and import [SteamVR](https://www.assetstore.unity3d.com/en/#!/content/32647) from Asset Store

## FAQ

#### `IUsesMenuOrigin not found`

You downloaded the EditorVR unity package.  Delete the `EditorVR` folder and import `ProBuilderVR-with-dependencies.unitypackage`

####  Nothing is working!

Like, nothing nothing?  Try asking about that over at the [Unity EditorVR forum](https://forum.unity3d.com/forums/editorvr.126/).

#### ProBuilderVR specifically isn't working!

Please open an Issue on Github explaining the problem in as much detail as possible.

#### I Have a Suggestion / Complaint

Suggestions, comments, high fives, etc are welcome over at our [forum](http://www.procore3d.com/forum/forum/43-probuildervr/).
