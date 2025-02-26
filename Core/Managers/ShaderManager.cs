namespace Somniloquy {
    using System;
    using System.IO;
    using System.Diagnostics;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public static class ShaderManager {
        private static string ShaderDirectory = "Assets/Shaders/";

        private static FileSystemWatcher ShaderWatcher;
        private static Effect FallbackShader;

        public static event Action<string, Effect> ShaderUpdated;

        public static void LoadContent(Effect fallbackShader) {
            FallbackShader = fallbackShader;
        }

        public static void Initialize() {
            ShaderWatcher = new FileSystemWatcher(ShaderDirectory, "*.fx") {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };
            ShaderWatcher.Changed += OnShaderChanged;
            ShaderWatcher.Created += OnShaderChanged;

            ShaderWatcher.EnableRaisingEvents = true;
        }

        private static void OnShaderChanged(object sender, FileSystemEventArgs e) {
            // Delay to avoid file lock issues
            System.Threading.Thread.Sleep(100);

            string shaderPath = e.FullPath;

            try {
                // Recompile shader
                Effect newShader = CompileShader(shaderPath);

                // Notify listeners (like ETFractalScreen) that the shader has updated
                ShaderUpdated?.Invoke(Path.GetFileNameWithoutExtension(shaderPath), newShader);
            } catch (Exception ex) {
                // Handle compilation errors and fall back
                DebugInfo.AddTempLine(() => $"Shader compilation failed: {ex.Message}", 10);
                ShaderUpdated?.Invoke(Path.GetFileName(shaderPath), FallbackShader);
            } finally {
                Debug.WriteLine($"Shader file changed: {shaderPath}");
            }
        }

        private static Effect CompileShader(string shaderPath) {
            // Run the MonoGame content pipeline or external `fxc.exe` for shader compilation
            string compiledPath = Path.ChangeExtension(shaderPath, ".mgfx");

            // Example using fxc.exe (DirectX HLSL compiler)
            var processInfo = new ProcessStartInfo {
                FileName = "C:\\Users\\Somni\\.nuget\\packages\\dotnet-mgcb-editor-windows\\3.8.1.303\\content\\mgfxc.exe",
                Arguments = $"\"{shaderPath}\" \"{compiledPath}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo)) {
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0) {
                    throw new Exception(errors);
                }
            }

            byte[] buffer = File.ReadAllBytes(compiledPath);
            return new Effect(SQ.GD, buffer);
        }
    }
}