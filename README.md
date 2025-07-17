# Somniloquy

Yume 2kki was the video game that has inspired me to be indulged in dreams, and since the interest to dreams was my reason for being interested in Neuroscience and Artificial Intelligence - I can say that it has made a prolonging impact to my life. Although I do not plan to become a video game developer, my interest to the game and its original, Yume Nikki has continuously gave impulse to build a fangame to those. 

...As I could not abandon that impulses, I have improvised various ideas that would make a creative Yume Nikki fan game, and therefore I present this project - which tries to implement some relatively probable ideas of those.

Somniloquy is a Yume Nikki fangame that aims to capture the nature of oneironautics more meticulously. This means various aspects of the game are inspired from popular techniques or empirical commons of lucid dreamers (but not yet completely abandoning the abstractized aesthetics of Yume Nikki).


```
monogame-somniloquy
├─ app.manifest
├─ Assets
│  ├─ doll.sqSection2D
│  ├─ Loops
│  │  ├─ bgm006.wav
│  │  ├─ loop_74.wav
│  │  ├─ moseni_firefly.wav
│  │  ├─ moseni_fog.wav
│  │  ├─ moseni_forgetmenot.wav
│  │  ├─ moseni_freesia.wav
│  │  ├─ moseni_hydrangea.wav
│  │  ├─ moseni_leaf.wav
│  │  ├─ moseni_lotus.wav
│  │  ├─ moseni_luna.wav
│  │  ├─ music14.mp3
│  │  ├─ My Song 3.wav
│  │  ├─ n3-AoF.wav
│  │  ├─ n3-BoM.wav
│  │  ├─ n3-CaSP.wav
│  │  ├─ n3-ELP.wav
│  │  ├─ n3-HRiS.wav
│  │  ├─ n3-iFF.wav
│  │  ├─ n3-KtO.wav
│  │  ├─ n3-LaW.wav
│  │  ├─ n3-LaWoS.wav
│  │  ├─ n3-LI.wav
│  │  ├─ n3-NiT.wav
│  │  ├─ n3-RoW.wav
│  │  ├─ n3-SaP.wav
│  │  ├─ n3-Tcs.wav
│  │  ├─ n3-tFS.wav
│  │  ├─ n3-tSP.wav
│  │  ├─ n3-tWW.wav
│  │  ├─ n3-WeR.wav
│  │  └─ 本名_1_small.wav
│  ├─ Shaders
│  │  ├─ ETFractal.fx
│  │  └─ ETFractal.mgfx
│  ├─ Sounds
│  │  ├─ confirm.ogg
│  │  ├─ menu-close.ogg
│  │  ├─ menu-open.ogg
│  │  ├─ ocarina-convolver-impulse.ogg
│  │  ├─ ocarina.ogg
│  │  ├─ song-correct.ogg
│  │  └─ start.ogg
│  ├─ Sprites
│  └─ Worlds
│     ├─ room.sqSection2D
│     └─ tree.sqSection2D
├─ Content
│  ├─ Content.mgcb
│  ├─ Content.npl
│  ├─ Fonts
│  │  ├─ misaki.fnt
│  │  ├─ Misaki.spritefont
│  │  ├─ misaki_0.png
│  │  └─ misaki_gothic_2nd.ttf
│  └─ Shaders
│     └─ ETFractal.fx
├─ Core
│  ├─ 2D
│  │  ├─ Camera2D.cs
│  │  ├─ Entity.cs
│  │  ├─ Layer2D.cs
│  │  ├─ Section2D.cs
│  │  ├─ Sprite2D.cs
│  │  ├─ TextureLayer2D.cs
│  │  ├─ Tile2D.cs
│  │  └─ TileLayer2D.cs
│  ├─ 3D
│  │  ├─ Camera3D.cs
│  │  ├─ Object3D.cs
│  │  ├─ PhysicsManager.cs
│  │  ├─ Section3D.cs
│  │  ├─ SQLight.cs
│  │  ├─ SQMaterial.cs
│  │  └─ SQRenderer3D.cs
│  ├─ Brush.cs
│  ├─ Command.cs
│  ├─ DebugInfo.cs
│  ├─ Extensions
│  │  ├─ CircleF.cs
│  │  ├─ ColorExtensions.cs
│  │  ├─ RectangleExtensions.cs
│  │  ├─ RectangleF.cs
│  │  ├─ Serializers.cs
│  │  ├─ SQReferenceHandler.cs
│  │  ├─ SQSpriteBatch.cs
│  │  ├─ SQTexture2D.cs
│  │  ├─ Vector2I.cs
│  │  └─ VectorExtensions.cs
│  ├─ Managers
│  │  ├─ InputManager.cs
│  │  ├─ IOManager.cs
│  │  ├─ ScriptManager.cs
│  │  ├─ ShaderManager.cs
│  │  └─ SoundManager.cs
│  ├─ PixelActions.cs
│  ├─ Screens
│  │  ├─ CollisionMode.cs
│  │  ├─ ETFractalScreen.cs
│  │  ├─ PaintMode.cs
│  │  ├─ Screen.cs
│  │  ├─ ScreenManager.cs
│  │  ├─ Section2DEditor.cs
│  │  ├─ Section2DPlayer.cs
│  │  ├─ Section2DScreen.cs
│  │  ├─ Section3DScreen.cs
│  │  ├─ TileMode.cs
│  │  └─ UI
│  │     ├─ BoxUI.cs
│  │     ├─ BoxUIRenderer.cs
│  │     ├─ ColorPicker.cs
│  │     ├─ FileExplorer.cs
│  │     ├─ LayerTable.cs
│  │     ├─ NewLayerUI.cs
│  │     └─ TextLabel.cs
│  ├─ Somniloquy.cs
│  ├─ Tweening.cs
│  ├─ Util.cs
│  └─ World.cs
├─ ExternalAssemblies
│  ├─ FMod
│  │  ├─ fmod.dll
│  │  ├─ fmodL.dll
│  │  ├─ fmodstudio.dll
│  │  └─ fmodstudioL.dll
│  └─ WintabDN
│     ├─ CMemUtils.cs
│     ├─ CWintabContext.cs
│     ├─ CWintabData.cs
│     ├─ CWintabExtensions.cs
│     ├─ CWintabFuncs.cs
│     ├─ CWintabInfo.cs
│     ├─ MessageEvents.cs
│     └─ SDL2.cs
├─ Icon.bmp
├─ Icon.ico
├─ obj
│  ├─ Debug
│  │  └─ net6.0
│  │     ├─ .NETCoreApp,Version=v6.0.AssemblyAttributes.cs
│  │     ├─ apphost.exe
│  │     ├─ ref
│  │     │  └─ Somniloquy.dll
│  │     ├─ refint
│  │     │  └─ Somniloquy.dll
│  │     ├─ Somniloquy.AssemblyInfo.cs
│  │     ├─ Somniloquy.AssemblyInfoInputs.cache
│  │     ├─ Somniloquy.assets.cache
│  │     ├─ Somniloquy.csproj.AssemblyReference.cache
│  │     ├─ Somniloquy.csproj.BuildWithSkipAnalyzers
│  │     ├─ Somniloquy.csproj.CopyComplete
│  │     ├─ Somniloquy.csproj.CoreCompileInputs.cache
│  │     ├─ Somniloquy.csproj.FileListAbsolute.txt
│  │     ├─ Somniloquy.csproj.Up2Date
│  │     ├─ Somniloquy.dll
│  │     ├─ Somniloquy.GeneratedMSBuildEditorConfig.editorconfig
│  │     ├─ Somniloquy.genruntimeconfig.cache
│  │     ├─ Somniloquy.pdb
│  │     └─ Somniloquy.sourcelink.json
│  ├─ project.assets.json
│  ├─ project.nuget.cache
│  ├─ Release
│  │  └─ net6.0
│  │     ├─ .NETCoreApp,Version=v6.0.AssemblyAttributes.cs
│  │     ├─ ref
│  │     ├─ refint
│  │     ├─ Somniloquy.AssemblyInfo.cs
│  │     ├─ Somniloquy.AssemblyInfoInputs.cache
│  │     ├─ Somniloquy.assets.cache
│  │     ├─ Somniloquy.csproj.AssemblyReference.cache
│  │     ├─ Somniloquy.GeneratedMSBuildEditorConfig.editorconfig
│  │     └─ win-x64
│  │        ├─ .NETCoreApp,Version=v6.0.AssemblyAttributes.cs
│  │        ├─ apphost.exe
│  │        ├─ PublishOutputs.1caa94acc5.txt
│  │        ├─ R2R
│  │        │  ├─ Autofac.dll
│  │        │  ├─ FmodForFoxes.dll
│  │        │  ├─ MathNet.Numerics.dll
│  │        │  ├─ MonoGame.Extended.dll
│  │        │  ├─ MonoGame.Extended.Graphics.dll
│  │        │  ├─ MonoGame.Extended.Tiled.dll
│  │        │  ├─ MonoGame.Framework.dll
│  │        │  ├─ NativeFileDialogSharp.dll
│  │        │  ├─ Newtonsoft.Json.dll
│  │        │  └─ Somniloquy.dll
│  │        ├─ ref
│  │        │  └─ Somniloquy.dll
│  │        ├─ refint
│  │        │  └─ Somniloquy.dll
│  │        ├─ Somniloquy.AssemblyInfo.cs
│  │        ├─ Somniloquy.AssemblyInfoInputs.cache
│  │        ├─ Somniloquy.assets.cache
│  │        ├─ Somniloquy.csproj.AssemblyReference.cache
│  │        ├─ Somniloquy.csproj.CoreCompileInputs.cache
│  │        ├─ Somniloquy.csproj.FileListAbsolute.txt
│  │        ├─ Somniloquy.csproj.Up2Date
│  │        ├─ Somniloquy.dll
│  │        ├─ Somniloquy.GeneratedMSBuildEditorConfig.editorconfig
│  │        ├─ Somniloquy.genruntimeconfig.cache
│  │        ├─ Somniloquy.pdb
│  │        └─ Somniloquy.sourcelink.json
│  ├─ Somniloquy.csproj.nuget.dgspec.json
│  ├─ Somniloquy.csproj.nuget.g.props
│  └─ Somniloquy.csproj.nuget.g.targets
├─ README.md
├─ Somniloquy.csproj
├─ Somniloquy.csproj.Backup.tmp
└─ Somniloquy.sln

```