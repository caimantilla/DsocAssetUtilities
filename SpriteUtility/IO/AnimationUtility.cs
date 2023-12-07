using SpriteUtility.DBAM;

namespace SpriteUtility.IO
{
    public static class AnimationUtility
    {
        public static string GetAnimationAsJsonString(AnimationData animation)
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true,
            };

            var jsonString = System.Text.Json.JsonSerializer.Serialize(animation, options);
            return jsonString;
        }

        public static string GetAnimationAsYamlString(AnimationData animation)
        {
            var serializer = new YamlDotNet.Serialization.Serializer();
            string yamlString = serializer.Serialize(animation);
            return yamlString;
        }
        
        public static AnimationData ReadAnimationFromPath(string path)
        {
            AnimationData animation = new();

            using FileStream fileStream = new FileStream(path, FileMode.Open);
            using BinaryReader binaryReader = new BinaryReader(fileStream);
            
            ArrangementData[] arrangements = new ArrangementData[binaryReader.ReadUInt16()];
            for (int i = 0; i < arrangements.Length; i++)
            {
                ArrangementData arrangement = new();
                SpriteData[] sprites = new SpriteData[binaryReader.ReadUInt16()];

                for (int j = 0; j < sprites.Length; j++)
                {
                    SpriteData sprite = new SpriteData
                    {
                        // Unlike every other value, the sprite part's world position is signed.
                        WorldPositionX = binaryReader.ReadInt16(),
                        WorldPositionY = binaryReader.ReadInt16(),
                        
                        TextureSizeX = binaryReader.ReadUInt16(),
                        TextureSizeY = binaryReader.ReadUInt16(),
                        
                        TexturePositionX = binaryReader.ReadUInt16(),
                        TexturePositionY = binaryReader.ReadUInt16(),
                    };

                    sprites[j] = sprite;
                    
                    // Skip the next 4 bytes, there's some padding between sprites for some reason.
                    binaryReader.ReadUInt32();
                }

                arrangement.Sprites = sprites;
                arrangements[i] = arrangement;
            }

            animation.Arrangements = arrangements;

            // Some flags that come here.
            // I don't think they're particularly useful. I managed to change the visibility with them, perhaps something like the unit's shadow graphic is also set using this?
            ushort _uselessFlags = binaryReader.ReadUInt16();

            FrameData[] frames = new FrameData[binaryReader.ReadUInt16()];

            for (int i = 0; i < frames.Length; i++)
            {
                FrameData frame = new FrameData
                {
                    ArrangementIndex = binaryReader.ReadUInt16(),
                    StopLength = binaryReader.ReadUInt16(),
                };

                frames[i] = frame;
                
                // Seek to the next frame. Not sure what goes here.
                for (int j = 0; j < 8; j++)
                {
                    binaryReader.ReadUInt16();
                }
            }

            animation.Frames = frames;
            
            return animation;
        }



        public static System.Drawing.Bitmap[] GetAnimationArrangementBitmaps(AnimationData animation, System.Drawing.Bitmap partsBitmap)
        {
            int bmPosX = 0;
            int bmPosY = 0;
            int bmEndX = 0;
            int bmEndY = 0;

            
            for (int arrIdx = 0; arrIdx < animation.Arrangements.Length; arrIdx++)
            {
                ArrangementData arrangement = animation.Arrangements[arrIdx];
                for (int sprIdx = 0; sprIdx < arrangement.Sprites.Length; sprIdx++)
                {
                    SpriteData sprite = arrangement.Sprites[sprIdx];

                    bmPosX = Math.Min(bmPosX, sprite.WorldPositionX);
                    bmPosY = Math.Min(bmPosY, sprite.WorldPositionY);

                    bmEndX = Math.Max(bmEndX, sprite.WorldPositionX + sprite.TextureSizeX);
                    bmEndY = Math.Max(bmEndY, sprite.WorldPositionY + sprite.TextureSizeY);
                }
            }

            
            int bmSizeX = bmEndX - bmPosX;
            int bmSizeY = bmEndY - bmPosY;
            

            System.Drawing.Bitmap[] arrangementBitmaps = new System.Drawing.Bitmap[animation.Arrangements.Length];
            
            for (int arrIdx = 0; arrIdx < animation.Arrangements.Length; arrIdx++)
            {
                ArrangementData arrangement = animation.Arrangements[arrIdx];
                System.Drawing.Bitmap arrangementBitmap = new (bmSizeX, bmSizeY);
                
                // The sprites are listed in the opposite of draw order
                for (int sprIdx = arrangement.Sprites.Length - 1; -1 < sprIdx; sprIdx--)
                {
                    SpriteData sprite = arrangement.Sprites[sprIdx];

                    for (int x = sprite.TexturePositionX; x < (sprite.TexturePositionX + sprite.TextureSizeX); x++)
                    {
                        for (int y = sprite.TexturePositionY; y < (sprite.TexturePositionY + sprite.TextureSizeY); y++)
                        {
                            // Some sprites seem to have out-of-range texture sizes?? Like Decarabia's d045_01_f.dbam
                            // So perform this additional check before setting the pixel...
                            if (partsBitmap.Width <= x || partsBitmap.Height <= y)
                                continue;
                            
                            // Don't overlay invisible parts.
                            // Maybe the colors should be blended instead...?
                            System.Drawing.Color pixelColor = partsBitmap.GetPixel(x, y);
                            if (pixelColor.A == 0)
                                continue;
                            
                            int destinationX = x - sprite.TexturePositionX + sprite.WorldPositionX - bmPosX;
                            int destinationY = y - sprite.TexturePositionY + sprite.WorldPositionY - bmPosY;

                            arrangementBitmap.SetPixel(destinationX, destinationY, pixelColor);
                        }
                    }
                }

                arrangementBitmaps[arrIdx] = arrangementBitmap;
            }

            return arrangementBitmaps;
        }
    }
}
