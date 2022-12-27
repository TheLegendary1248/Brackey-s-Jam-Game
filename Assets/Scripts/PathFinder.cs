using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This component provides generic pathfinding for enemies. not yet implemented
/// </summary>
public class PathFinder : MonoBehaviour
{
    [System.Flags]
    public enum Obstacles { Terrain, Fire}
    public Vector2Int[] GetPathNow()
    {

        return null;
    }
    public void GetPath(ref Vector2Int[] pts, ref bool isReady) { }
}
