using System.Data.SqlTypes;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Upgrade
{
    public string Name;
    public int Cost;
    public bool Owned;

    public Upgrade(string name, int cost)
    {
        Name = name;
        Cost = cost;

        // Restore the purchase from the save file, so an upgrade bought in an
        // earlier session still counts as owned.
        Owned = GameData.IsUpgradeOwned(name);
    }
    public int[] purchase(int coins)
    {
        int[] result = {0,0};
        if (Owned == false)
        {
            if (coins >= Cost) 
            {   
                GameData.MarkUpgradeOwned(Name);
                Owned = true;
                coins -= Cost;
                GameData.Spend(Cost);
                result[0] = 2;
                result[1] = coins;
                return result;
            } 
            else 
            {
                result[0] = 3;
                result[1] = coins;
                return result;
            }
        }
        else
        {
            result[0] = 1;
            result[1] = coins;
            return result;
        }

        // Spend() checks the balance itself and changes nothing if the player is
        // short, so the score and the Owned flag can never disagree.
//        if (!GameData.Spend(Cost))
//        {
//            result[0] = 1;
//            result[1] = coins;
//            return result;
//        }
//
//        Owned = true;
//        GameData.MarkUpgradeOwned(Name);
//        return true;
    }
}


public class upgrades : MonoBehaviour
{
    public Score_Noting current_score;
    void Start()
    {
        if (Scoreboard != null)
        {
            current_score = Scoreboard.GetComponent<Score_Noting>();
        }
        coins = current_score != null ? current_score.getscore() : GameData.Clicks;
        AlreadyPurchased.SetActive(false);
        SuccessfullyPurchased.SetActive(false);
        NotEnoughMoney.SetActive(false);
    }
    public GameObject Scoreboard;
    int coins;
    [SerializeField] public GameObject AlreadyPurchased;
    [SerializeField] public GameObject SuccessfullyPurchased;
    [SerializeField] public GameObject NotEnoughMoney;
    Upgrade Autoclicker = new Upgrade("Autoclicker",1000);
    Upgrade DoubleClick = new Upgrade("DoubleClick", 50);
    Upgrade TripleClick = new Upgrade("TripleClick", 100);
    Upgrade QuadrupleClick = new Upgrade("QuadrupleClick", 150);
    public void upgrade_button_press(string upgrade)
    {
        int[] result = {0,0};
        if (upgrade == "Autoclicker")
        {
            result = Autoclicker.purchase(coins);
        }
        else if (upgrade == "DoubleClick")
        {
            result = DoubleClick.purchase(coins);
        }
        else if (upgrade == "TripleClick")
        {
            result = TripleClick.purchase(coins);
        }
        else if (upgrade == "QuadrupleClick")
        {
            result = QuadrupleClick.purchase(coins);
        }
        if (result[0] == 1)
        {
            AlreadyPurchased.SetActive(true);
            SuccessfullyPurchased.SetActive(false);
            NotEnoughMoney.SetActive(false);
        } 
        else if (result[0] == 2)
        {
            AlreadyPurchased.SetActive(false);
            SuccessfullyPurchased.SetActive(true);
            NotEnoughMoney.SetActive(false);
        } 
        else if (result[0] == 3)
        {
            AlreadyPurchased.SetActive(false);
            SuccessfullyPurchased.SetActive(true);
            NotEnoughMoney.SetActive(false);
        }
        coins = result[1];
        StartCoroutine(Wait(2.5f));
    }
    IEnumerator Wait(float delay)
    {
        yield return new WaitForSeconds(delay);
        AlreadyPurchased.SetActive(false);
        SuccessfullyPurchased.SetActive(false);
        NotEnoughMoney.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
//        if (upgrade == _autoclicker.Name) return _autoclicker;
//        if (upgrade == _doubleClick.Name) return _doubleClick;
//        if (upgrade == _tripleClick.Name) return _tripleClick;
//        if (upgrade == _quadrupleClick.Name) return _quadrupleClick;
//        return null;
    }
}
