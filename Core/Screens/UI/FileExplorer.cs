namespace Somniloquy {
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public static class FileExplorer {
        public static bool Active;

        public static string CurrentDirectory;
        public static List<string> DirectoryContents = new();
        public static int HighlightedLine;
        public static bool HighlightingDirectoryContents;
        
        public static bool OverwriteWarning;

        public static BoxUI RootUI;
        public static BoxUI ContentsBox;
        public static TextLabel SaveNameBox;

        public static void Initialize() {
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.O }, (parameters) => { ToggleFileExplorer(); }, TriggerOnce.True, true);
            InputManager.RegisterKeybind(Keys.Tab, Keys.LeftShift, (parameters) => { if (Active) MoveHighlightedLine(1); }, TriggerOnce.Block);
            InputManager.RegisterKeybind(new object[] {Keys.LeftShift, Keys.Tab}, (parameters) => { if (Active) MoveHighlightedLine(-1); }, TriggerOnce.Block, true);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.S }, (parameters) => { if (Active) Save(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.L }, (parameters) => { if (Active) Load(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.E }, (parameters) => { if (Active) Export(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(Keys.Enter, Keys.LeftShift, (parameters) => { if (Active) EnterDirectory(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] {Keys.LeftShift, Keys.Enter}, (parameters) => { if (Active) LeaveDirectory(); }, TriggerOnce.True, true);
        }

        public static void BuildUI() {
            RootUI = new BoxUI(Util.ShrinkRectangle(new Rectangle(0, 0, SQ.WindowSize.X, SQ.WindowSize.Y), new(20))) { Identifier = "root" };
            Active = true;
            DebugInfo.Active = false;
            ScreenManager.GetFirstScreenOfType<Section2DScreen>().Editor.ColorPicker.Active = false;
            
            RootUI.AddChild(new TextLabel(RootUI, 20, 5) { Identifier = "path" });
            var mainBox = RootUI.AddChild(new BoxUI(RootUI, 20, 0) { MainAxis = Axis.Horizontal, Identifier = "mainBox", MainAxisFill = true, });
                ContentsBox = mainBox.AddChild(new BoxUI(mainBox) { MainAxis = Axis.Vertical, Identifier = "contents", MainAxisShrink = true, Highlighted = true });
                var previewBox = mainBox.AddChild(new BoxUI(mainBox) { Identifier = "previewBox" });
            var bottomBox = RootUI.AddChild(new BoxUI(RootUI, 20, 0) { Identifier = "bottomBox" });
                SaveNameBox = (TextLabel)bottomBox.AddChild(new TextLabel(bottomBox, 0, 20) { Identifier = "saveNameBox", Editable = true, } );
                var Button = bottomBox.AddChild(new BoxUI(bottomBox, 0, 20) { Identifier = "saveButton" });
        }

        public static void DestroyUI() {
            Active = false;
            DebugInfo.Active = true;
            ScreenManager.GetFirstScreenOfType<Section2DScreen>().Editor.ColorPicker.Active = true;

            RootUI.Destroy();
            RootUI = null;
        }

        public static void ToggleFileExplorer() {
            if (!Active) {
                BuildUI(); 
                OpenDirectory("c:\\Somnia\\Projects\\monogame-somniloquy\\Assets");
            } else {
                DestroyUI();
            }
        }
        
        public static void OpenDirectory(string directory) {
            CurrentDirectory = directory;
            DirectoryContents.Clear();
            HighlightedLine = 0;

            var (folders, files) = ListDirectoryContents(directory);
            ContentsBox.Children.Clear();

            foreach (var folder in folders) {
                DirectoryContents.Add(folder);
                ContentsBox.AddChild(new TextLabel(ContentsBox, 0, 10, folder.Split('\\')[^1]));
            }
            foreach (var file in files) {
                DirectoryContents.Add(file);
                ContentsBox.AddChild(new TextLabel(ContentsBox, 0, 10, file.Split('\\')[^1]));
            }
            ((TextLabel)RootUI.GetChildByID("path")).Text = CurrentDirectory;
            if (DirectoryContents.Count > 0) ((TextLabel)ContentsBox.Children[HighlightedLine]).DefaultColor = Color.Yellow;

            DebugInfo.AddTempLine(() => $"Directory contents: {DirectoryContents.Count}", 2);
        }

        public static (List<string> folders, List<string> files) ListDirectoryContents(string directoryPath) {
            List<string> folderList = new List<string>();
            List<string> fileList = new List<string>();

            var directories = Directory.GetDirectories(directoryPath);
            var files = Directory.GetFiles(directoryPath);

            folderList.AddRange(directories.Select(d => Path.Combine(directoryPath, Path.GetFileName(d))));
            fileList.AddRange(files.Select(f => Path.Combine(directoryPath, Path.GetFileName(f))));

            folderList.Sort();
            fileList.Sort();

            return (folderList, fileList);
        }

        public static void MoveHighlightedLine(int amount) {
            if (DirectoryContents.Count == 0) return;
            ((TextLabel)ContentsBox.Children[HighlightedLine]).DefaultColor = Color.White;
            HighlightedLine = Util.PosMod(HighlightedLine + amount, DirectoryContents.Count);
            ((TextLabel)ContentsBox.Children[HighlightedLine]).DefaultColor = Color.Yellow;
        }

        public static void EnterDirectory() {
            if (DirectoryContents.Count == 0) return;
            if (File.GetAttributes(DirectoryContents[HighlightedLine]).HasFlag(FileAttributes.Directory)) {
                OpenDirectory(DirectoryContents[HighlightedLine]);
            }
        }

        public static void LeaveDirectory() {
            if (!string.IsNullOrEmpty(CurrentDirectory)) {
                var parentDirectory = Path.GetFullPath(Path.Combine(CurrentDirectory, @".."));
                OpenDirectory(parentDirectory);
            }
        }

        public static void Save() {
            var section = ScreenManager.GetFirstScreenOfType<Section2DScreen>().Section;
            if (section == null) {
                DebugInfo.AddTempLine(() => "No section to save.", 5);
                return;
            }

            string json = section.Serialize();

            if (!Directory.Exists(CurrentDirectory)) {
                DebugInfo.AddTempLine(() => "Current directory is invalid.", 5);
                return;
            }

            // var sectionIdentifier = section.Identifier == "" ? section.Identifier : "temp"; 
            string filePath = Path.Combine(CurrentDirectory, $"{SaveNameBox.Text.Split(".")[0]}.sqSection2D");

            try {
                File.WriteAllText(filePath, json);
                DebugInfo.AddTempLine(() => $"Section saved to {filePath}", 5);
            } catch (Exception e) {
                DebugInfo.AddTempLine(() => $"Error saving section: {e.Message}", 5);
            }

            OpenDirectory(CurrentDirectory);
        }

        public static void Load() {
            var path = DirectoryContents[HighlightedLine];

            try {
                if (Directory.Exists(path)) {
                    OpenDirectory(path);
                } else if (File.Exists(path)) {
                    if (path.EndsWith(".wav")) {
                        var name = SoundManager.AddSound(new FileInfo(path));
                        SoundManager.StartLoop(name);
                        DebugInfo.AddTempLine(() => $"Playing loop - {Path.GetFileName(path)}.", 5);
                    } else if (path.EndsWith(".png")) {
                        var sectionScreen = ScreenManager.GetFirstScreenOfType<Section2DScreen>();
                        if (sectionScreen is null) {
                            DebugInfo.AddTempLine(() => $"Error: Section2DScreen doesn't exist.", 5);
                            return;
                        }
                        try {
                            Texture2D texture = Texture2D.FromFile(SQ.GD, path);
                            if (sectionScreen.Editor.SelectedLayer is TextureLayer2D textureLayer) {
                                textureLayer.PaintImage((Vector2I)sectionScreen.Camera.GlobalMousePos.Value, texture, 1f, CommandManager.AddCommandChain(new CommandChain()));
                            }
                            DebugInfo.AddTempLine(() => $"Imported image - {Path.GetFileName(path)}.", 5);
                        } catch (Exception e) {
                            DebugInfo.AddTempLine(() => $"Error reading {Path.GetFileName(path)}: {e.Message}", 5);
                        }
                    } else if (path.EndsWith(".sqSection2D")) {
                        var sectionScreen = ScreenManager.GetFirstScreenOfType<Section2DScreen>();
                        if (sectionScreen is null) {
                            DebugInfo.AddTempLine(() => $"Error: Section2DScreen doesn't exist.", 5);
                            return;
                        }
                        // try {
                            string json = File.ReadAllText(path);
                            sectionScreen.Section = Section2D.Deserialize(json);
                            sectionScreen.Editor.SelectedLayer = sectionScreen.Section.LayerGroups.First().Value.Layers.Values.OfType<TextureLayer2D>().FirstOrDefault();
                            DebugInfo.AddTempLine(() => $"Loaded section from {Path.GetFileName(path)}", 5);
                        // } catch (Exception e) {
                        //     DebugInfo.AddTempLine(() => $"Error reading {Path.GetFileName(path)}: {e.Message}", 5);
                        // }
                    } else {
                        DebugInfo.AddTempLine(() => $"{Path.GetFileName(path)} is an unsupported file type.", 5);
                    }
                } else {
                    DebugInfo.AddTempLine(() => $"{Path.GetFileName(path)} does not exist.", 5);
                }

                DestroyUI();
            } catch (FileNotFoundException e) {
                DebugInfo.AddTempLine(() => e.ToString(), 5);
            }
        }

        public static void Export() {
            var path = Path.Combine(CurrentDirectory, $"{SaveNameBox.Text.Split("."[0])}.png");

            var layerGroup = ScreenManager.GetFirstScreenOfType<Section2DScreen>().Section.LayerGroups.First().Value;
            layerGroup.SaveTexture(path);
        }
    }
}