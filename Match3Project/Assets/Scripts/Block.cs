using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockState
{
    WAIT,
    MOVE
}

public class Block : MonoBehaviour
{
    public BlockState currState = BlockState.WAIT;
    public int value;

    public int row;
    public int column;
    public int prevRow;
    public int prevColumn;

    public float accumTime = 0f;
    public float dragRegist = 0.5f;

    public bool isTunning = false;
    public bool isMoving = false;

    private Vector2 startPos;
    private Vector2 endPos;

    private BoardManager board;
    private SpriteRenderer pieceSprite;
    public Block target;
    private Vector2 moveToPos;

    private void Awake()
    {
        pieceSprite = GetComponent<SpriteRenderer>();
    }

    public void InitPiece(int v, int r, int c, BoardManager b)
    {
        if (board == null)
            board = b;

        if (target != null)
            target = null;

        value = v;
        row = r;
        column = c;

        pieceSprite.sprite = board.pieceSprites[value];
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
        if (board.currState == BoardState.ORDER && currState == BlockState.WAIT)
        {
            Vector2 dir = (endPos - startPos).normalized;

            if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
                MoveToPiece(dir.y > dragRegist ? Vector2.up : Vector2.down);

            else if (Mathf.Abs(dir.y) < Mathf.Abs(dir.x))
                MoveToPiece(dir.x > dragRegist ? Vector2.right : Vector2.left);
        }
    }

    private void MoveToPiece(Vector2 direction)
    {
        if ((row + direction.x) < board.width &&
            (column + direction.y) < board.height &&
            board.boardIndex[row + (int)direction.x, column + (int)direction.y] != null)
        {
            // 블럭이 참조할 대상
            target = board.GetPiece(row + (int)direction.x, column + (int)direction.y);
            target.target = this;

            // 블럭 이전 위치 값 초기화
            prevRow = row;
            prevColumn = column;
            target.prevRow = target.row;
            target.prevColumn = target.column;

            // 블럭 현재 위치 값 초기화
            row += (int)direction.x;
            column += (int)direction.y;
            target.row += -1 * (int)direction.x;
            target.column += -1 * (int)direction.y;

            // 블럭을 움직임
            currState = BlockState.MOVE;
            target.currState = BlockState.MOVE;

            board.selectPiece = this;
            board.targetPiece = target;
        }
    }
   
    public void MovedPiece()
    {
        moveToPos = new Vector2(row, column);
        accumTime += Time.deltaTime / board.blockDuration;

        transform.position = Vector2.Lerp(transform.position, moveToPos, accumTime);
        if (Vector2.Distance(transform.position, moveToPos) < 0.2f)
        {
            accumTime = 0f;
            currState = BlockState.WAIT;
            //Debug.Log(gameObject.name + " moved Complete");
        }
    }

    /*IEnumerator MovedPieceCo()
    {
        moveToPos = new Vector2(row, column);

        while (Vector2.Distance(transform.position, moveToPos) > 0.2f)
        {
            accumTime += Time.deltaTime / board.blockDuration;
            transform.position = Vector2.Lerp(transform.position, moveToPos, accumTime);

            yield return null;
        }
        accumTime = 0f;

        currState = BlockState.WAIT;
        Debug.Log(gameObject.name + " moved Complete");
    }*/

}