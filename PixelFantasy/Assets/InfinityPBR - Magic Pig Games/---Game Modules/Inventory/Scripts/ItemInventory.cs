using System;
using UnityEngine;

/*
 * INFINITY PBR - https://infinitypbr.com
 * Join the Discord for support & discussion with the community: https://discord.com/invite/cmZY2tH
 * Scripting documentation: https://infinitypbr.gitbook.io/infinity-pbr/
 * Youtube videos, tutorials, demos, and integrations: https://www.youtube.com/channel/UCzATh2-NC_xZSGnhZF-cFaw
 * All assets on the Asset Store: https://assetstore.unity.com/publishers/4645?aid=1100lxWw&pubref=p88
 */

namespace InfinityPBR.Modules.Inventory
{
    [Serializable]
    public class ItemInventory : MonoBehaviour
    {
        [Header("Required")]
        public GameObject inGamePrefab;
    }
}
