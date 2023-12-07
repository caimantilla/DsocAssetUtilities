using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using SpriteUtility.DBAM;
using AnimatedGif;

namespace SpriteUtility.IO
{
    public static class CommonUtility
    {
        private sealed class AnimFilePair
        {
            public string Name = "";
            public string RootOutputDirectory = "";
            public string ArrangementImagesOutputDirectory = "";
            public string AnimPath = "";
            public string TexPath = "";
        }
        
        public static void WriteAllSpriteData(string inputDirectory, string outputDirectory, SpriteProcessingMode mode)
        {
            const string SeparatorHappy = "^o^";
            const string SeparatorWide = "--------------------------------------------------------------------------------";
            
            Dictionary<string, AnimFilePair> filePairs = new();
            
            string[] animFilePaths = Directory.GetFileSystemEntries(inputDirectory, "*.dbam", SearchOption.AllDirectories);
            string[] texFilePaths = Directory.GetFileSystemEntries(inputDirectory, "*.dbm", SearchOption.AllDirectories);

            for (int i = 0; i < animFilePaths.Length; i++)
            {
                string animPath = animFilePaths[i];
                string relativePath = Path.GetRelativePath(inputDirectory, animPath);
                string relativeDirPath = Path.GetDirectoryName(relativePath);
                string animName = Path.GetFileNameWithoutExtension(animPath);
                
                var pair = new AnimFilePair
                {
                    Name = animName,
                    RootOutputDirectory = Path.Join(outputDirectory, relativeDirPath),
                    ArrangementImagesOutputDirectory = Path.Join(outputDirectory, relativeDirPath, animName),
                    AnimPath = animPath,
                    TexPath = "",
                };

                if (!filePairs.ContainsKey(animName))
                    filePairs.Add(animName, pair);
            }

            for (int i = 0; i < texFilePaths.Length; i++)
            {
                string texPath = texFilePaths[i];
                string animName = Path.GetFileNameWithoutExtension(texPath);

                if (filePairs.ContainsKey(animName))
                {
                    var pair = filePairs[animName];
                    pair.TexPath = texPath;
                }
            }

            foreach (var pair in filePairs.Values)
            {
                if (!File.Exists(pair.AnimPath) || !File.Exists(pair.TexPath))
                    continue;
                
                const string NowWorkingOn = "Now working on: ";
                const string RootOutPathIs = "Root output path: ";
                const string AnimPathIs = "Animation path: ";
                const string TexPathIs = "Texture path: ";
                
                Console.WriteLine(NowWorkingOn + pair.Name);
                Console.WriteLine(RootOutPathIs + pair.RootOutputDirectory);
                Console.WriteLine(AnimPathIs + pair.AnimPath);
                Console.WriteLine(TexPathIs + pair.TexPath);
                Console.WriteLine(SeparatorHappy);

                Directory.CreateDirectory(pair.RootOutputDirectory);
                Directory.CreateDirectory(pair.ArrangementImagesOutputDirectory);
                Debug.Print("Directory created.");

                AnimationData animation = AnimationUtility.ReadAnimationFromPath(pair.AnimPath);
                Debug.Print("Animation created.");
                Bitmap partsBitmap = ImageUtility.GetDbmBitmapFromPath(pair.TexPath);
                Debug.Print("Parts bitmap created.");
                
                partsBitmap.Save(Path.Join(pair.RootOutputDirectory, pair.Name + "_parts.png"));
                
                Bitmap[] arrangementBitmaps = AnimationUtility.GetAnimationArrangementBitmaps(animation, partsBitmap);
                Debug.Print("Arrangement bitmaps created.");

                Bitmap plainHorizontalSheetBitmap = new Bitmap(arrangementBitmaps[0].Width * arrangementBitmaps.Length, arrangementBitmaps[0].Height);
                
                for (int arrIdx = 0; arrIdx < arrangementBitmaps.Length; arrIdx++)
                {
                    const string SavingTo = "Saving to: ";
                    
                    string savePath = Path.Join(pair.ArrangementImagesOutputDirectory, arrIdx.ToString().PadLeft(2, '0') + ".png");
                    
                    Console.WriteLine(SavingTo + savePath);
                    
                    Bitmap arrBitmap = arrangementBitmaps[arrIdx];
                    arrBitmap.Save(savePath);

                    using (Graphics graphics = Graphics.FromImage(plainHorizontalSheetBitmap))
                    {
                        graphics.DrawImage(arrBitmap, arrBitmap.Width * arrIdx, 0);
                    }
                }
                
                // Save as a sheet
                plainHorizontalSheetBitmap.Save(Path.Join(pair.RootOutputDirectory, pair.Name + "_sheet.png"));

                // Make a GIF out of it as well
                using (var gif = new AnimatedGifCreator(Path.Join(pair.RootOutputDirectory, pair.Name + "_anim.gif"), 16))
                {
                    for (int frameIdx = 0; frameIdx < animation.Frames.Length; frameIdx++)
                    {
                        FrameData frameData = animation.Frames[frameIdx];
                        Bitmap arrBitmap = arrangementBitmaps[frameData.ArrangementIndex];
                        
                        gif.AddFrame(arrBitmap, frameData.StopLength * 16, GifQuality.Bit8);
                    }
                }

                using (var gif = new AnimatedGifCreator(Path.Join(pair.RootOutputDirectory, pair.Name + "_anim_4x.gif"),
                           16))
                {
                    for (int frameIdx = 0; frameIdx < animation.Frames.Length; frameIdx++)
                    {
                        FrameData frameData = animation.Frames[frameIdx];
                        Bitmap arrBitmap = arrangementBitmaps[frameData.ArrangementIndex];
                        
                        using (Graphics g = Graphics.FromImage())
                    }
                }

                string jsonAnimationString = AnimationUtility.GetAnimationAsJsonString(animation);
                File.WriteAllText(Path.Join(pair.RootOutputDirectory, pair.Name + "_base_anim_data.json"), jsonAnimationString);
                
                Console.WriteLine(SeparatorWide);
            }
            
            Console.WriteLine("Finished dumping units!");
        }
    }
}
