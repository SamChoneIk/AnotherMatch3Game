using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int column;
    public int row;
    public int targetX;
    public int targetY;
    public bool isMatched = false;
    private GameHandler board;
    private GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;
    public float swipeAngle = 0;

    void Start()
    {
        board = FindObjectOfType<GameHandler>();
        targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;
        row = targetY;
        column = targetX;
    }

    void Update()
    {
        FindMatches();

        if(isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1f, 1f, 1f, .2f);
        }

        targetX = column;
        targetY = row;

        if(Mathf.Abs(targetX - transform.position.x) > 0.1)
        {
            //move Towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.4f);
        }

        else
        {
            // directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
        }

        if (Mathf.Abs(targetY - transform.position.y) > 0.1)
        {
            //move Towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.4f);
        }

        else
        {
            // directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
            board.allDots[column, row] = gameObject;
        }

    }

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        Debug.Log(swipeAngle);
        MovePieces();
    }

    private void MovePieces()
    {   // Right Swipe
        if(swipeAngle > -45 && swipeAngle <= 45 &&
            column < board.width - 1)
        {
            otherDot = board.allDots[column + 1, row];
            otherDot.GetComponent<Block>().column -= 1;
            column += 1;
        }
        // Up Swipe
        else if (swipeAngle > 45 && swipeAngle <= 135 &&
            row < board.height - 1)
        {
            otherDot = board.allDots[column, row + 1];
            otherDot.GetComponent<Block>().row -= 1;
            row += 1;
        }
        // Left Swipe
        else if ((swipeAngle > 135 || swipeAngle <= -135) &&
            column > 0)
        {
            otherDot = board.allDots[column - 1, row];
            otherDot.GetComponent<Block>().column += 1;
            column -= 1;
        }
        // Down Swipe
        else if ((swipeAngle < -45 && swipeAngle >= -135) &&
            row > 0)
        {
            otherDot = board.allDots[column, row - 1];
            otherDot.GetComponent<Block>().row += 1;
            row -= 1;
        }

    }

    private void FindMatches()
    {
        if(column > 0 && column < board.width -1)
        {
            GameObject leftBlock1 = board.allDots[column - 1, row];
            GameObject rightBlock1 = board.allDots[column + 1, row];
            if(leftBlock1.tag == this.gameObject.tag && rightBlock1.tag == gameObject.tag)
            {
                leftBlock1.GetComponent<Block>().isMatched = true;
                rightBlock1.GetComponent<Block>().isMatched = true;
                isMatched = true;
            }
        }

        if (row > 0 && row < board.width - 1)
        {
            GameObject upBlock1 = board.allDots[column, row + 1];
            GameObject downBlock1 = board.allDots[column, row - 1];
            if (upBlock1.tag == this.gameObject.tag && downBlock1.tag == gameObject.tag)
            {
                upBlock1.GetComponent<Block>().isMatched = true;
                downBlock1.GetComponent<Block>().isMatched = true;
                isMatched = true;
            }
        }
    }
}
