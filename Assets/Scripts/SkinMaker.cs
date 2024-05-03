using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkinMaker
{
    static int[,] invout = new int[,]
    {
        {2,2,0,0,0,0,1,1},
        {2,2,3,3,3,1,1,1},
        {0,0,0,0,0,0,0,0},
        {0,0,0,0,1,1,1,1}
    };

    public static Texture2D Combine(this Texture2D baseTex, Texture2D topTex, Texture2D bottomTex)
    {
        if (baseTex.width != topTex.width || baseTex.height != bottomTex.height)
            throw new System.InvalidOperationException("Combine only works with two equal sized images");
        var baseData = baseTex.GetPixels();
        var topData = topTex.GetPixels();
        var bottomData = bottomTex.GetPixels();
        int count = baseData.Length;
        var rData = new Color[count];
        for (int i = 0; i < count; i++)
        {
            switch (invout[i / (64 * 16), i / 8 % 8])
            {
                case 0:
                    rData[i] = baseData[i];
                    break;
                case 1:
                    rData[i] = topData[i];
                    break;
                case 2:
                    rData[i] = bottomData[i];
                    break;
                case 3:
                    rData[i] = topData[i].a > 0 ? topData[i] : bottomData[i];
                    break;
            }
        }
        
        return SharpTexture(baseTex.width, baseTex.height, rData);
    }

    public static Texture2D ApplyColors(this Texture2D pic, Color col1, Color col2)
    {
        var picData = pic.GetPixels();
        var count = picData.Length;
        for (int i = 0; i < count; i++)
        {
            Color pCol = picData[i];
            var alpha = pCol.a;
            var lrp = pCol.r;
            pCol = col1 * lrp + col2 * (1 - lrp);
            pCol.a = alpha;
            picData[i] = pCol;
        }
        return SharpTexture(pic.width, pic.height, picData);
    }

    public static Texture2D ColorButtonTexture(int w, int h, Color p, Color s)
    {
        var arr = new Color[w * h];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = (w - i%w) + (i/w) > h*0.75 ? p : s;
        }
        return SharpTexture(w, h, arr);
    }

    public static Texture2D ApplyFace(this Texture2D pic, Color a, Color b)
    {
        var map = new int[,]
        {
            {-1,1,1,1,1,-1},
            {1,1,1,1,1,1},
            {1,0,1,1,0,1},
            {1,0,1,1,0,1},
            {1,1,1,1,1,1},
            {-1,1,1,1,1,-1}
        };

        var picData = pic.GetPixels();

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                var index = 64 * (i + 49) + (j + 9);
                switch (map[i, j])
                {
                    case 0:
                        picData[index] = b;
                        break;
                    case 1:
                        picData[index] = a;
                        break;
                    default:
                        break;
                }
            }
        }

        return SharpTexture(pic.width,pic.height,picData);
    }

    public static Texture2D SharpTexture(int w, int h, Color[] pix)
    {
        var res = new Texture2D(w,h);
        res.wrapMode = TextureWrapMode.Clamp;
        res.filterMode = FilterMode.Point;
        res.SetPixels(pix);
        res.Apply();
        return res;
    }
}
