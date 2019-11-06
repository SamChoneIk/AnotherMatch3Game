using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public float fallSpeed = 0f;
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

    private ParticleSystem[] effectObjects;
    private List<ParticleSystem> pieceEffects; // 0 = PieceExplosion, 1 = ColumnExplosion, 2 = CrossBomb, 3 = RowBomb

    public Block target;
    public Vector2 moveToPos;

    private void Awake()
    {
        pieceSprite = GetComponent<SpriteRenderer>();
        effectObjects = GetComponentsInChildren<ParticleSystem>();
        pieceEffects = new List<ParticleSystem>();

        for (int i = 0; i < effectObjects.Length; ++i)
        {
            if (effectObjects[i].transform.parent == this)
            {
                pieceEffects.Add(effectObjects[i]);
            }
        }

        effectObjects = null;
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

        foreach(var effect in pieceEffects)
        {
            effect.Stop();
        }

        pieceSprite.sprite = board.pieceSprites[value];

        currState = BlockState.WAIT;
    }

    /// <summary>
    ///         <param name="index">
    ///         index is particle elemants.
    ///         Effect Play Numbers [ 0 : PieceExplosion || 1 : ColumnBomb || 2 : CrossBomb || 3 : RowBomb || 4 : HintEffect ]
    ///         </param>
    /// </summary>
    public void EffectPlay(int index)
    {
        pieceEffects[index].Play();
    }

    public void AllClearPiece()
    {
        board.boardIndex[row, column] = null;

        value = 0;
        row = 0;
        column = 0;

        target = null;
        rowBomb = false;
        columnBomb = false;
        crossBomb = false;

        pieceSprite.sprite = null;
        itemSprite.sprite = null;
    }

    public void SetDisabledPiece()
    {
        foreach (var effect in pieceEffects)
        {
            effect.Stop();
        }

        name = "DefaultPiece";
        transform.parent = board.disabledPieces.transform;
        transform.position = new Vector2(row, column);

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currState == BlockState.MOVE)
        {
            if (pieceEffects[4].isPlaying)
                pieceEffects[4].Stop();

            fallSpeed += Time.deltaTime * board.blockFallSpeed;

            if (Mathf.Abs(row - transform.position.x) > 0.1f || Mathf.Abs(column - transform.position.y) > 0.1f)
                transform.position = Vector2.Lerp(transform.position, moveToPos, fallSpeed);

            else
            {
                if (board.boardIndex[row, column] != gameObject)
                    board.boardIndex[row, column] = gameObject;

                gameObject.name = "[" + row + " , " + column + "]";
                transform.position = moveToPos;
                fallSpeed = 0f;

                currState = BlockState.WAIT;
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

    public void MoveToBack()
    {
        row = prevRow;
        column = prevColumn;

        moveToPos = new Vector2(prevRow, prevColumn);

        isTunning = true;

        currState = BlockState.MOVE;
    }
}