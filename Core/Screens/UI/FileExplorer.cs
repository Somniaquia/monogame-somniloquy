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
            InputManager.RegisterKeybind(Keys.Escape, (parameters) => { if (Active) DestroyUI(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(Keys.Tab, Keys.LeftShift, (parameters) => { if (Active) MoveHighlightedLine(1); }, TriggerOnce.Block);
            InputManager.RegisterKeybind(new object[] {Keys.LeftShift, Keys.Tab}, (parameters) => { if (Active) MoveHighlightedLine(-1); }, TriggerOnce.Block, true);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.S }, (parameters) => { if (Active) SaveSection(SaveNameBox?.Text.Split(".")[0]); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.L }, (parameters) => { if (Active) Load(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.E }, (parameters) => { if (Active) Export(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(Keys.Enter, Keys.LeftShift, (parameters) => { if (Active) EnterDirectory(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] {Keys.LeftShift, Keys.Enter}, (parameters) => { if (Active) LeaveDirectory(); }, TriggerOnce.True, true);
        }

        public static void BuildUI() {
            RootUI = new BoxUI(Util.ShrinkRectangle(new Rectangle(0, 0, SQ.WindowSize.X, SQ.WindowSize.Y), new(20))) { Identifier = "root" };
            Active = true;
            DebugInfo.Active = false;
            ScreenManager.GetFirstOfType<Section2DScreen>().Editor.ColorPicker.Active = false;
            
            RootUI.AddChild(new TextLabel(RootUI, 20, 5) { Identifier = "path" });
            var mainBox = RootUI.AddChild(new BoxUI(RootUI, 20, 0) { MainAxis = Axis.Horizontal, Identifier = "mainBox", MainAxisFill = true, });
                ContentsBox = mainBox.AddChild(new BoxUI(mainBox) { MainAxis = Axis.Vertical, Identifier = "contents", MainAxisShrink = true, });
                var previewBox = mainBox.AddChild(new BoxUI(mainBox) { Identifier = "previewBox" });
            var bottomBox = RootUI.AddChild(new BoxUI(RootUI, 20, 0) { Identifier = "bottomBox" });
                SaveNameBox = (TextLabel)bottomBox.AddChild(new TextLabel(bottomBox, 0, 20) { Identifier = "saveNameBox", Editable = true, } );
                var Button = bottomBox.AddChild(new BoxUI(bottomBox, 0, 20) { Identifier = "saveButton" });
        }

        public static void DestroyUI() {
            Active = false;
            DebugInfo.Active = true;
            ScreenManager.GetFirstOfType<Section2DScreen>().Editor.ColorPicker.Active = true;

            RootUI?.Destroy();
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
                ContentsBox.AddChild(new TextLabel(ContentsBox, 0, 10, folder.Split('\\')[^1]) { Focusable = false });
            }
            foreach (var file in files) {
                DirectoryContents.Add(file);
                ContentsBox.AddChild(new TextLabel(ContentsBox, 0, 10, file.Split('\\')[^1]) { Focusable = false });
            }
            ((TextLabel)RootUI.GetChildByID("path")).Text = CurrentDirectory;
            if (DirectoryContents.Count > 0) ((TextLabel)ContentsBox.Children[HighlightedLine]).DefaultColor = Color.Yellow;

            DebugInfo.AddTempLine(() => $"Directory contents: {DirectoryContents.Count}", 2);
            RootUI.PositionChildren();
            ContentsBox.ScrollValue = 0;
            ContentsBox.SmoothScrollValue = 0;
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

            if (ContentsBox.Overflowed) {
                var boxLength = ContentsBox.GetMaxLength(ContentsBox.MainAxis);
                var entryLength = ((BoxUI)ContentsBox.Children[HighlightedLine]).GetContentLength(ContentsBox.MainAxis, null);
                
                if (entryLength * HighlightedLine < ContentsBox.ScrollValue) ContentsBox.ScrollValue = entryLength * HighlightedLine;
                if (entryLength * HighlightedLine < ContentsBox.SmoothScrollValue) ContentsBox.SmoothScrollValue = entryLength * HighlightedLine;
                if (entryLength * (HighlightedLine + 1) > ContentsBox.ScrollValue + boxLength) ContentsBox.ScrollValue = entryLength * (HighlightedLine + 1) - boxLength;
                if (entryLength * (HighlightedLine + 1) > ContentsBox.SmoothScrollValue + boxLength) ContentsBox.SmoothScrollValue = entryLength * (HighlightedLine + 1) - boxLength;
            }
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

        public static void SaveSection(string saveName) {
            if (saveName == "") return;

            var section = ScreenManager.GetFirstOfType<Section2DScreen>().Section;
            if (section == null) {
                DebugInfo.AddTempLine(() => "No section to save.", 5);
                return;
            }

            section.Identifier = $"{saveName}.sqSection2D";
            string json = section.Serialize();

            if (!Directory.Exists(CurrentDirectory)) {
                DebugInfo.AddTempLine(() => "Current directory is invalid.", 5);
                return; // TODO: Create folders duh I am lazy
            }

            // var sectionIdentifier = section.Identifier == "" ? section.Identifier : "temp"; 
            string filePath = Path.Combine(CurrentDirectory, $"{saveName}.sqSection2D");

            try {
                File.WriteAllText(filePath, json);
                DebugInfo.AddTempLine(() => $"Section saved to {filePath}", 5);
                DestroyUI();
            } catch (Exception e) {
                DebugInfo.AddTempLine(() => $"Error saving section: {e.Message}", 5);
            }

            // OpenDirectory(CurrentDirectory);
        }

        public static void LoadImage(string path, LayerGroup2D layerGroup = null) {
            var layer = new TileLayer2D();

            if (layerGroup is null) ScreenManager.GetFirstOfType<Section2DScreen>().Section.AddLayer(layer);
            else layerGroup.AddLayer(layer);

                Texture2D texture = Texture2D.FromFile(SQ.GD, path);
                layer.PaintImage(Vector2I.Zero, texture, 1f, CommandManager.AddCommandChain(new CommandChain()));
            try {
                DebugInfo.AddTempLine(() => $"Imported image - {Path.GetFileName(path)}.", 5);
            } catch (Exception e) {
                DebugInfo.AddTempLine(() => $"Error reading {Path.GetFileName(path)}: {e.Message}", 5);
            }
        }

        public static void LoadImageDirectory(string path, LayerGroup2D layerGroup = null) {
            if (DirectoryContents.Count == 0) return;
            var (folders, files) = ListDirectoryContents(path);
            if (layerGroup is null) {
                layerGroup = new LayerGroup2D(Path.GetDirectoryName(path));
                ScreenManager.GetFirstOfType<Section2DScreen>().Section.AddLayer(layerGroup);
            }
            
            foreach (var folder in folders) {
                LoadImageDirectory(folder, layerGroup);
            }

            foreach (var file in files) {
                if (new string[] { ".png", ".jpg", ".jpeg" }.Contains(Path.GetExtension(file).ToLower())) {
                    LoadImage(file, layerGroup);
                }
            }
        }

        public static void Load() {
            var path = DirectoryContents[HighlightedLine];

            try {
                if (Directory.Exists(path)) {
                    LoadImageDirectory(path);
                    LayerTable.BuildUI();
                } else if (File.Exists(path)) {
                    if (path.EndsWith(".wav")) {
                        var name = SoundManager.AddSound(new FileInfo(path));
                        SoundManager.StartLoop(name);
                        DebugInfo.AddTempLine(() => $"Playing loop - {Path.GetFileName(path)}.", 5);
                    } else if (new string[] { ".png", ".jpg", ".jpeg" }.Contains(Path.GetExtension(path).ToLower())) {
                        LoadImage(path);
                        DestroyUI();
                        LayerTable.BuildUI();
                    } else if (path.EndsWith(".sqSection2D")) {
                        var sectionScreen = ScreenManager.GetFirstOfType<Section2DScreen>();
                        if (sectionScreen is null) {
                            DebugInfo.AddTempLine(() => $"Error: Section2DScreen doesn't exist.", 5);
                            return;
                        }
                        // try {
                            string json = File.ReadAllText(path);
                            sectionScreen.Section = Section2D.Deserialize(json);
                            sectionScreen.Editor.SelectedLayer = sectionScreen.Section.Layers.OfType<TextureLayer2D>().FirstOrDefault();
                            DebugInfo.AddTempLine(() => $"Loaded section from {Path.GetFileName(path)}", 5);
                        // } catch (Exception e) {
                        //     DebugInfo.AddTempLine(() => $"Error reading {Path.GetFileName(path)}: {e.Message}", 5);
                        // }
                        DestroyUI();
                        LayerTable.BuildUI();
                    } else {
                        DebugInfo.AddTempLine(() => $"{Path.GetFileName(path)} is an unsupported file type.", 5);
                    }
                } else {
                    DebugInfo.AddTempLine(() => $"{Path.GetFileName(path)} does not exist.", 5);
                }
            } catch (FileNotFoundException e) {
                DebugInfo.AddTempLine(() => e.ToString(), 5);
            }
        }

        public static void Export() {
            var path = Path.Combine(CurrentDirectory, $"{SaveNameBox.Text.Split("."[0])}.png");

            
        }
    }
}