using UnityEngine;
using UnityEngine.UI;

public class Autoclicker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Button UpgradeButton;
    public string Upgrade = "Autoclicker";
    [SerializeField] private upgrades Upgrade_manager;
    void Start()
    {

        if (_upgradeButton == null)
        {
            _upgradeButton = GetComponent<Button>();
        }

        if (_upgradeManager == null)
        {
            _upgradeManager = FindFirstObjectByType<upgrades>();
        }

        if (_upgradeButton == null)
        {
            Debug.LogError(
                $"{name}: no Button on this object and none assigned in the Inspector.",
                this);
            return;
        }

        if (_upgradeManager == null)
        {
            Debug.LogError(
                $"{name}: no upgrades component found in this scene. Add one, or " +
                "assign it in the Inspector.",
                this);
            return;
        }

        _upgradeButton.onClick.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        _upgradeManager.upgrade_button_press(_upgradeName);
    }
}
