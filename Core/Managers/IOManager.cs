namespace Somniloquy {
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Xna.Framework.Graphics;

    public static class IOManager {
        public static async Task SaveTextureToPngAsync(Texture2D texture, string path) {
            await Task.Run(() => {
                using var fileStream = new FileStream(path, FileMode.Create);
                texture.SaveAsPng(fileStream, texture.Width, texture.Height);
            });
        }
    }
}