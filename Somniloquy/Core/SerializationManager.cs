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

    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;
    using MonoGame.Extended.Serialization;

    public static class SerializationManager {
        public static Dictionary<Type, string> Directories { get; private set; } = new();

        public static void InitializeDirectories(params (Type, string)[] directories) {
            string baseDirectory = Directory.GetCurrentDirectory();

            foreach (var entry in directories) {
                Directories.Add(entry.Item1, $"{baseDirectory}/{entry.Item2}");
            }
        }

        public static void Serialize<T>(object instance, string fileName) {
            if (!Directory.Exists($"{Directories[typeof(T)]}")) Directory.CreateDirectory($"{Directories[typeof(T)]}");
            string directory = $"{Directories[typeof(T)]}/{fileName}";

            JsonSerializerSettings settings = new() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            string serialized = JsonConvert.SerializeObject(instance, settings);
            
            using FileStream compressedFileStream = File.Create(directory);
            using GZipStream gzipStream = new(compressedFileStream, CompressionMode.Compress);
            using StreamWriter writer = new(gzipStream);
            writer.Write(serialized);

            // Console.WriteLine(serialized);
        }

        public static T Deserialize<T>(string fileName) {
            string directory = $"{Directories[typeof(T)]}/{fileName}";

            try {
                using FileStream compressedFileStream = File.OpenRead(directory);
                using GZipStream gzipStream = new(compressedFileStream, CompressionMode.Decompress);
                using StreamReader reader = new(gzipStream);

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new PointConverter());
                settings.Converters.Add(new DictionaryConverter<Point, string>());

                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), settings);
            } catch (Exception e) {
                System.Console.Out.WriteLine($"Failed to read file: {directory} \n {e.Message}");
                return default;
            }
        }
    }

    public class PointConverter : JsonConverter<Point> {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var jObject = JObject.Load(reader);
            int x = (int)jObject["X"];
            int y = (int)jObject["Y"];
            return new Point(x, y);
        }

        public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer) {
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

    public class DictionaryConverter<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue>> {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override IDictionary<TKey, TValue> ReadJson(JsonReader reader, Type objectType, IDictionary<TKey, TValue> existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var jObject = JObject.Load(reader);
            var dictionary = new Dictionary<TKey, TValue>();

            foreach (var property in jObject.Properties()) {
                var key = property.Name;
                var value = property.Value.ToObject<TValue>(serializer);
                dictionary.Add((TKey)Convert.ChangeType(key, typeof(TKey)), value);
            }

            return dictionary;
        }

        public override void WriteJson(JsonWriter writer, IDictionary<TKey, TValue> value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }

    public class Texture2DConverter : JsonConverter<Texture2D> {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override Texture2D ReadJson(JsonReader reader, Type objectType, Texture2D existingValue, bool hasExistingValue, JsonSerializer serializer) {
            // Load the JSON object
            var jObject = JObject.Load(reader);

            // Extract the necessary data for Texture2D creation
            var bounds = jObject["Bounds"];
            var width = (int)bounds["Width"];
            var height = (int)bounds["Height"];
            // Additional data extraction if needed

            var texture2D = new Texture2D(GameManager.GraphicsDeviceManager.GraphicsDevice, width, height);
            texture2D.SetData(data);

            return texture2D;
        }

        public override void WriteJson(JsonWriter writer, Texture2D value, JsonSerializer serializer) {
            // Create a JSON object to represent the Texture2D
            var jObject = new JObject();
            // Add necessary data to the object
            jObject.Add("Bounds", new JObject(new JProperty("Width", value.Width), new JProperty("Height", value.Height)));
            // Additional data serialization if needed

            // Write the JSON object to the writer
            jObject.WriteTo(writer);
        }
    }
}