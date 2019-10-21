using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    public int value;
    public int row;
    public int column;

    private int oriRow;
    private int oriColumn;

    private float accumTime = 0;

    private Vector2 startPos;
    private Vector2 endPos;

    private BoardManager board;
    private GameObject selectPiece;
    private GameObject swapPiece;

    public void InitPiece(int v, int r, int c)
    {
        board = FindObjectOfType<BoardManager>();

        value = v;
        row = r;
        column = c;
    }

    private void OnMouseDown()
    {
        startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        selectPiece = gameObject;
    }

    private void OnMouseUp()
    {
        endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculratePiece();
    }

    private void CalculratePiece()
    {
        Vector2 dir = (endPos - startPos).normalized;
        Debug.Log(dir);

        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
        {
            if (dir.y > 0)
            {
                MovedPiece(Vector2.up);
                Debug.Log("up");
            }

            if (dir.y < 0)
            {
                MovedPiece(Vector2.down);
                Debug.Log("down");
            }
        }

        else if (Mathf.Abs(dir.y) < Mathf.Abs(dir.x))
        {
            if (dir.x > 0)
            {
                MovedPiece(Vector2.right);
                Debug.Log("right");
            }

            if (dir.x < 0)
            {
                MovedPiece(Vector2.left);
                Debug.Log("left");
            }
        }
    }
    
    private void MovedPiece(Vector2 direction)
    {
        swapPiece = board.board[row + (int)direction.x, column + (int)direction.y];

        oriRow = row;
        oriColumn = column;

        swapPiece.GetComponent<PieceManager>().row += -1 * (int)direction.x;
        swapPiece.GetComponent<PieceManager>().column += -1 * (int)direction.y;

        row += (int)direction.x;
        column += (int)direction.y;

        StartCoroutine(MovePiece());
        //board.currState = BoardState.ORDER;
    }
    
    IEnumerator MovePiece()
    {
        while (accumTime < board.duration)
        {
            accumTime += Time.deltaTime / board.duration;
            gameObject.transform.position = Vector2.Lerp(gameObject.transform.position, swapPiece.transform.position, accumTime);

            Debug.Log(accumTime);
            yield return null;
        }
        Debug.Log("asds");
    }
}