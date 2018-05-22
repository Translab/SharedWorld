using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CombineTextures
{

    [MenuItem("Assets/SKS/Combine selected into array texture")]
    private static void CreateTexture3D()
    {
        List<Texture2D> textures = new List<Texture2D>();

        foreach (Object o in Selection.objects)
        {
            if (o.GetType() == typeof(Texture2D))
            {
                textures.Add((Texture2D)o);
            }
        }

        textures = textures.OrderBy(a => a.name).ToList();

        int xSize = textures[0].width;
        int ySize = textures[0].height;
        int zSize = textures.Count;

        uint zPow2 = (uint) zSize;
        zPow2--;
        zPow2 |= zPow2 >> 1;
        zPow2 |= zPow2 >> 2;
        zPow2 |= zPow2 >> 4;
        zPow2 |= zPow2 >> 8;
        zPow2 |= zPow2 >> 16;
        zPow2++;

        Color[] colorArray = new Color[xSize * ySize];


       Texture2DArray texture = new Texture2DArray(xSize, ySize, (int)zPow2, TextureFormat.RGBA32, true);
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    colorArray[x + (y * xSize)] = textures[z].GetPixel(x, y); 
                }
            }
            texture.SetPixels(colorArray, z);
        }


        texture.Apply();
        AssetDatabase.CreateAsset(texture, AssetDatabase.GetAssetPath(Selection.activeObject) + "3dTexture.asset");
    }

}
