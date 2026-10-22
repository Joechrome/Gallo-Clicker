using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;


public static class GameData
{

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


    /// <summary>
    /// Set while a ransom sequence is running: no click and no key press may earn
    /// score during the warning window or the coin collection. Owned by RansomEvent.
    /// </summary>
    public static bool ScoreBlocked { get; set; }

    public static void AddClick()
    {
        if (ScoreBlocked)
        {
            return;
        }

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
        if (ScoreBlocked)
        {
            return;
        }

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
        Cooldown = 10;
        ScoreBlocked = false;
    }


    // For Skin Changer
    private static int _skinNumber;
    private static string[] skinList = new string[] { "Default", "Sombrero", "SailorHat", "Goku" }; 

    public static void ChangeSkin()
    {
        _skinNumber += 1;
        Debug.Log($"skin: {_skinNumber}");
        Debug.Log(IsUpgradeOwned(skinList[1]));

        while (true)
        {
            if (_skinNumber >= 4)
            {
                _skinNumber = 0;
                break;
            }
            if (IsUpgradeOwned(skinList[_skinNumber]))
            {
                break;
            }
            _skinNumber += 1;
            
        } 
        Debug.Log($"skin: {_skinNumber}");
        
    } 
    public static int GetSkin()
    {
        return _skinNumber;
    }
    public static void ResetSkin()
    {
        _skinNumber = 0;
    }
}

