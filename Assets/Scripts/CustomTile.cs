using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class CustomTile : Tile
{
    public ushort health;
    public ushort burnLife;
    public byte flammability;
    public ushort hardness;
    [Tooltip("Tells if the tile has a method associated with it. May make automatic")]
    public bool runFunc;
    public GameObject particleSys;
    public ObjectRef.ObjConstruct[] spawn;
    static Dictionary<string, Queue<GameObject>> particlePool = new Dictionary<string, Queue<GameObject>>();
    
    public void RunTile(Vector2 pos, bool destroyedTile)
    {
        if(particleSys != null && Settings.TerrainParticles == true) //Particle spawning and pooling
        {
            if (particlePool.ContainsKey(particleSys.name))//If pool does exist
            {
                Queue<GameObject> pool = particlePool[particleSys.name];
                if (pool.Peek().activeSelf)//If object is still active and cannot be used
                {
                    GameObject g = Instantiate(particleSys, pos, Quaternion.identity);
                    pool.Enqueue(g);
                    
                }
                else //If the object is NOT active and can be used
                {
                    GameObject g = pool.Dequeue();
                    g.transform.SetPositionAndRotation(pos, Quaternion.identity);
                    g.SetActive(true);
                    g.GetComponent<ParticleSystem>().Play();
                    pool.Enqueue(g);
                }
            }
            else //If pool doesn't exist
            {
                Queue<GameObject> t = new Queue<GameObject>();
                particlePool.Add(particleSys.name, t);
                t.Enqueue(Instantiate(particleSys, pos, Quaternion.identity));
            }
        }
        if (!destroyedTile) return;
        switch (name) //ADD REFLECTION TO GET METHODS
        {
            case "TNT":
                TileFunctions.TNT(pos);
                break;
            case "Wood_Support":
                TileFunctions.Wood_Support(pos);
                break;
            default:
                break;
        }
    }

#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/CustomTile")]
    public static void CreateTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Tile", "New Tile", "Asset", "Save Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CustomTile>(), path);
    }
#endif
    class TileFunctions
    {
        public static void TNT(Vector2 pos) => Explosion.SummonExplosion(pos, 4, 7, 6, Health.MiscSrc.Explosion,"Normal");
        public static void Wood_Support(Vector2 pos)
        {
            List<CustomTile> choice = new List<CustomTile>();
            CustomTile t;
            if ((t = Level.GetTile(pos + Vector2.left)) != null && t.name != "Wood_Support") choice.Add(t);
            if ((t = Level.GetTile(pos + Vector2.right)) != null && t.name != "Wood_Support") choice.Add(t);
            if ((t = Level.GetTile(pos + Vector2.down)) != null && t.name != "Wood_Support") choice.Add(t);
            if ((t = Level.GetTile(pos + Vector2.up)) != null && t.name != "Wood_Support") choice.Add(t);
            if (choice.Count == 0) Level.SetTile(pos, Level.data["Dirt"]);
            else Level.SetTile(pos, choice[Random.Range(0, choice.Count - 1)]);
        }
        public void LuckyBlock(Vector2 pos) { }
    }
}
[System.Serializable]
public class TileStruct
{
    public ushort health;
}
public class ObjectRef
{
    public enum Type
    {
        Tile,
        Item,
        Entity,
        Effect,
        Particle,
        Empty
    }
    [System.Serializable]
    public struct ObjConstruct
    {
        public ObjectRef.Type type;
        public string name;
        public int startRange;
        public int endRange;
        public float chance;
        public ObjConstruct[] sub;
    }
}
[CustomEditor(typeof(CustomTile))]
public class CustomTileEditor: Editor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        CustomTile example = (CustomTile)target;
        if (example == null || example.sprite == null)
            return null;
        // example.PreviewIcon must be a supported format: ARGB32, RGBA32, RGB24,
        // Alpha8 or one of float formats
        Texture2D tex = new Texture2D(width, height);
        EditorUtility.CopySerialized(example.sprite.texture, tex);
        return tex;
    }
}