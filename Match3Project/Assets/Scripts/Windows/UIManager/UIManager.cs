using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public bool onTheMenu = false;
    private float timeScales => onTheMenu ? 0.0f : 1.0f;

    public Sprite fillStarSprite;
    public Sprite emptyStarSprite;
    public Sprite[] clearStageNodeSprites;
    public Sprite firstStageNodeSprite;

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
        OnTheWindow((Menus)id);
    }

    public void MenuClose(int id = -1)
    {
        OnTheWindow((Menus)id, true);
    }

    public UImenu OnTheWindow(Menus id, bool close = false)
    {
        onTheMenu = !close;
        Time.timeScale = timeScales;

        if (close)
        {
            if (id == Menus.None)
            {
                if (currentMenuID != id)
                {
                    ToggleVisible(currentMenuID, close);
                    currentMenuID = id;
                }

                if (previousMenuID != id)
                    previousMenuID = id;

                return null;
            }

            //if (previousMenuID != Menus.None)
                currentMenuID = previousMenuID;

            previousMenuID = id;

            if (previousMenuID != Menus.None)
                ToggleVisible(previousMenuID, close);

            if (currentMenuID != Menus.None)
                ToggleVisible(currentMenuID);
        }

        else
        {
            if (id == Menus.None)
                return null;

           // if (currentMenuID != Menus.None)
                previousMenuID = currentMenuID;

            currentMenuID = id;

            if (currentMenuID != Menus.None)
                ToggleVisible(currentMenuID);

            if (previousMenuID != Menus.None)
                ToggleVisible(previousMenuID, close);
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