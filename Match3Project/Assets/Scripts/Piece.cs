using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PieceState
{
    WAIT,
    MOVE
}

public class Piece : MonoBehaviour
{
    [Header("Piece Offset")]
    public PieceState currState = PieceState.WAIT;

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

    public Piece target;
    public Vector2 moveToPos;

    private Vector2 startPos;
    private Vector2 endPos;

    [Header("Piece Parts")]
    public SpriteRenderer itemSprite;
    private SpriteRenderer pieceSprite;

    public List<ParticleSystem> pieceEffects; // 0 = PieceExplosion, 1 = ColumnExplosion, 2 = CrossBomb, 3 = RowBomb, 4 = HintEffect
    private ParticleSystem[] effectObjects;

    private Board board;

    private void Awake()
    {
        pieceSprite = GetComponent<SpriteRenderer>();

        AddPieceEffect();
    }

    private void AddPieceEffect()
    {
        pieceEffects = new List<ParticleSystem>();

        effectObjects = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < effectObjects.Length; ++i)
        {
            if (effectObjects[i].transform.parent == transform)
                pieceEffects.Add(effectObjects[i]);
        }

        effectObjects = null;
    }

    public void SettingPiece(Board b, string n, int v, int r, int c)
    {
        if (board == null)
            board = b;

        if (target != null)
            target = null;

        if(value != v)
        value = v;
        row = r;
        column = c;

        moveToPos = new Vector2(row, column);
        name = n;

        if (!crossBomb)
            pieceSprite.sprite = board.pieceSprites[value];

        if (board.boardIndex[row, column] != gameObject)
            board.boardIndex[row, column] = gameObject;
    }

    public void SetPieceValue(int v)
    {
        value = v;
        pieceSprite.sprite = board.pieceSprites[value];
    }

    private void Update()
    {
        if (currState == PieceState.MOVE)
        {
            fallSpeed += Time.deltaTime * board.fallSpeed;

            if (Mathf.Abs(row - transform.position.x) > 0.1f || Mathf.Abs(column - transform.position.y) > 0.1f)
                transform.position = Vector2.Lerp(transform.position, moveToPos, fallSpeed);

            else
            {
                if (board.boardIndex[row, column] != gameObject)
                    board.boardIndex[row, column] = gameObject;
                
                gameObject.name = "[" + row + " , " + column + "]";
                transform.position = moveToPos;
                fallSpeed = 0f;

                currState = PieceState.WAIT;
            }
        }
    }

    private void OnMouseDown()
    {
        if (currState == PieceState.WAIT && Time.timeScale > 0 &&
            board.currState == BoardState.ORDER)
        {
            if (board.currState == BoardState.CLEAR || board.currState == BoardState.FAIL)
                return;

            board.selectPiece = this;
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (currState == PieceState.WAIT && Time.timeScale > 0 &&
            board.currState == BoardState.ORDER)
        {
            if (board.currState == BoardState.CLEAR || board.currState == BoardState.FAIL)
                return;

            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculratePiece();
        }
    }

    private void CalculratePiece()
    {
        if (currState == PieceState.WAIT && Time.timeScale > 0 &&
            board.currState == BoardState.ORDER)
        {
            if (board.currState == BoardState.CLEAR || board.currState == BoardState.FAIL)
                return;

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
            currState = PieceState.MOVE;
            target.currState = PieceState.MOVE;

            board.selectPiece = this;
           // board.AllStopEffect();

            StageManager.instance.SoundEffectPlay(0);

            board.currState = BoardState.WORK;
        }
    }

    public void MoveToBack()
    {
        row = prevRow;
        column = prevColumn;

        moveToPos = new Vector2(prevRow, prevColumn);

        isTunning = true;

        currState = PieceState.MOVE;
    }

    /// <summary>
    ///         <param name="index">
    ///         index is particle elemants.
    ///         Effect Play Numbers [ 0 : PieceExplosion || 1 : ColumnBomb || 2 : CrossBomb || 3 : RowBomb || 4 : HintEffect ]
    ///         </param>
    /// </summary>
    public void PieceEffectPlay(int index)
    {
        pieceEffects[index].Stop();
        pieceEffects[index].Play();
    }

    public bool IsEffectPlaying()
    {
        foreach(var effect in pieceEffects)
        {
            if (pieceEffects[0] == effect)
                continue;

            if (effect.isPlaying)
                return true;
        }
        return false;
    }

    public void AllClearPiece()
    {
        board.boardIndex[row, column] = null;

        value = 0;
        row = 0;
        column = 0;

        target = null;

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
}