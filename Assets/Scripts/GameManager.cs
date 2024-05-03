using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class SkinPattern
{
    public string name;
    public Texture2D pattern, display;
    public RawImage button;
}

[System.Serializable]
public class ColorTuple
{
    public string name;
    public Color primary, secondary;
}

[System.Serializable]
public class Outfit
{
    public enum OutfitType {All, Top, Bottom};

    public string name;
    public Texture2D outfit;
    public OutfitType type;

    public Outfit() { }

    public Outfit(string name, Texture2D outfit)
    {
        this.name = name;
        this.outfit = outfit;
    }
}

public class GameManager : MonoBehaviour
{
    public List<SkinPattern> patterns;
    public List<ColorTuple> colors, maskColors;
    public List<Outfit> outfits;

    [Header("Prefabs")]

    public GameObject bodyRoot;
    public Material mat;
    public GameObject button;

    [Header("Containers")]

    public GameObject colorCont;
    public GameObject patternCont, maskCont, topCont, bottomCont;

    [Header("Buttons")]

    public Button colorButton;
    public Button maskButton, outfitButton, exportButton;

    [Header("Menus")]

    public GameObject colorMenu;
    public GameObject maskMenu, outfitMenu;

    [Header("Camera Stuff")]

    public Camera outfitCam;
    public GameObject outfitBottom, outfitTop;

    ColorTuple bCol, mCol;
    Texture2D pattern, top, bottom;
    List<GameObject> models;
    Vector3 mousePos;
    
    // Start is called before the first frame update
    void Start()
    {
        var loaded = FileManager.GetAll(Application.dataPath + "/Skins");

        colors = loaded.c;
        maskColors = loaded.m;
        patterns = loaded.p;
        outfits = loaded.o;

        pattern = patterns[0].pattern;
        mCol = maskColors[0];

        outfits.Insert(0, new Outfit("None", new Texture2D(64,64)));
        top = outfits[0].outfit;
        bottom = outfits[0].outfit;

        MakeButtons();
        UpdateColor(colors[0]);
    }


    void MakeButtons()
    {
        //Color Buttons
        foreach (var item in colors)
        {
            var ret = MakeButton(colorCont, SkinMaker.ColorButtonTexture(450, 200, item.primary, item.secondary), item.name);
            ret.b.onClick.AddListener(delegate { UpdateColor(item); } );
        }

        //Pattern Buttons
        foreach (var item in patterns)
        {
            var ret = MakeButton(patternCont, item.display, item.name);
            ret.b.onClick.AddListener(delegate { UpdatePattern(item.pattern); });
            item.button = ret.r;
        }

        //Mask Color Buttons
        foreach (var item in maskColors)
        {
            var ret = MakeButton(maskCont, SkinMaker.ColorButtonTexture(450, 200, item.primary, item.secondary), item.name);
            ret.b.onClick.AddListener(delegate { UpdateMask(item); });
        }

        //Outfit Buttons
        models = new List<GameObject>();
        for (int i = 0; i < outfits.Count; i++)
        {
            var item = outfits[i];

            //Outfit Top
            if (item.type == Outfit.OutfitType.All || item.type == Outfit.OutfitType.Top)
            {
                var txt = MakeRender(outfitTop, item.outfit, new Vector3(-20, -20 * (i + 1), -20));
                var ret = MakeButton(topCont, txt.r, item.name);
                models.Add(txt.obj);

                ret.b.onClick.AddListener(delegate { UpdateTop(item.outfit); });
            }

            //Outfit Bottom
            if (item.type == Outfit.OutfitType.All || item.type == Outfit.OutfitType.Bottom)
            {
                var txt = MakeRender(outfitBottom, item.outfit, new Vector3(20, -20 * (i + 1), -20));
                var ret = MakeButton(bottomCont, txt.r, item.name);
                models.Add(txt.obj);

                ret.b.onClick.AddListener(delegate { UpdateBottom(item.outfit); });
            }
        }

        //Menu Buttons
        colorButton.onClick.AddListener(delegate { MenuSwitch(0); });
        maskButton.onClick.AddListener(delegate { MenuSwitch(1); });
        outfitButton.onClick.AddListener(delegate { MenuSwitch(2); });
        exportButton.onClick.AddListener(Export);
    }


    public (Button b, RawImage r) MakeButton(GameObject container, Texture txt, string name)
    {
        var cont = Instantiate(button, container.transform);
        var dims = cont.transform.GetChild(0);
        var butt = dims.GetComponent<Button>();
        var img = dims.GetComponent<RawImage>();
        img.texture = txt;
        var text = cont.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
        text.text = name;

        return (butt, img);
    }

    public (RenderTexture r, GameObject obj) MakeRender(GameObject model, Texture2D skin, Vector3 camPos)
    {
        var cam = Instantiate(outfitCam, camPos, Quaternion.identity);
        var txt = new RenderTexture(450, 200, 16, RenderTextureFormat.ARGB32);
        cam.targetTexture = txt;

        var modelInst = Instantiate(model, cam.transform);
        modelInst.transform.localPosition = new Vector3(0,0,5);
        foreach (Transform item in modelInst.transform)
        {
            var block = new MaterialPropertyBlock();
            var renderer = item.GetComponent<Renderer>();
            renderer.GetPropertyBlock(block);
            block.SetTexture("_BaseMap", skin);
            renderer.SetPropertyBlock(block);
        }

        return (txt, modelInst);
    }

    public void UpdateColor(ColorTuple col)
    {
        foreach (var item in patterns)
        {
            item.button.texture = item.display.ApplyColors(col.primary, col.secondary);
        }

        bCol = col;
        UpdateSkin();
    }

    public void UpdatePattern(Texture2D pic)
    {
        pattern = pic;
        UpdateSkin();
    }

    public void UpdateMask(ColorTuple col)
    {
        mCol = col;
        UpdateSkin();
    }

    public void UpdateTop(Texture2D pic)
    {
        top = pic;
        UpdateSkin();
    }

    public void UpdateBottom(Texture2D pic)
    {
        bottom = pic;
        UpdateSkin();
    }

    // Update is called once per frame
    void UpdateSkin()
    {
        var texture = pattern.ApplyColors(bCol.primary, bCol.secondary);
        texture = texture.ApplyFace(mCol.primary, mCol.secondary);
        texture = texture.Combine(top, bottom);
        mat.mainTexture = texture;
    }

    public void MenuSwitch(int index)
    {
        var menuList = new List<GameObject> { colorMenu, maskMenu, outfitMenu };

        foreach (var item in menuList)
        {
            item.SetActive(false);
        }

        menuList[index].SetActive(true);
    }


    public void Export()
    {
        byte[] bytes = ((Texture2D)mat.mainTexture).EncodeToPNG();
        var dirPath = Application.dataPath + "/RenderOutput";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }

        System.IO.File.WriteAllBytes(dirPath + "/R_" + Random.Range(0, 100000) + ".png", bytes);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private void Update()
    {
        if(!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(0))
        {
            var dRot = new Vector3(Input.mousePosition.y - mousePos.y, mousePos.x - Input.mousePosition.x, 0) * Time.deltaTime * 25;
            var rot = bodyRoot.transform.rotation.eulerAngles + dRot;
            rot.x = Mathf.Clamp((rot.x + 90) % 360, 5, 175) - 90;
            bodyRoot.transform.rotation = Quaternion.Euler(rot);

            foreach (var item in models)
            {
                item.transform.rotation = Quaternion.Euler(rot);
            }
        }

        mousePos = Input.mousePosition;
    }
}
