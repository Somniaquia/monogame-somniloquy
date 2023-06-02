namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// The SerializationManager handles compression/decompression of strings and wrtiting/reading those into files.
    /// </summary>
    public static class SerializationManager {
        public static Dictionary<Type, string> Directories { get; private set; } = new();

        public static void InitializeDirectories(params (Type, string)[] directories) {
            string baseDirectory = Directory.GetCurrentDirectory();

            foreach (var entry in directories) {
                Directories.Add(entry.Item1, $"{baseDirectory}/{entry.Item2}");
            }
        }

        public static void WriteToFile(Type type, string fileName, string serialized) {
            string directory = $"{Directories[type]}/{fileName}";

            using (FileStream compressedFileStream = File.Create(directory)) {
                using (GZipStream gzipStream = new GZipStream(compressedFileStream, CompressionMode.Compress)) {
                    using (StreamWriter writer = new StreamWriter(gzipStream)) {
                        writer.Write(serialized);
                    }
                }
            }
        }

        public static string ReadFromFile(Type type, string fileName) {
            string directory = $"{Directories[type]}/{fileName}";

            try {
                using (FileStream compressedFileStream = File.OpenRead(directory)) {
                    using (GZipStream gzipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress)) {
                        using (StreamReader reader = new StreamReader(gzipStream)) {
                            return reader.ReadToEnd();
                        }
                    }
                }
            } catch (Exception e) {
                System.Console.Out.WriteLine($"Failed to read file: {directory} \n {e.Message}");
                return "null";
            }
        }
    }
}