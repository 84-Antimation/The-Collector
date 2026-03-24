using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoxX.TheCollector
{
    public class MergeManager : MonoBehaviour
    {
        [Header(" Animation Settings ")]
        [SerializeField] private float _goUpDuration;
        [SerializeField] private float _goUpDistance;
        [SerializeField] private LeanTweenType _goUpTweenType;

        [SerializeField] private float _smashDuration;
        [SerializeField] private LeanTweenType _smashTweenType;

        [Header(" Effects ")]
        [SerializeField] private ParticleSystem _mergeParticle;

        [Header(" Actions ")]
        public static Action Merged;

        private void Awake()
        {
            ItemSpotsManager.ItemsStartedMerge += OnItemsStartedMerge;
        }

        private void OnDestroy()
        {
            ItemSpotsManager.ItemsStartedMerge -= OnItemsStartedMerge;
        }

        private void OnItemsStartedMerge(List<Item> items)
        {
            // Move these items up a bit, then smash them together
            for (int i = 0; i < items.Count; i++)
            {
                Vector3 targetPos = items[i].transform.position + items[i].transform.up * _goUpDistance;
                Action callback = null;

                if (i == 0)
                    callback = () => SmashItems(items);

                LeanTween.move(items[i].gameObject, targetPos, _goUpDuration)
                    .setEase(_goUpTweenType)
                    .setOnComplete(callback);
            }
        }

        private void SmashItems(List<Item> items)
        {
            // The items are already sorted from left to right
            // THis might not be the case anymore

            items.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            float targetX = items[1].transform.position.x;

            LeanTween.moveX(items[0].gameObject, targetX, _smashDuration)
                .setEase(_smashTweenType)
                .setOnComplete(() => FinalizeMerge(items));

            LeanTween.moveX(items[2].gameObject, targetX, _smashDuration)
               .setEase(_smashTweenType);
        }

        private void FinalizeMerge(List<Item> items)
        {
            for (int i = items.Count - 1; i >= 0; i--)
                Destroy(items[i].gameObject);

            // Play some particles
            ParticleSystem mergeParticleInstance = 
                Instantiate(_mergeParticle, items[1].transform.position, Quaternion.identity, transform);

            mergeParticleInstance.Play();

            Merged?.Invoke();
        }
    }
}