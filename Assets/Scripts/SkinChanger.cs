using UnityEngine;
using System;
using System.Collections.Generic;

public class SkinChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameData.ResetSkin();
    }

    void Update()
    {

    }

    public void Trigger()
    {
        GameData.ChangeSkin();

    }
}