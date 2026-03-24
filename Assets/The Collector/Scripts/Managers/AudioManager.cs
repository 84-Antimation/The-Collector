using System;
using UnityEngine;

namespace FoxX.TheCollector
{
    public class AudioManager : MonoBehaviour
    {
        [Header(" SFX ")]
        [SerializeField] private AudioSource _itemSelected;
        [SerializeField] private AudioSource _itemPlaced;
        [SerializeField] private AudioSource _itemsMerged;
        [SerializeField] private AudioSource _cardComplete;

        private void Awake()
        {
            ItemSpotsManager.ItemPickedUp   += PlayItemSelected;
            ItemSpotsManager.ItemPlaced     += PlayItemPlaced;

            MergeManager.Merged             += PlayItemsMerged;

            GoalCard.complete               += PlayCardComplete;
        }

        private void OnDestroy()
        {
            ItemSpotsManager.ItemPickedUp   -= PlayItemSelected;
            ItemSpotsManager.ItemPlaced     -= PlayItemPlaced;

            MergeManager.Merged             -= PlayItemsMerged;

            GoalCard.complete               -= PlayCardComplete;
        }

        private void PlayItemSelected(Item unused)
        {
            PlaySource(_itemSelected, UnityEngine.Random.Range(.9f, 1f));
        }

        private void PlayItemPlaced()
        {
            //PlaySource(itemPlaced);
        }

        private void PlayItemsMerged()
        {
            PlaySource(_itemsMerged);
        }

        private void PlayCardComplete()
        {
            PlaySource(_cardComplete);
        }

        private void PlaySource(AudioSource source, float pitch)
        {
            source.pitch = pitch;
            PlaySource(source);
        }

        private void PlaySource(AudioSource source)
        {
            source.Play();
        }
    }
}