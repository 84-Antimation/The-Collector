using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;
using System;

namespace FoxX.TheCollector
{
    public class GoalCard : MonoBehaviour
    {
        [Header(" Elements ")]
        [SerializeField] private GameObject _back;
        [SerializeField] private Animator _animator;
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private GameObject _check;

        [Header(" Settings ")]
        private bool _isComplete;
        public bool IsComplete => _isComplete;

        [Header(" Actions ")]
        public static Action complete;

        private void Start()
        {
            _animator.enabled = false;
        }

        private void Update()
        {
            if (_isComplete)
                HandleBackFace();
        }

        public void Configure(Sprite itemIcon, int initialAmount)
        {
            _itemImage.sprite = itemIcon;
            _amountText.text = initialAmount.ToString();
        }

        public void UpdateAmount(int amount)
        {
            _amountText.text = amount.ToString();

            Bump();
        }
        
        private void Bump()
        {
            LeanTween.cancel(gameObject);

            transform.localScale = Vector3.one;
            LeanTween.scale(gameObject, Vector3.one * 1.1f, .1f)
                .setLoopPingPong(1);
        }
        
        [Button]
        public void Complete()
        {
            _animator.enabled = true;

            _amountText.text = "";
            _check.SetActive(true);

            _isComplete = true;

            PlayFinalAnimation();
        }

        private void PlayFinalAnimation()
        {
            _animator.Play("Complete");
        }

        private void TriggerCompleteSFX()
            => complete?.Invoke();

        private void HandleBackFace()
            => _back.SetActive(Vector3.Dot(transform.forward, Vector3.forward) < 0);
    }
}