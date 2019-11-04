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
    public float dragRegist = 2f;

    public bool isTunning = false;

    public bool rowBomb = false;
    public bool columnBomb = false;
    public bool crossBomb = false;

    public SpriteRenderer itemSprite;

    private Vector2 startPos;
    private Vector2 endPos;

    private BoardManager board;
    private SpriteRenderer pieceSprite;

    public Block target;
    public Vector2 moveToPos;

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

        currState = BlockState.WAIT;
    }

    public void AllClearPiece()
    {
        board.boardIndex[row, column] = null;

        InitPiece(0, 0, 0, board);

        itemSprite.sprite = null;

        target = null;
        rowBomb = false;
        columnBomb = false;
        crossBomb = false;

        name = "DefaultPiece";
        transform.parent = board.disabledPieces.transform;
        transform.position = new Vector2(row, column);

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currState == BlockState.MOVE)
        {
            if(isTunning)
            {
                row = prevRow;
                column = prevColumn;

                moveToPos = new Vector2(prevRow, prevColumn);
            }

            accumTime += Time.deltaTime / board.blockDuration;

            if (Mathf.Abs(row - transform.position.x) > 0.1f || Mathf.Abs(column - transform.position.y) > 0.1f)
                transform.position = Vector2.MoveTowards(transform.position, moveToPos, accumTime);

            else
            {
                if (board.boardIndex[row, column] != gameObject)
                    board.boardIndex[row, column] = gameObject;

                gameObject.name = "[" + row + " , " + column + "]";
                transform.position = moveToPos;
                accumTime = 0f;

                currState = BlockState.WAIT;

                if (isTunning)
                {
                    isTunning = false;
                    board.currState = BoardState.ORDER;
                }
            }
        }
    }

    private void OnMouseDown()
    {
        if (board.currState == BoardState.ORDER && currState == BlockState.WAIT)
        {
            board.selectPiece = this;
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currState == BoardState.ORDER && currState == BlockState.WAIT)
        {
            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculratePiece();
        }
    }

    private void CalculratePiece()
    {
        if (board.currState == BoardState.ORDER && currState == BlockState.WAIT)
        {
            Vector2 dir = (endPos - startPos);

            if (Mathf.Abs(dir.x) > dragRegist || Mathf.Abs(dir.y) > dragRegist)
            {
                if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
                    MoveToPiece(dir.y > 0 ? Vector2.up : Vector2.down);

                else if (Mathf.Abs(dir.y) < Mathf.Abs(dir.x))
                    MoveToPiece(dir.x > 0 ? Vector2.right : Vector2.left);
            }
        }
    }

    private void MoveToPiece(Vector2 direction)
    {
        if ((row + direction.x) < board.width && (column + direction.y) < board.height &&
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

            moveToPos = new Vector2(row, column);
            target.moveToPos = new Vector2(target.row, target.column);

            // 블럭을 움직임
            currState = BlockState.MOVE;
            target.currState = BlockState.MOVE;

            board.selectPiece = this;

            board.currState = BoardState.WORK;
        }
    }
}