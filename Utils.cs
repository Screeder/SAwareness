using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;
using Rectangle = SharpDX.Rectangle;

namespace SAwareness
{
    static class Log
    {
        public static String File = "C:\\SAwareness.log";
        public static String Prefix = "Packet";

        public static void LogString(String text, String file = null, String prefix = null)
        {
            switch (text)
            {
                case "missile":
                case "DrawFX":
                case "Mfx_pcm_mis.troy":
                case "Mfx_bcm_tar.troy":
                case "Mfx_bcm_mis.troy":
                case "Mfx_pcm_tar.troy":
                    return;
            }
            LogWrite(text, file, prefix);
        }

        public static void LogPacket(byte[] data, String file = null, String prefix = null)
        {
            LogWrite(BitConverter.ToString(data), file, prefix);
        }

        private static void LogWrite(String text, String file = null, String prefix = null)
        {
            if (text == null)
                return;
            if (file == null)
                file = File;
            if (prefix == null)
                prefix = Prefix;
            using (StreamWriter stream = new StreamWriter(file, true))
            {
                stream.WriteLine(prefix + "@" + Environment.TickCount + ": " + text);
            }
        }
    }

    static class Common
    {
        public static bool IsOnScreen(Vector3 vector)
        {
            float[] screen = Drawing.WorldToScreen(vector);
            if (screen[0] < 0 || screen[0] > Drawing.Width || screen[1] < 0 || screen[1] > Drawing.Height)
                return false;
            return true;
        }

        public static Size ScaleSize(this Size size, float scale, Vector2 mainPos = default(Vector2))
        {
            size.Height = (int)(((size.Height - mainPos.Y) * scale) + mainPos.Y);
            size.Width = (int)(((size.Width - mainPos.X) * scale) + mainPos.X);
            return size;
        }
    }

    static class Download
    {
        public static String Host = "https://github.com/Screeder/SAwareness/raw/master/Sprites/SAwareness/";
        public static String Path = "CHAMP/";

        public static void DownloadFile(String hostfile, String localfile)
        {
            var webClient = new WebClient();
            webClient.DownloadFile(Host + Path + hostfile, localfile);
        }
    }

    static class SpriteHelper
    {
        public static Texture LoadTexture(String onlineFile, String subOnlinePath, String localPathFile, ref Texture texture, bool bForce = false)
        {
            if (!File.Exists(localPathFile))
            {
                String filePath = localPathFile;
                filePath = filePath.Remove(0, filePath.LastIndexOf("\\Sprites\\", System.StringComparison.Ordinal));
                try
                {
                    Download.Path = subOnlinePath;
                    Download.DownloadFile(onlineFile, localPathFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SAwareness: Path: " + onlineFile + " \nException: " + ex.ToString());
                }
            }
            if (File.Exists(localPathFile) && (bForce || texture == null))
            {
                try
                {
                    texture = Texture.FromFile(Drawing.Direct3DDevice, localPathFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SAwarness: Couldn't load texture: " + localPathFile + "\n Ex: " + ex.ToString());
                }
                if (texture == null)
                {
                    return null;
                }
            }
            return texture;
        }

    }

    static class DirectXDrawer
    {
        public static void DrawText(Font font, String text, Size size, Color color)
        {
            DrawText(font, text, size.Width, size.Height, color);
        }

        public static void DrawText(Font font, String text, int posX, int posY, Color color)
        {
            if (font == null || font.IsDisposed)
            {
                throw new SharpDXException("");
                return;
            }
            var rec = font.MeasureText(null, text, FontDrawFlags.Center);
            font.DrawText(null, text, posX + 1 + rec.X, posY, Color.Black);
            font.DrawText(null, text, posX + 1 + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX - 1 + rec.X, posY, Color.Black);
            font.DrawText(null, text, posX - 1 + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY, color);
        }

        public static void DrawSprite(Sprite sprite, Texture texture, Size size, float[] scale = null, float rotation = 0.0f)
        {
            DrawSprite(sprite, texture, size, Color.White, scale, rotation);
        }

        public static void DrawSprite(Sprite sprite, Texture texture, Size size, Color color, float[] scale = null, float rotation = 0.0f)
        {
            if (sprite != null && !sprite.IsDisposed && texture != null && !texture.IsDisposed)
            {
                Matrix matrix = sprite.Transform;
                var nMatrix = (scale != null ? Matrix.Scaling(scale[0], scale[1], 0) : Matrix.Scaling(1)) * Matrix.RotationZ(rotation) * Matrix.Translation(size.Width, size.Height, 0);
                sprite.Transform = nMatrix;
                sprite.Draw(texture, color);
                sprite.Transform = matrix;
            }
        }

        public static void DrawTransformSprite(Sprite sprite, Texture texture, Color color, Size size, float[] scale, float rotation, Rectangle? spriteResize)
        {
            if (sprite != null && texture != null)
            {
                Matrix matrix = sprite.Transform;
                var nMatrix = Matrix.Scaling(scale[0], scale[1], 0) * Matrix.RotationZ(rotation) * Matrix.Translation(size.Width, size.Height, 0);
                sprite.Transform = nMatrix;
                sprite.Draw(texture, color);
                sprite.Transform = matrix;
            }
        }

        public static void DrawTransformedSprite(Sprite sprite, Texture texture, Rectangle spriteResize, Color color)
        {
            if (sprite != null && texture != null)
            {
                sprite.Draw(texture, color);
            }
        }

        public static void DrawSprite(Sprite sprite, Texture texture, Size size, Color color, Rectangle? spriteResize)
        {
            if (sprite != null && texture != null)
            {
                sprite.Draw(texture, color, spriteResize, new Vector3(size.Width, size.Height, 0));
            }
        }

        public static void DrawSprite(Sprite sprite, Texture texture, Size size, Color color)
        {
            if (sprite != null && texture != null)
            {
                DrawSprite(sprite, texture, size, color, null);
            }
        }
    }
}
