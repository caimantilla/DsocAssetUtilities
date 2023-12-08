using System.Drawing;
using System.Drawing.Drawing2D;
using DBAM;

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

                using (var arrGraphic = System.Drawing.Graphics.FromImage(arrangementBitmap))
                {
                    arrGraphic.InterpolationMode = InterpolationMode.NearestNeighbor;
                    arrGraphic.SmoothingMode = SmoothingMode.None;
                    arrGraphic.CompositingMode = CompositingMode.SourceOver;

                    // The sprites are listed in the opposite of draw order, so iterate through them backwards.
                    for (int sprIdx = arrangement.Sprites.Length - 1; -1 < sprIdx; sprIdx--)
                    {
                        SpriteData sprite = arrangement.Sprites[sprIdx];
                        System.Drawing.Bitmap spriteBitmap = new(sprite.TextureSizeX, sprite.TextureSizeY);

                        using (var sprGraphic = System.Drawing.Graphics.FromImage(spriteBitmap))
                        {
                            sprGraphic.DrawImage(partsBitmap, 
                                0, 0, new Rectangle(
                                    sprite.TexturePositionX, 
                                    sprite.TexturePositionY, 
                                    sprite.TexturePositionX + sprite.TextureSizeX, 
                                    sprite.TexturePositionY + sprite.TextureSizeY), GraphicsUnit.Pixel
                                );
                        }

                        if (sprite.FlipH)
                        {
                            spriteBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                        
                        arrGraphic.DrawImage(spriteBitmap,
                            sprite.WorldPositionX - bmPosX,
                            sprite.WorldPositionY - bmPosY,
                            new Rectangle(0, 0, spriteBitmap.Width, spriteBitmap.Height),
                            GraphicsUnit.Pixel
                        );
                        
                        
                    }
                }

                arrangementBitmaps[arrIdx] = arrangementBitmap;
            }

            return arrangementBitmaps;
        }
    }
}
