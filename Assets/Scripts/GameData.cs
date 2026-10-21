using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;


public static class GameData
{
    private static int _skinNumber;

    public static void ChangeSkin()
    {
        _skinNumber += 1;
        if (_skinNumber >= 4)
        {
            _skinNumber = 0;
        }
    }
    public static int GetSkin()
    {
        return _skinNumber;
    }
    public static void ResetSkin()
    {
        _skinNumber = 0;
    }

    private static int _clicks;


    private static readonly HashSet<string> OwnedUpgrades = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnStartup()
    {
        ResetAll();
    }


    public static int Clicks
    {
        get
        {
            return _clicks;
        }
        private set
        {
            _clicks = value;
        }
    }


    public static void AddClick()
    {
        if (OwnedUpgrades.Contains("QuadrupleClick"))
        {
            Clicks = Clicks + 4;
        }
        else if (OwnedUpgrades.Contains("TripleClick"))
        {
            Clicks = Clicks + 3;
        }
        else if (OwnedUpgrades.Contains("DoubleClick"))
        {
            Clicks = Clicks + 2;
        }
        else
        {
           Clicks = Clicks + 1;
        }
    }
    public static int Cooldown = 10;
    public static void autoclicker()
    {
        if (OwnedUpgrades.Contains("Autoclicker"))
        {
            if (Cooldown == 0)
            {
                Clicks = Clicks + 1;
                Cooldown = 10;
            }
            else
            {
                Cooldown -= 1;
            }
        }
    }

    public static bool CanAfford(int cost)
    {
        return Clicks >= cost;
    }

    public static bool Spend(int cost)
    {
        if (!CanAfford(cost))
        {
            UnityEngine.Debug.Log('a');
            return false;
        }

        Clicks = Clicks - cost;
        return true;
    }

    public static bool IsUpgradeOwned(string upgradeName)
    {
        return OwnedUpgrades.Contains(upgradeName);
    }

    public static void MarkUpgradeOwned(string upgradeName)
    {
        OwnedUpgrades.Add(upgradeName);
    }

    public static void ResetAll()
    {
        Clicks = 0;
        OwnedUpgrades.Clear();
    }
}
