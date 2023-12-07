namespace SpriteUtility
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string DbamPath = @"C:\Users\cmsca\Development\Projects\dsoc\Game\Content\Unit\PC\Atsuro\p002_00_f.dbam";
            const string DbmImagePath = @"C:\Users\cmsca\Development\Projects\devil-return\original\3ds\us\romfs\target\Data\Unit\pc\p002\p002_00_f.dbm";
            
            DBAM.AnimationData animation = IO.AnimationUtility.ReadAnimationFromPath(DbamPath);
            System.Drawing.Bitmap partsBitmap = IO.ImageUtility.GetDbmBitmapFromPath(DbmImagePath);

            System.Drawing.Bitmap[] arrangementBitmaps = IO.AnimationUtility.GetAnimationArrangementBitmaps(animation, partsBitmap);

            for (int i = 0; i < arrangementBitmaps.Length; i++)
            {
                System.Drawing.Bitmap arrangementBitmap = arrangementBitmaps[i];
                arrangementBitmap.Save($@"C:\Users\cmsca\Pictures\Funny\atsuro\{i}.png");
            }
        }
    }
}
