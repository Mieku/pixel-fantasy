// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Controllers;
// using DataPersistence;
// using UnityEngine;
// using UnityEngine.Tilemaps;
//
// [RequireComponent(typeof(Tilemap))]
// public class PersistentTilemap : MonoBehaviour
// {
//     public TilemapLayer TilemapLayer;
//     
//     [SerializeField] Tilemap _tilemap;
//
//     private void Awake()
//     {
//         _tilemap = GetComponent<Tilemap>();
//     }
//
//     public object CaptureState()
//     {
//         var bounds = _tilemap.cellBounds;
//         List<TileBase> tiles = new List<TileBase>();
//         List<Vector3Int> poses = new List<Vector3Int>();
//         List<Color> colours = new List<Color>();
//
//         for (int x = bounds.min.x; x < bounds.max.x; x++)
//         {
//             for (int y = bounds.min.y; y < bounds.max.y; y++)
//             {
//                 var temp = _tilemap.GetTile(new Vector3Int(x, y, 0));
//                 var colour = _tilemap.GetColor(new Vector3Int(x, y, 0));
//                 if (temp != null)
//                 {
//                     tiles.Add(temp);
//                     poses.Add(new Vector3Int(x, y, 0));
//                     colours.Add(colour);
//                 }
//             }
//         }
//
//         return new MapData
//         {
//             TilemapLayer = TilemapLayer,
//             Tiles = tiles,
//             TilePoses = poses,
//             TileColours = colours,
//         };
//     }
//
//     public void RestoreState(object data)
//     {
//         var state = (MapData)data;
//
//         _tilemap.ClearAllTiles();
//
//         for (int i = 0; i < state.Tiles.Count; i++)
//         {
//             _tilemap.SetTile(state.TilePoses[i], state.Tiles[i]);
//             _tilemap.SetColor(state.TilePoses[i], state.TileColours[i]);
//         }
//     }
//
//     public struct MapData
//     {
//         public TilemapLayer TilemapLayer;
//         public List<TileBase> Tiles;
//         public List<Vector3Int> TilePoses;
//         public List<Color> TileColours;
//     }
// }
