using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
/// <summary>
/// The level generator. Contains methods for manipulating the world
/// </summary>
public class Level : MonoBehaviour
{
    //TODO :
    //ADD METHOD TO ALTER FUTURE GENERATION DURING GAME
    //ADD FLEXIBLE IMPLEMENTATION FOR EXTERNAL METHODS
    //MORE COMPLEX AND INDEPTH GENERATION FOR TERRAIN
    //DUNGEONS
    public static int levelSize = 128;
    public static Dictionary<int, int> healthMap = new Dictionary<int, int>();
    public static Dictionary<string, CustomTile> data = new Dictionary<string, CustomTile>();
    static Tilemap map;
    static Tilemap floor;
    static bool changedInFrame = false;
    CompositeCollider2D comp;
    public LevelTerrain[] levels;
    public static ushort depth;
    public static Vector3Int[] ladderLocations = new Vector3Int[0];
    public static BitArray[] solidTiles;
    public Slider slider;
    public Text depthIndicator;
    float startTime = 0;
    public static Level singleton;
    List<float> times = new List<float>();
    public static short Depth { get; }
    public void Start() //might wanna do that 'indexing' now than later
    {
        singleton = this;
        comp = GetComponentInChildren<CompositeCollider2D>();
        map = transform.GetChild(0).GetComponent<Tilemap>();
        floor = transform.GetChild(1).GetComponent<Tilemap>();
        if (map == null) Debug.LogError($"Map not found on level gameobject {map.name}");
        CustomTile[] t = Resources.LoadAll<CustomTile>("Tiles");
        string[] list = new string[t.Length]; int i = 0;
        foreach(CustomTile raw in t)
        {
            list[i] = raw.name;
            data.Add(raw.name, raw);
            i++;
        }
        Debug.Log($"{t.Length} tiles loaded : {string.Join(", ", list)}");
    }
    private void FixedUpdate()
    {
        if (changedInFrame) { comp.GenerateGeometry(); changedInFrame = false; }
    }
    public void Death()
    {
        float t = Time.time - startTime;
        MainMenu.singleton.ShowDeath(times.ToArray(), t);
    }
    public static void NextLevel() => singleton._NextLevel();
    void _NextLevel() => StartCoroutine(GenerateNewLevel());
    IEnumerator GenerateNewLevel()
    {
        //Prepare
        MainMenu.singleton.ShowLoadingScreen(true);
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Finish")) Destroy(g);
        GameObject dummy = (GameObject)Resources.Load("Enemies/Slime");
        map.origin = Vector3Int.zero;
        map.size = new Vector3Int(levelSize, levelSize, 0);
        map.ResizeBounds();
        floor.origin = Vector3Int.zero;
        floor.size = new Vector3Int(levelSize, levelSize, 0);
        floor.ResizeBounds();
        if (solidTiles == null || solidTiles.Length != levelSize) solidTiles = new BitArray[levelSize];
        //'n' Serve
        float xOff = Random.Range(0, 255f);
        float yOff = Random.Range(0, 255f);
        CustomTile[] walls = new CustomTile[levelSize * levelSize];
        CustomTile[] floors = new CustomTile[levelSize * levelSize];
        LevelTerrain level = levels[0];
        foreach(LevelTerrain i in levels)
        {
            if (i.level <= depth) level = i;
            else break;
        }
        
        int y = 0, x = 0, entitySpace = 0;
        while(y < levelSize) //Per tile gen
        {
            if (solidTiles[y] == null) solidTiles[y] = new BitArray(levelSize);
            else if (solidTiles[y].Length != levelSize) solidTiles[y].Length = levelSize;
            while(x < levelSize) //Tile in Column
            {
                float f = Mathf.PerlinNoise((x * level.scale) + xOff, (y * level.scale) + yOff);
                CustomTile tile = null;
#if false //Save for later: In depth terrain gen
                foreach(LevelTerrain.TerrainLayers layer in level.layers) //Check through each layer
                {
                    if (f > layer.composition[0].density) //Check if density is higher than first element density, aka Use this layer
                    {
                        foreach (LevelTerrain.TerrainLayers.LayerComp c in layer.composition)//Check through each composition
                        {
                            if (f > c.density) tile = c.tile; //If density is higher than element, use it
                            else break; //Remain to previous tile set
                        }
                        break;
                    } //Remain null if fail
                }
#endif
                bool isScatter = false;
                foreach (LevelTerrain.ScatterTiles s in level.scatter)
                {
                    if(s.minDensity < f && s.maxDensity > f)
                    {
                        if(s.chance > Random.value)
                        {
                            tile = s.tile;
                            isScatter = true;
                            break;
                        }
                    }
                }
                if (!isScatter)
                {
                    foreach (LevelTerrain.LayerComp c in level.comp) //Check each element in composition
                    {
                        if (f > c.density) tile = c.tile; //If density is higher than element, use it
                        else break; //Remain to previous tile set
                    }
                }

                walls[(y * levelSize) + x] = tile;
                floors[(y * levelSize) + x] = (tile == null || tile.colliderType != Tile.ColliderType.Grid) ? level.comp[0].tile : tile;
                solidTiles[y].Set(x, tile != null && tile.colliderType == Tile.ColliderType.Grid);
                if (entitySpace > level.enemySpacing & tile == null)
                {
                    Instantiate(dummy, new Vector2(x + 0.5f, y + 0.5f ), Quaternion.identity); entitySpace = 0; 
                }
                if (tile == null) entitySpace++;
                x++;
            }
            x = 0;
            y++;
            slider.value = y / (float)levelSize;
            yield return new WaitForEndOfFrame();
        }
        map.SetTilesBlock(new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(levelSize, levelSize, 1)), walls);
        floor.SetTilesBlock(new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(levelSize, levelSize, 1)), floors);

        if (depth == 0) Player.tf.position = new Vector2(levelSize / 2, levelSize / 2);

        //Fix this FUCKING bulshit another time : 
        List<Vector3Int> randLadders = new List<Vector3Int>();
        for (int i = 0; i < 2; i++) 
        {
            randLadders.Add(new Vector3Int(Random.Range(0, levelSize), Random.Range(0, levelSize), 0));
        }
        //Make space for player, and reuse block for ladders : Prepare
        Vector3Int player = new Vector3Int(Mathf.FloorToInt(Player.tf.position.x), Mathf.FloorToInt(Player.tf.position.y), 0);
        CustomTile sb = data["Stone_Brick"];
        CustomTile csb = data["Stone_Brick_Cracked"];
        CustomTile[] startBlock = new CustomTile[25];
        //Build Floor
        for (int i = 0; i < startBlock.Length; i++) startBlock[i] = Random.value > 0.2f ? sb : csb;
        floor.SetTilesBlock(new BoundsInt(player - new Vector3Int(2,2,0), new Vector3Int(5,5,1)), startBlock);
        foreach (Vector3Int ladder in randLadders) floor.SetTilesBlock(new BoundsInt(ladder - new Vector3Int(2, 2, 0), new Vector3Int(5, 5, 1)), startBlock);
        //Build Walls with openings
        int[] empty = new int[] {2,6,7,8,10,11,12,13,14,16,17,18,22};
        foreach (int i in empty) startBlock[i] = null;
        map.SetTilesBlock(new BoundsInt(player - new Vector3Int(2, 2, 0), new Vector3Int(5, 5, 1)), startBlock);
        GameObject ladderGB = (GameObject)Resources.Load("Ladder");
        foreach (Vector3Int ladder in randLadders)
        {
            map.SetTilesBlock(new BoundsInt(ladder - new Vector3Int(2, 2, 0), new Vector3Int(5, 5, 1)), startBlock);
            Instantiate(ladderGB, ladder + new Vector3(0.5f,0.5f,0), Quaternion.identity);
            ladderLocations = randLadders.ToArray();
        }
        yield return new WaitForEndOfFrame(); //Make sure map is updated so Composite Collider can generate geometry
        comp.GenerateGeometry();
        healthMap.Clear(); //Clear tile health values
        depthIndicator.text = depth.ToString();
        switch (depth)
        {
            case 0:
                times.Clear();
                startTime = Time.time;
                break;
            case 10:
            case 20:
            case 30:
            case 40:
            case 50:
                times.Add(startTime - Time.time);
                break;
        }
        depth++;
        MainMenu.singleton.ShowLoadingScreen(false);
    } //Split sections into methods
    public static CustomTile GetTile(Vector2 pos) => (CustomTile)map.GetTile(new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), 0));
    public static void SetTile(Vector2 pos, CustomTile tile)
    {
        Vector3Int aPos = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), 0);
        map.SetTile(aPos, tile);
        healthMap.Remove(TileAddress(aPos.x, aPos.y));
    }
    public static bool DamageTile(Vector2 pos, int dmg) => DamageTile(pos.x, pos.y, dmg, map.GetTile<CustomTile>(new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), 0)));
    /// <summary>
    /// Damages tile at x,y from 0,0 of tilemap with given tile.
    /// </summary>
    public static bool DamageTile(float x, float y, int dmg) => DamageTile(x, y, dmg, map.GetTile<CustomTile>(new Vector3Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y), 0)));
    /// <summary>
    /// Damages tile at x, y from 0,0 of tilemap with given tile. Note this one is mainly used for CircleDamageTiles so as to get performant results using GetTilesBlock
    /// </summary>
    /// <returns>Returns if tile was destroyed</returns>
    public static bool DamageTile(float x, float y, int dmg, CustomTile t) //OPTIMIZE--------------------------------------------------!!!!!!!!!!!!!!
    {
        int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);
        if (t == null) return false;
        int address = TileAddress(xi,yi);
        bool tileDestroyed = false;
        if (healthMap.ContainsKey(address)) //If tile has already been affected and in healthMap
        {
            if(healthMap[address] + t.hardness <= dmg) //A - Share def : Destroy if larger than hp
            {
                healthMap.Remove(address);
                map.SetTile(new Vector3Int(xi, yi, 0), null);
                tileDestroyed = true;
            }
            else healthMap[address] -= Mathf.Max(dmg - t.hardness, 0); //Else deduct hp from map
        }
        else if(dmg >= t.health + t.hardness) //A - Share def : Destroy if larger than hp
        {
            map.SetTile(new Vector3Int(xi, yi, 0), null);
            tileDestroyed = true;
        }
        else healthMap.Add(address, t.health - Mathf.Max(dmg - t.hardness, 0)); //Add to healthmap if non-existent

        t.RunTile(new Vector2(x,y), tileDestroyed); 
        if (tileDestroyed) changedInFrame = true;
        return tileDestroyed;
    }
    /// <summary>
    /// Damages tile from collision contact point. 
    /// This method deals with the fact that collision contact points slightly reside within the empty tile beside the tile actually hit.
    /// </summary>
    /// <param name="colPoint"></param>
    /// <param name="dmg"></param>
    public static void DamageTileFromPoint(Vector2 colPoint, int dmg)
    {
        Vector3Int gridPos = new Vector3Int(Mathf.FloorToInt(colPoint.x), Mathf.FloorToInt(colPoint.y), 0);
        CustomTile t = (CustomTile)map.GetTile(gridPos);
        if (t != null && t.colliderType == Tile.ColliderType.Sprite) DamageTile(colPoint, dmg); //If the point of collision is actually in a tile
        else
        {
            float xOff = colPoint.x - (gridPos.x + 0.5f); //Get point relative to center of cell
            float yOff = colPoint.y - (gridPos.y + 0.5f);
            if(Mathf.Abs(xOff) > Mathf.Abs(yOff)) colPoint.x += ((Mathf.Sign(xOff) / 2f) - xOff) * 2f; //If x is a larger absolute, reflect over x boundary
            else colPoint.y += ((Mathf.Sign(yOff) / 2f) - yOff) * 2f; //If y is a larger absolute, reflect over y boundary
            DamageTile(colPoint, dmg); 
        }
        
    }
    public static int CircleDamageTiles(Vector2 pos, float radius, int dmg)
    {
        int x = Mathf.RoundToInt(pos.x - radius);
        int y = Mathf.RoundToInt(pos.y - radius);
        int xRange = Mathf.RoundToInt(pos.x + radius) - x;
        int yRange = Mathf.RoundToInt(pos.y + radius) - y;
        int yIter = 0;
        TileBase[] chunk = map.GetTilesBlock(new BoundsInt(x, y, 0, xRange, yRange, 1));
        while (yIter < yRange)
        {
            int xIter = 0;
            while (xIter < xRange)
            {
                if ((new Vector2(xIter + x + 0.5f, yIter + y + 0.5f) - pos).sqrMagnitude < radius * radius)
                { 
                    DamageTile(xIter + x + 0.5f, yIter + y + 0.5f, dmg, (CustomTile)chunk[xIter + (yIter * xRange)]);
                }
                xIter++;
            }
            yIter++;
        }
        return 0;
    }
    public static bool CheckForTile(Vector2 pos) => map.GetTile(new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y),0)) != null;
    public int BoxClear(Vector2 pos, Vector2 extent) { return 0; }
    /// <summary>
    /// Returns int address of a tile. DOES NOT RESPECT SIGNED VALUES.
    /// </summary>
    public static int TileAddress(Vector2Int pos) => pos.x + (pos.y * levelSize);
    public static int TileAddress(int x, int y) => x + (y * levelSize);
    /// <summary>
    /// Returns int address of a tile in a block when using GetTilesBlock. DOES NOT RESPECT SIGNED VALUES
    /// </summary>
    /// <param name="xRange">Size of the block x-wise</param>
    public static int TileAddressBlock(int x, int y, int xRange) => x + (y * xRange);
    [System.Serializable]
    public struct LevelTerrain
    {
        public ushort level;
        public LayerComp[] comp;
        public ScatterTiles[] scatter;
        public float scale;
        public int enemySpacing;
        [System.Serializable]
        public struct ScatterTiles
        {
            public CustomTile tile;
            public float chance;
            public float minDensity;
            public float maxDensity;
        }
        [System.Serializable]
        public struct LayerComp
        {
            public CustomTile tile;
            public float density;
        }
    }
}

