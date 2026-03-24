using TMPro;
using UnityEngine;

public enum EPowerupType
{
    Vacuum = 0,
    Spring = 1,
    Fan = 2,
    FreezeGun = 3
}

public abstract class Powerup : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private TextMeshPro amountText;
    [SerializeField] private GameObject videoIcon;

    [Header(" Settings ")]
    [SerializeField] protected EPowerupType powerupType;
    public EPowerupType PowerupType => powerupType;

    public void UpdateVisuals(int amount)
    {
        videoIcon.SetActive(amount <= 0);

        if (amount <= 0)
            amountText.text = "";
        else
            amountText.text = amount.ToString();
    }
}
