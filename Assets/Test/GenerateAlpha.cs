using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Test
{
    public class GenerateAlpha:MonoBehaviour
    {
        private const string InputFile = @"E:\Resource\Sprite\muzzleFire2.png";

        public void Generate()
        {
            Texture2D srcTex = new Texture2D(0,0);
            srcTex.LoadImage(File.ReadAllBytes(InputFile));
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    Color col = srcTex.GetPixel(i,j);
                    col.a = col.r * 0.299f + col.g * 0.587f + col.b * 0.114f;
                    srcTex.SetPixel(i,j,col);
                }
            }

            string newPath = Path.GetDirectoryName(InputFile)+Path.GetFileNameWithoutExtension(InputFile)+".png";
            File.WriteAllBytes(newPath,srcTex.EncodeToPNG());
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 500, 300), "start"))
            {
                Generate();
            }
        }
    }
}
