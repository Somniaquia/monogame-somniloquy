namespace Somniloquy {
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using System.IO.Compression;

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
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.O }, _ => { ToggleFileExplorer(); }, TriggerOnce.True, true);
            InputManager.RegisterKeybind(Keys.Escape, _ => { if (Active) DestroyUI(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(Keys.Tab, Keys.LeftShift, _ => { if (Active) MoveHighlightedLine(1); }, TriggerOnce.Block);
            InputManager.RegisterKeybind(new object[] {Keys.LeftShift, Keys.Tab}, _ => { if (Active) MoveHighlightedLine(-1); }, TriggerOnce.Block, true);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.S }, _ => { if (Active) SaveSection(SaveNameBox?.Text.Split(".")[0]); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.L }, _ => { if (Active) Load(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.E }, _ => { if (Active) Export(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(Keys.Enter, Keys.LeftShift, _ => { if (Active) EnterDirectory(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] {Keys.LeftShift, Keys.Enter}, _ => { if (Active) LeaveDirectory(); }, TriggerOnce.True, true);
        }

        public static void BuildUI() {
            RootUI = new BoxUI(Util.ShrinkRectangle(new Rectangle(0, 0, SQ.WindowSize.X, SQ.WindowSize.Y), new(20))) { Identifier = "root" };
            Active = true;
            DebugInfo.Active = false;
            if (ScreenManager.GetFirstOfType<ColorPicker>() is not null) ScreenManager.GetFirstOfType<ColorPicker>().Active = false;
            LayerTable.DestroyUI();

            var directoryLabel = new TextLabel(RootUI, 20, 5) { Identifier = "path", PerpendicularAxisFill = true };
            var mainBox = new BoxUI(RootUI, 20, 0) { MainAxis = Axis.Horizontal, Identifier = "mainBox", MainAxisFill = true, PerpendicularAxisFill = true };
                ContentsBox = new BoxUI(mainBox) { MainAxis = Axis.Vertical, Identifier = "contents", MainAxisShrink = true, PerpendicularAxisFill = true };
                var previewBox = new BoxUI(mainBox) { Identifier = "previewBox" };
            var bottomBox = new BoxUI(RootUI, 20, 0) { Identifier = "bottomBox", PerpendicularAxisFill = true };
                SaveNameBox = new TextLabel(bottomBox, 0, 20) { Identifier = "saveNameBox", Editable = true, MainAxisFill = true, Text = "" };
                var Button = new BoxUI(bottomBox, 0, 20) { Identifier = "saveButton", PerpendicularAxisFill = true };
        }

        public static void DestroyUI() {
            Active = false;
            DebugInfo.Active = true;
            if (ScreenManager.GetFirstOfType<ColorPicker>() is not null) ScreenManager.GetFirstOfType<ColorPicker>().Active = true;
            LayerTable.BuildUI();

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
                _ = new TextLabel(ContentsBox, 0, 10, folder.Split('\\')[^1]) { Focusable = false, PerpendicularAxisFill = true };
            }
            foreach (var file in files) {
                DirectoryContents.Add(file);
                _ = new TextLabel(ContentsBox, 0, 10, file.Split('\\')[^1]) { Focusable = false, PerpendicularAxisFill = true };
            }
            ((TextLabel)RootUI.GetChildByID("path")).Text = CurrentDirectory;
            if (DirectoryContents.Count > 0) ((TextLabel)ContentsBox.Children[HighlightedLine]).DefaultColor = Color.Yellow;

            // DebugInfo.AddTempLine(() => $"Directory contents: {DirectoryContents.Count}", 2);
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
                var boxLength = ContentsBox.GetAvailableSpace(ContentsBox.MainAxis);
                var entryLength = ((BoxUI)ContentsBox.Children[HighlightedLine]).GetContentLength(ContentsBox.MainAxis);
                
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
            if (string.IsNullOrEmpty(saveName)) return;

            var section = ScreenManager.GetFirstOfType<Section2DScreen>().Section;
            if (section == null) {
                DebugInfo.AddTempLine(() => "No section to save.", 5);
                return;
            }

            section.Identifier = $"{saveName}.sqSection2D";
            string json = section.Serialize();

            if (!Directory.Exists(CurrentDirectory)) {
                DebugInfo.AddTempLine(() => "Current directory is invalid.", 5);
                return;
            }

            string filePath = Path.Combine(CurrentDirectory, $"{saveName}.sqSection2D");
            
            try {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
                using (StreamWriter writer = new StreamWriter(gzipStream)) {
                    writer.Write(json);
                }
                DebugInfo.AddTempLine(() => $"Section saved to {filePath}", 5);
                DestroyUI();
            } catch (Exception e) {
                DebugInfo.AddTempLine(() => $"Error saving section: {e.Message}", 5);
            }
        }

        public static void LoadSection(string path) {
            try {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                using (StreamReader reader = new StreamReader(gzipStream)) {
                    string json = reader.ReadToEnd();
                    var sectionScreen = ScreenManager.GetFirstOfType<Section2DScreen>();
                    sectionScreen.Section = Section2D.Deserialize(json);
                    sectionScreen.Editor.SelectedLayer = sectionScreen.Section.Root.GetSelfAndChildren().OfType<PaintableLayer2D>().FirstOrDefault();
                    DebugInfo.AddTempLine(() => $"Loaded section from {Path.GetFileName(path)}", 5);
                }
            } catch (Exception e) {
                DebugInfo.AddTempLine(() => $"Error loading section: {e.Message}", 5);
            }
        }

        public static void LoadImage(string path, Layer2D parent = null) {
            var layer = new TileLayer2D() { Identifier = Path.GetFileName(path) };

            if (parent is null) ScreenManager.GetFirstOfType<Section2DScreen>().Section.Root.AddLayer(layer);
            else parent.AddLayer(layer);

            try {
                Texture2D texture = Texture2D.FromFile(SQ.GD, path);
                layer.PaintImage(Vector2I.Zero, texture, 1f, CommandManager.AddCommandChain(new CommandChain()));
                // DebugInfo.AddTempLine(() => $"Imported image - {Path.GetFileName(path)}.", 5);
            } catch (Exception e) {
                DebugInfo.AddTempLine(() => $"Error reading {Path.GetFileName(path)}: {e.Message}", 5);
            }
        }

        public static void LoadImageDirectory(string path, Layer2D parent = null) {
            if (DirectoryContents.Count == 0) return;
            var (folders, files) = ListDirectoryContents(path);
            if (parent is null) {
                parent = new Layer2D(path.Split('\\')[^1].Split('.')[0]);
                ScreenManager.GetFirstOfType<Section2DScreen>().Section.Root.AddLayer(parent);
            } else {
                parent = parent.AddLayer(new Layer2D(path.Split('\\')[^1].Split('.')[0]));
            }
            
            foreach (var folder in folders) {
                LoadImageDirectory(folder, parent);
            }

            foreach (var file in files) {
                if (new string[] { ".png", ".jpg", ".jpeg" }.Contains(Path.GetExtension(file).ToLower())) {
                    LoadImage(file, parent);
                }
            }

            DestroyUI();
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
                        LoadSection(path);
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