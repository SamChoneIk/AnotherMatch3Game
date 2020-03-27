using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public bool onTheMenu = false;
    private float timeScales => onTheMenu ? 0.0f : 1.0f; 

    [HideInInspector]
    public UImenu[] menus;

    public Menus previousMenuID;
    public Menus currentMenuID;

    private void Awake()
    {
        UImenu.manager = this;
    }

    public void MenuOpen(int id)
    {
        OnTheWindow((Menus)id, false);
    }

    public void MenuClose(int id = -1)
    {
        OnTheWindow((Menus)id, true);
    }

    public UImenu OnTheWindow(Menus id, bool close = false)
    {
        if (close)
        {
            if (id == Menus.None)
            {
                if (currentMenuID != id)
                {
                    ToggleVisible(currentMenuID, true);
                    currentMenuID = id;
                }

                if (previousMenuID != id)
                    previousMenuID = id;

                return null;
            }

            if (previousMenuID != Menus.None)
                currentMenuID = previousMenuID;

            previousMenuID = id;
            ToggleVisible(previousMenuID, true);

            if (currentMenuID != Menus.None)
                ToggleVisible(currentMenuID);
        }

        else
        {
            if (id == Menus.None)
                return null;

            if (currentMenuID != Menus.None)
                previousMenuID = currentMenuID;

            currentMenuID = id;
            ToggleVisible(currentMenuID);

            if (previousMenuID != Menus.None)
                ToggleVisible(previousMenuID, true);
        }

        return GetWindow(id);
    }

    private void ToggleVisible(Menus id, bool Invisible = false)
    {
        if (Invisible)
            GetWindow(id).Close();

        else
            GetWindow(id).Open();
    }

    public UImenu GetWindow(Menus id)
    {
        return menus[(int)id];
    }

    private int GetWindowLength()
    {
        return (int)Enum.GetValues(typeof(Menus)).Cast<Menus>().Max();
    }
}