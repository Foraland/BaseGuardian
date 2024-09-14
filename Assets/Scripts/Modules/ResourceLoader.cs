using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader : Singleton<ResourceLoader>
{
    const string texturePath = "Texture";
    private Dictionary<string, Sprite> spritesMap = new Dictionary<string, Sprite>();
    public Sprite GetSprite(string name)
    {
        if (!spritesMap.ContainsKey(name))
            return null;
        return spritesMap[name];
    }
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/LoadSprite")]
#endif
    public static void LoadSprite()
    {
        if (ResourceLoader.Ins == null)
        {
            Debug.LogError("ResourceLoader未实例化");
            return;
        }
        Sprite[] sprites = Resources.LoadAll<Sprite>(texturePath);
        for (int i = 0; i < sprites.Length; i++)
        {
            ResourceLoader.Ins.spritesMap.Add(sprites[i].name, sprites[i]);
        }
    }
}
