using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using NaughtyAttributes;

namespace FoxX.TheCollector
{
    public class ItemPlacer : MonoBehaviour
    {
        [Header(" Elements ")]
        [SerializeField] private ItemLevelData[] _itemDatas;

        [Header(" Settings ")]
        [SerializeField] private BoxCollider _spawnZone;
        [SerializeField] private int _seed;

        public ItemLevelData[] GetGoals()
        {
            List<ItemLevelData> goals = new List<ItemLevelData>();

            foreach (ItemLevelData data in _itemDatas)
                if (data.IsGoal)
                    goals.Add(data);

            return goals.ToArray();
        }

        public Item[] GetItems()
            => GetComponentsInChildren<Item>();
        

#region Item Generation

#if UNITY_EDITOR
        
        [Button]
        private void GenerateItems()
        {
            // Destroy all previous elements
            while(transform.childCount > 0)
            {
                Transform t = transform.GetChild(0);
                t.SetParent(null);
                DestroyImmediate(t.gameObject);
            }

            Random.InitState(_seed);

            // We need to make sure that we have 3 elements of each
            for (int i = 0; i < _itemDatas.Length; i++)
            {
                for (int j = 0; j < _itemDatas[i].Amount; j++)
                {
                    Vector3 spawnPosition = GetSpawnPosition();

                    Item itemInstance = PrefabUtility.InstantiatePrefab(_itemDatas[i].ItemPrefab, transform) as Item;
                    itemInstance.transform.position = spawnPosition;
                    itemInstance.transform.rotation = Quaternion.Euler(Random.onUnitSphere * 360f);
                }
            }
        }

        private Vector3 GetSpawnPosition()
        {
            float x = Random.Range(-_spawnZone.size.x / 2, _spawnZone.size.x / 2);
            float y = Random.Range(-_spawnZone.size.y / 2, _spawnZone.size.y / 2);
            float z = Random.Range(-_spawnZone.size.z / 2, _spawnZone.size.z / 2);

            Vector3 localSpawnPoint = _spawnZone.center + new Vector3(x, y, z);
            Vector3 spawnPosition = transform.TransformPoint(localSpawnPoint);

            return spawnPosition;
        }

#endif

#endregion

    }
}