
using UnityEngine;
using TMPro;

public class PlayerRunUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerHealth;
    [SerializeField] private PlayerHealth player;

    private void OnEnable()
    {
        if (player != null)
            player.onHealthChanged.AddListener(UpdateHealth);
    }

    private void Start()
    {
        if (player != null)
            UpdateHealth(player.CurrentHealth, player.MaxHealth);
    }

    private void OnDisable()
    {
        if (player != null)
            player.onHealthChanged.RemoveListener(UpdateHealth);
    }

    private void UpdateHealth(float current, float max)
    {
        playerHealth.text = $"{current}/{max}";
    }
}
