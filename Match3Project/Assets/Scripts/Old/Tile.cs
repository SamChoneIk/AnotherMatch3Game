using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int hitPoint;
    private SpriteRenderer sprite;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (hitPoint <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        hitPoint -= damage;
        MakeLighter();
    }

    private void MakeLighter()
    {
        // take the current color
        Color color = sprite.color;
        //Get the current color's alpha value and cut it in half.
        float newAlpha = color.a * 0.5f;
        sprite.color = new Color(color.r, color.g, color.b, newAlpha);
    }
}
