namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Content;

    public static class SerializationManager {
        public static Dictionary<Type, string> Directories { get; private set; } = new();

        public static void InitializeDirectories(params (Type, string)[] directories) {
            string baseDirectory = Directory.GetCurrentDirectory();

            foreach (var entry in directories) {
                Directories.Add(entry.Item1, $"{baseDirectory}/{entry.Item2}");
            }
        }

        public static void Serialize<T>(object instance, string fileName) {
            try {
                var directory = fileName[^4..].Equals(".txt") ? fileName : fileName + ".txt";
                Console.WriteLine(directory);
                // Required for storing references to 'parent classes' without causing a loop.
                JsonSerializerSettings settings = new() { PreserveReferencesHandling = PreserveReferencesHandling.Objects };

                string serialized = JsonConvert.SerializeObject(instance, settings);

                using FileStream compressedFileStream = File.Create(directory);
                using GZipStream gzipStream = new(compressedFileStream, CompressionMode.Compress);
                using StreamWriter writer = new(gzipStream);
                writer.Write(serialized);
                writer.Flush();
            }
            catch (Exception ex) {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        public static T Deserialize<T>(string fileName) {
            //string directory = $"{Directories[typeof(T)]}/{fileName}";
            var directory = fileName;

            try {
                using FileStream compressedFileStream = File.OpenRead(directory);
                using GZipStream gzipStream = new(compressedFileStream, CompressionMode.Decompress);
                using StreamReader reader = new(gzipStream);

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new Vector2IConverter());
                
                var deserializedObject = JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), settings);
                reader.Close();
                return deserializedObject;
            } catch (Exception e) {
                Console.Out.WriteLine($"Failed to read file: {directory} \n {e.Message}");
                return default;
            }
        }
    }

    public class Vector2IConverter : JsonConverter<Vector2I> {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override Vector2I ReadJson(JsonReader reader, Type objectType, Vector2I existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var jObject = JObject.Load(reader);
            int x = (int)jObject["X"];
            int y = (int)jObject["Y"];
            return new Vector2I(x, y);
        }

        public override void WriteJson(JsonWriter writer, Vector2I value, JsonSerializer serializer) {
            var jObject = new JObject {
                { "X", value.X },
                { "Y", value.Y }
            };

            jObject.WriteTo(writer);
        }
    }

    public class RectangleConverter : JsonConverter<Rectangle> {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override Rectangle ReadJson(JsonReader reader, Type objectType, Rectangle existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var jObject = JObject.Load(reader);

            var x = (int)jObject["X"];
            var y = (int)jObject["Y"];
            var width = (int)jObject["Width"];
            var height = (int)jObject["Height"];

            var rectangle = new Rectangle(x, y, width, height);

            return rectangle;
        }

        public override void WriteJson(JsonWriter writer, Rectangle value, JsonSerializer serializer) {
            var jObject = new JObject {
                { "X", value.X },
                { "Y", value.Y },
                { "Width", value.Width },
                { "Height", value.Height }
            };

            jObject.WriteTo(writer);
        }
    }

    public class ChunksConverter : JsonConverter<Dictionary<Vector2I, Tile2D[,]>> {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override Dictionary<Vector2I, Tile2D[,]> ReadJson(JsonReader reader, Type objectType, Dictionary<Vector2I, Tile2D[,]> existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var jObject = JObject.Load(reader);
            var chunks = new Dictionary<Vector2I, Tile2D[,]>();

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new Vector2IConverter());

            foreach (var property in jObject.Properties()) {
                var key = JsonConvert.DeserializeObject<Vector2I>(property.Name, settings);
                var value = property.Value.ToObject<Tile2D[,]>(serializer);
                chunks.Add(key, value);
            }

            return chunks;
        }

        public override void WriteJson(JsonWriter writer, Dictionary<Vector2I, Tile2D[,]> value, JsonSerializer serializer) {
            var jObject = new JObject();

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new Vector2IConverter());

            foreach (var kvp in value) {
                var Vector2I = JsonConvert.SerializeObject(kvp.Key, settings);
                var chunkToken = JToken.FromObject(kvp.Value, serializer);
                jObject.Add(Vector2I, chunkToken);
            }
            
            jObject.WriteTo(writer);
        }
    }

    public class Texture2DConverter : JsonConverter<Texture2D> {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override Texture2D ReadJson(JsonReader reader, Type objectType, Texture2D existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var jObject = JObject.Load(reader);

            var width = (int)jObject["Bounds"]["Width"];
            var height = (int)jObject["Bounds"]["Height"];
            var base64Data = (string)jObject["PixelData"]["PixelData"];
            byte[] data = Convert.FromBase64String(base64Data);
            // for (int i = 0; i < data.Length; i++) {
            //     Console.Write($" {data[i]}");
            // }
            var texture2D = new Texture2D(SQ.GD, width, height);
            texture2D.SetData(data);

            return texture2D;
        }

        public override void WriteJson(JsonWriter writer, Texture2D value, JsonSerializer serializer) {
            var data = new byte[value.Width * value.Height * 4];
            value.GetData(data);

            var jObject = new JObject {
                { "Bounds", new JObject(new JProperty("Width", value.Width), new JProperty("Height", value.Height)) },
                { "PixelData", new JObject(new JProperty("PixelData", data)) }
            };

            jObject.WriteTo(writer);
        }
    }
}