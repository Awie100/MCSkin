using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FileManager
{
    public static (List<ColorTuple> c, List<ColorTuple> m, List<SkinPattern> p, List<Outfit> o) GetAll(string path)
    {
        var c = GetColors(path + "/Colors.txt");
        var m = GetColors(path + "/MaskColors.txt");
        var p = GetSkinPatterns(path + "/Patterns");
        var o = GetOutfits(path + "/Outfits");

        return (c, m, p, o);
    }

    public static List<ColorTuple> GetColors(string path)
    {
        var colors = new List<ColorTuple>();

        if (!File.Exists(path))
        {
            var file = File.Create(path);
            file.Dispose();
        }
        var colorsTxt = LoadColors(path);

        foreach (var line in colorsTxt)
        {
            var color = new ColorTuple();
            var propList = line.Split(',');

            color.name = propList[0];
            if (!ColorUtility.TryParseHtmlString(propList[1], out color.primary)) color.primary = Color.magenta;
            if (!ColorUtility.TryParseHtmlString(propList[2], out color.secondary)) color.secondary = Color.black;

            colors.Add(color);
        }

        return colors;
    }

    public static List<SkinPattern> GetSkinPatterns(string path)
    {
        var patterns = new List<SkinPattern>();

        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        DirectoryInfo info = new DirectoryInfo(path);
        var folders = info.GetDirectories();
        foreach (var item in folders)
        {
            SkinPattern skin = new SkinPattern();
            skin.name = item.Name;

            var dir = item.FullName + "/pattern.png";
            skin.pattern = LoadPNG(dir);
            if (skin.pattern == null) skin.pattern = new Texture2D(64, 64);

            dir = item.FullName + "/display.png";
            skin.display = LoadPNG(dir);
            if (skin.display == null) skin.display = new Texture2D(64, 64);

            patterns.Add(skin);
        }

        return patterns;
    }

    public static List<Outfit> GetOutfits(string path)
    {
        var outfits = new List<Outfit>();

        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        DirectoryInfo info = new DirectoryInfo(path);
        var folders = info.GetDirectories();
        foreach (var item in folders)
        {
            Outfit skin = new Outfit();
            skin.name = item.Name;

            var files = item.GetFiles();
            if(files.Length > 0)
            {
                switch(files[0].Name)
                {
                    case "top.png":
                        skin.type = Outfit.OutfitType.Top;
                        break;
                    case "bottom.png":
                        skin.type = Outfit.OutfitType.Bottom;
                        break;
                    default:
                        skin.type = Outfit.OutfitType.All;
                        break;
                }

                skin.outfit = LoadPNG(files[0].FullName);
                outfits.Add(skin);
            }

        }

        return outfits;
    }



    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    public static List<string> LoadColors(string filePath)
    {
        List<string> colors = new List<string>();
        StreamReader txtStm = new StreamReader(filePath);

        while (!txtStm.EndOfStream)
        {
            string txtLn = txtStm.ReadLine();
            colors.Add(txtLn);
        }

        txtStm.Close();
        return colors;
    }
}
