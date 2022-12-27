using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Depth", menuName = "Scriptables/LevelComp", order = 1)]
public class LevelComp : ScriptableObject
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
