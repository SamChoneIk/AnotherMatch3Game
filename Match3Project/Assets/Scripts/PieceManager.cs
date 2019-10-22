using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    public int value;
    public int row;
    public int column;

    public bool isMatched = false;

    private int prevRow;
    private int prevColumn;

    private float accumTime = 0;

    private Vector2 startPos;
    private Vector2 endPos;

    private BoardManager board;
    private GameObject selectPiece;
    private PieceManager swapPiece;

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
            MoveToPiece(dir.y > 0 ? Vector2.up : Vector2.down);
        }

        else if (Mathf.Abs(dir.y) < Mathf.Abs(dir.x))
        {
            MoveToPiece(dir.x > 0 ? Vector2.right : Vector2.left);
        }
    }

    private void MoveToPiece(Vector2 direction)
    {
        if (board.boardIndex[row + (int)direction.x, column + (int)direction.y] != null)
        {
            swapPiece = board.boardIndex[row + (int)direction.x, column + (int)direction.y].GetComponent<PieceManager>();

            prevRow = row;
            prevColumn = column;

            swapPiece.row += -1 * (int)direction.x;
            swapPiece.column += -1 * (int)direction.y;

            row += (int)direction.x;
            column += (int)direction.y;

            SetPostionPiece(swapPiece.row, swapPiece.column, swapPiece);
            SetPostionPiece(row, column, this);

            Debug.Log("Move Complete");

            board.currState = BoardState.WORK;
        }

        else
        {
            board.currState = BoardState.ORDER;
            Debug.Log("asd");
        }
    }
    private void SetPostionPiece(int row, int column, PieceManager piece)
    {
        piece.transform.position = new Vector2(row, column);
        board.boardIndex[row, column] = piece.gameObject;
        piece.name = "[" + row + " , " + column + "]";
    }
    
    IEnumerator MovePiece(PieceManager swapPiece)
    {
        while (accumTime < board.duration)
        {
            accumTime += Time.deltaTime / board.duration;
            transform.position = Vector2.Lerp(transform.position, swapPiece.transform.position, accumTime);

            Debug.Log(accumTime);
            yield return null;
        }
    }
}