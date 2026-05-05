using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;

    [Header("Coins")]
    [SerializeField] private TMP_Text coinText;

    [Header("Double Jump Upgrade")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image timerFill;   // optional fill bar

    private int coins = 0;

    private void Start()
    {
        coinText.text = "0";
    }

    private void Update()
    {
        UpdateUpgradeTimer();
    }

    public void AddCoin()
    {
        coinText.text = (++coins).ToString();
    }

    private void UpdateUpgradeTimer()
    {
        if (upgradePanel == null) return;

        bool active = player.IsDoubleJumpActive;
        upgradePanel.SetActive(active);

        if (!active) return;

        float left = Mathf.Max(player.DoubleJumpTimeLeft, 0f);
        float duration = player.DoubleJumpDuration;

        if (timerText != null)
            timerText.text = left.ToString("F1");

        if (timerFill != null)
            timerFill.fillAmount = duration > 0f ? left / duration : 0f;
    }
}
