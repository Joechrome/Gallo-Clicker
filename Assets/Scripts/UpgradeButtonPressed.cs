using UnityEngine.UI;
using UnityEngine;

public class Autoclicker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Button UpgradeButton;
    public string Upgrade = "Autoclicker";
    [SerializeField] private upgrades Upgrade_manager;
    void Start()
    {
        UpgradeButton.onClick.AddListener(TaskOnClick);
    }
    // Update is called once per frame
    void TaskOnClick()
    {
        Upgrade_manager.upgrade_button_press(Upgrade);
    }
}
