using System.Data.SqlTypes;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Upgrade
{
    public string Description;
    public int Cost;
    public bool Owned;
    public Upgrade(string description,int cost)
    {
        Description = description;
        Cost = cost;
        Owned = false;
    }
    public int[] purchase(int coins)
    {
        int[] result = {0,0};
        if (Owned == false)
        {
            if (coins >= Cost) 
            {   
                Owned = true;
                coins -= Cost;
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
    }
}


public class upgrades : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        coins = 100000;
        UnityEngine.Debug.Log(coins);
        AlreadyPurchased.SetActive(false);
        SuccessfullyPurchased.SetActive(false);
        NotEnoughMoney.SetActive(false);
    }
    int coins;
    [SerializeField] public GameObject AlreadyPurchased;
    [SerializeField] public GameObject SuccessfullyPurchased;
    [SerializeField] public GameObject NotEnoughMoney;
    Upgrade Autoclicker = new Upgrade("automatically clicks for you",10000);
    Upgrade DoubleClick = new Upgrade("each click gives 2 coins", 100);
    Upgrade TripleClick = new Upgrade("each click gives 3 coins", 200);
    Upgrade QuadrupleClick = new Upgrade("each click gives 4 coins", 300);
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
        } 
        else if (result[0] == 2)
        {
            SuccessfullyPurchased.SetActive(true);
        } 
        else if (result[0] == 3)
        {
            NotEnoughMoney.SetActive(true);
        }
        coins = result[1];
        float time = Time.deltaTime;
        UnityEngine.Debug.Log('b');
        StartCoroutine(Wait(1000f));
        AlreadyPurchased.SetActive(false);
        SuccessfullyPurchased.SetActive(false);
        NotEnoughMoney.SetActive(false);
        UnityEngine.Debug.Log(coins);
    }
    IEnumerator Wait(float delay)
    {
        yield return new WaitForSeconds(delay);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
