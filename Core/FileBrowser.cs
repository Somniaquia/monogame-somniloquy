namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public static class FileBrowser {
        private static List<string> lines = new List<string>();
        public static string CurrentDirectory;
        public static int HighlightedLine;
        public static bool Active = false;

        public static void Initialize() {
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.O }, (parameters) => { Active = true; OpenDirectory("c:\\Somnia\\Projects\\monogame-somniloquy\\Assets"); }, true, true);
            InputManager.RegisterKeybind(Keys.Tab, Keys.LeftShift, (parameters) => { if (Active) MoveHighlightedLine(1); }, true);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.S }, (parameters) => { if (Active) Save(); }, true);
            InputManager.RegisterKeybind(new object[] {Keys.LeftShift, Keys.Tab}, (parameters) => { if (Active) MoveHighlightedLine(-1); }, true, true);
            InputManager.RegisterKeybind(Keys.Enter, Keys.LeftShift, (parameters) => { if (Active) SelectDirectory(); }, true);
            InputManager.RegisterKeybind(new object[] {Keys.LeftShift, Keys.Enter}, (parameters) => { if (Active) LeaveDirectory(); }, true, true);
            InputManager.RegisterKeybind(Keys.Escape, (parameters) => { if (Active) Active = false; }, true);
        }

        public static (List<string> folders, List<string> files) ListDirectoryContents(string directoryPath) {
            List<string> folderList = new List<string>();
            List<string> fileList = new List<string>();

            try {
                var directories = Directory.GetDirectories(directoryPath);
                var files = Directory.GetFiles(directoryPath);

                folderList.AddRange(directories.Select(d => Path.Combine(directoryPath, Path.GetFileName(d))));
                fileList.AddRange(files.Select(f => Path.Combine(directoryPath, Path.GetFileName(f))));

                folderList.Sort();
                fileList.Sort();
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return (folderList, fileList);
        }

        public static void MoveHighlightedLine(int line) {
            HighlightedLine += line;
            HighlightedLine = Util.PosMod(HighlightedLine, lines.Count);
        }

        public static void SelectDirectory() {
            var path = lines[HighlightedLine];
            try {
                if (Directory.Exists(path)) {
                    OpenDirectory(path);
                } else if (File.Exists(path)) {
                    if (path.EndsWith(".wav")) {
                        var name = SoundManager.AddSound(new FileInfo(path));
                        SoundManager.StartLoop(name);
                        DebugInfo.AddTempLine(() => $"Playing loop 「{Path.GetFileName(path)}」.", 5);
                    } else if (path.EndsWith(".sqSection2D")) {
                        var sectionScreen = ScreenManager.GetFirstScreenOfType<Section2DScreen>();
                        if (sectionScreen is null) {
                            DebugInfo.AddTempLine(() => $"Error: Section2DScreen doesn't exist.", 5);
                            return;
                        }
                        try {
                            string json = File.ReadAllText(path);
                            sectionScreen.Section = Section2D.Deserialize(json);
                            sectionScreen.Editor.SelectedLayer = sectionScreen.Section.LayerGroups.First().Value.Layers.OfType<TextureLayer2D>().FirstOrDefault();
                            DebugInfo.AddTempLine(() => $"Loaded section from {Path.GetFileName(path)}.", 5);
                        } catch (Exception e) {
                            DebugInfo.AddTempLine(() => $"Error reading {Path.GetFileName(path)}: {e.Message}", 5);
                        }
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

        public static void LeaveDirectory() {
            if (!string.IsNullOrEmpty(CurrentDirectory)) {
                var parentDirectory = Path.GetFullPath(Path.Combine(CurrentDirectory, @".."));
                OpenDirectory(parentDirectory);
            }
        }

        public static void OpenDirectory(string directory) {
            lines.Clear();
            HighlightedLine = 0;

            CurrentDirectory = directory;
            var (folders, files) = ListDirectoryContents(directory);
            
            foreach (var folder in folders) {
                lines.Add(folder);
            }
            foreach (var file in files) {
                lines.Add(file);
            }

            DebugInfo.AddTempLine(() => $"Directory contents: {lines.Count}", 2);
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

            var sectionIdentifier = section.Identifier == "" ? section.Identifier : "temp"; 
            string filePath = Path.Combine(CurrentDirectory, $"{sectionIdentifier}.sqSection2D");

            try {
                File.WriteAllText(filePath, json);
                DebugInfo.AddTempLine(() => $"Section saved to {filePath}.", 5);
            } catch (Exception e) {
                DebugInfo.AddTempLine(() => $"Error saving section: {e.Message}", 5);
            }

            OpenDirectory(CurrentDirectory);
        }


        public static void Draw(SpriteFont font) {
            if (!Active) return;
            Vector2 position = new Vector2(SQ.WindowSize.X - 1, 1);
            SQ.SB.DrawString(font, CurrentDirectory + "\\", position - new Vector2(font.MeasureString(CurrentDirectory + "\\    ").X, 0), Color.White);
            position.Y += 20;
            for (int i = 0; i < lines.Count; i++) {
                Color color = i == HighlightedLine ? Color.Yellow : Color.White;
                var name = Path.GetFileName(lines[i]);
                try{
                    SQ.SB.DrawString(font, name, position - new Vector2(font.MeasureString(name).X, 0), color);
                } catch (ArgumentException) {
                    Debug.WriteLine(name);
                }
                position.Y += 18;
            }
        }
    }
}