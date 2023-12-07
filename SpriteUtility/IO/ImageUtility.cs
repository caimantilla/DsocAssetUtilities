using System.Diagnostics;
using Scarlet.IO;
using Scarlet.IO.ImageFormats;

namespace SpriteUtility.IO
{
    public static class ImageUtility
    {
        public static System.Drawing.Bitmap GetDbmBitmapFromPath(string dbmPath)
        {
            Debug.Print($"Loading DMPBM {dbmPath}");

            var originalImage = new DMPBM();
            originalImage.Open(dbmPath);
            
            System.Drawing.Bitmap bitmap = originalImage.GetBitmap();

            // Assume that the most common color is the background/transparent
            
            System.Collections.Generic.Dictionary<System.Drawing.Color, int> colorOccurences = new();

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    System.Drawing.Color currentColor = bitmap.GetPixel(x, y);
                    
                    if (colorOccurences.ContainsKey(currentColor))
                    {
                        colorOccurences[currentColor] += 1;
                    }
                    else
                    {
                        colorOccurences.Add(currentColor, 1);
                    }
                }
            }
            
            System.Drawing.Color mostCommonColor = System.Drawing.Color.Transparent;
            int greatestOccurences = 0;
            
            foreach (var color in colorOccurences.Keys)
            {
                int occurences = colorOccurences[color];
                if (greatestOccurences < occurences)
                {
                    greatestOccurences = occurences;
                    mostCommonColor = color;
                }
            }

            System.Drawing.Imaging.ColorPalette palette = bitmap.Palette;
            
            for (int colIndex = 0; colIndex < palette.Entries.Length; colIndex++)
            {
                System.Drawing.Color col = palette.Entries[colIndex];
                // The most common color should be made transparent
                if (col == mostCommonColor)
                {
                    palette.Entries[colIndex] = System.Drawing.Color.FromArgb(0, col.R, col.G, col.B);
                }
                // While every other color should be made opaque
                else
                {
                    palette.Entries[colIndex] = System.Drawing.Color.FromArgb(255, col.R, col.G, col.B);
                }
            }

            bitmap.Palette = palette;
            return bitmap;
        }
    }
}
