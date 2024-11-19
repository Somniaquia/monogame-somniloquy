namespace Somniloquy {
    using System;
    using System.IO;
    using System.Threading.Tasks;
    
    using Microsoft.Xna.Framework.Graphics;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    public static class IOManager {
        public static void SaveTextureDataAsPngAsync(Texture2D texture, string path) {
            Microsoft.Xna.Framework.Color[] textureData;
            textureData = new Microsoft.Xna.Framework.Color[texture.Width * texture.Height];
            texture.GetData(textureData);

            byte[] pixelData = new byte[textureData.Length * 4];
            for (int i = 0; i < textureData.Length; i++) {
                pixelData[i * 4 + 0] = textureData[i].R;
                pixelData[i * 4 + 1] = textureData[i].G;
                pixelData[i * 4 + 2] = textureData[i].B;
                pixelData[i * 4 + 3] = textureData[i].A;
            }
            
            Task.Run(() => {
                using var image = Image.LoadPixelData<Rgba32>(pixelData, texture.Width, texture.Height);
                image.SaveAsPng(path);
            });
        }
    }
}