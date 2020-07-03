using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CombineWindow
{
    public class AnimationCombine
    {
        private struct Coord
        {
            public int x;
            public int y;

            public Coord(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public static void Start(string fromDirectory, string saveDirectory)
        {
            Start(GetTextures(fromDirectory), saveDirectory);
        }

        public static void Start(List<Sprite> sprites, string saveDirectory)
        {
            List<Texture2D> textures = new List<Texture2D>();
            sprites.ForEach(x => textures.Add(x.texture));
            Start(textures, saveDirectory);
        }

        public static void Start(List<Texture2D> textureList, string saveDirectory)
        {
            try
            {
                Coord newMapCoord = GetNewmapCoord(textureList);
                Texture2D newMap = new Texture2D(newMapCoord.x * textureList.Count, newMapCoord.y);

                int mapIndex = 0;
                foreach (var curMap in textureList)
                {
                    Coord startCoord = new Coord((newMapCoord.x - curMap.width) >> 1, (newMapCoord.y - curMap.height) >> 1);

                    for (int i = 0; i < curMap.width; i++)
                        for (int j = 0; j < curMap.height; j++)
                            newMap.SetPixel(startCoord.x + i + mapIndex * newMapCoord.x, startCoord.y + j, curMap.GetPixel(i, j));
                    mapIndex++;
                }
                SaveTexture(newMap, saveDirectory + "\\" + Path.GetFileName(saveDirectory) + "_x" + newMapCoord.x + "_y" + newMapCoord.y + ".png");
            }
            catch (Exception)
            {
                Debug.LogError("Failed");
                throw;
            }
            Debug.Log("Success");
        }

        private static Coord GetNewmapCoord(List<Texture2D> list)
        {
            Coord result = new Coord(4, 4);
            foreach (var item in list)
            {
                if (item.width > result.x)
                {
                    int i = item.width % 4;
                    result.x = i == 0 ? item.width : item.width - i + 4;
                }
                if (item.height > result.y)
                {
                    int i = item.height % 4;
                    result.y = i == 0 ? item.height : item.height - i + 4;
                }
            }
            return result;
        }

        private static List<Texture2D> GetTextures(string directory)
        {
            SortedList<int, Texture2D> result = new SortedList<int, Texture2D>();
            foreach (var item in Directory.GetFiles(directory))
            {
                int id = int.Parse(Path.GetFileNameWithoutExtension(item));
                Texture2D texture = new Texture2D(0, 0);
                List<byte> byteList = new List<byte>();
                FileStream fileStream = new FileStream(item, FileMode.Open);

                byte[] buffer = new byte[1024];
                int readCount = 0;

                while ((readCount = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    for (int i = 0; i < readCount; i++)
                        byteList.Add(buffer[i]);

                texture.LoadImage(byteList.ToArray());
                result.Add(id, texture);
                fileStream.Close();
            }
            return result.Values.ToList();
        }

        private static void SaveTexture(Texture2D texture, string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.CreateNew);
            byte[] buffer = texture.EncodeToPNG();
            fileStream.Write(buffer, 0, buffer.Length);
            fileStream.Close();
        }
    }
}
