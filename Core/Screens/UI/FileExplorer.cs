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
        
        public static string SavingName;
        public static bool OverwriteWarning;

        public static BoxUI RootUI;

        public static void Initialize() {
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.O }, (parameters) => { if (!Active) BuildUI(); OpenDirectory("c:\\Somnia\\Projects\\monogame-somniloquy\\Assets"); }, TriggerOnce.True, true);
            InputManager.RegisterKeybind(Keys.Tab, Keys.LeftShift, (parameters) => { if (Active) MoveHighlightedLine(1); }, TriggerOnce.Block);
            InputManager.RegisterKeybind(new object[] {Keys.LeftShift, Keys.Tab}, (parameters) => { if (Active) MoveHighlightedLine(-1); }, TriggerOnce.Block, true);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.S }, (parameters) => { if (Active) Save(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.E }, (parameters) => { if (Active) Export(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(Keys.Enter, Keys.LeftShift, (parameters) => { if (Active) EnterDirectory(); }, TriggerOnce.True);
            InputManager.RegisterKeybind(new object[] {Keys.LeftShift, Keys.Enter}, (parameters) => { if (Active) LeaveDirectory(); }, TriggerOnce.True, true);
            InputManager.RegisterKeybind(Keys.Escape, (parameters) => { if (Active) DestroyUI(); }, TriggerOnce.True);
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
            HighlightedLine += amount;
            HighlightedLine = Util.PosMod(HighlightedLine, DirectoryContents.Count);
        }

        public static void EnterDirectory() {
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

        public static void OpenDirectory(string directory) {
            CurrentDirectory = directory;
            DirectoryContents.Clear();
            HighlightedLine = 0;

            var (folders, files) = ListDirectoryContents(directory);
            
            foreach (var folder in folders) DirectoryContents.Add(folder);
            foreach (var file in files) DirectoryContents.Add(file);
    
            DebugInfo.AddTempLine(() => $"Directory contents: {DirectoryContents.Count}", 2);
        }

        public static void BuildUI() {
            RootUI = new BoxUI(Util.ShrinkRectangle(new Rectangle(0, 0, SQ.WindowSize.X, SQ.WindowSize.Y), new(20)));
            
            var topBox = RootUI.AddChild(new BoxUI(RootUI, 20, 20));
            var mainBox = RootUI.AddChild(new BoxUI(RootUI, 20, 0) { MainAxis = Axis.Horizontal});
                var contentListBox = mainBox.AddChild(new BoxUI(mainBox));
                var previewBox = mainBox.AddChild(new BoxUI(mainBox));
            var bottomBox = RootUI.AddChild(new BoxUI(RootUI, 20, 20));
                var SavingNameBox = bottomBox.AddChild(new BoxUI(bottomBox));
                var Button = bottomBox.AddChild(new BoxUI(bottomBox));
        }

        public static void DestroyUI() {
            RootUI = null;
        }

        public static void Save() { }

        public static void Export() { }
    }
}