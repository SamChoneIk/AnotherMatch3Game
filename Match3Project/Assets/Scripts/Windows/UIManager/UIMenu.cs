using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UImenu : MonoBehaviour
{
	public GameObject firstSelected;

    public Text windowsTitleText;
    public string menuName;

    protected EventSystem eventSystem;
    protected EventSystem EventSystem
    {
        get
        {
            if (eventSystem == null)
                eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

            return eventSystem;
        }
    }

    public static UIManager manager;

    protected virtual void Awake()
    {
        Close();
    }

    public virtual void OnFocus()
    {
        EventSystem.SetSelectedGameObject(firstSelected);
    }

    protected virtual void Display(bool display)
    {
        if (display)
        {
            if (!string.IsNullOrEmpty(menuName) && windowsTitleText != null)
                windowsTitleText.text = menuName;
        }

        gameObject.SetActive(display);
    }

	public virtual void Open()
    {
        Display(true);
        OnFocus();
    }

    public virtual void Close()
    {
        Display(false);
    }
}