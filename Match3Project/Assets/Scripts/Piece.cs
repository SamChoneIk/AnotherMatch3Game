using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Piece : MonoBehaviour
{
    [Header("Piece Offset")]
    public PieceState currState = PieceState.WAIT;

    public int value;

    public int row;
    public int column;
    private int prevRow;
    private int prevColumn;

    public bool isTunning = false;

    public bool rowBomb = false;
    public bool columnBomb = false;
    public bool crossBomb = false;

    public Piece target;

    private Vector2 moveToPos;
    private Vector2 startPos;
    private Vector2 endPos;

    private float fallSpeed;
    private float dragRegist = 1f;

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

        if (board.pieceArray[row, column] != gameObject || board.pieceArray[row, column] == null)
            board.pieceArray[row, column] = gameObject;
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
            fallSpeed += Time.deltaTime * board.pieceFallSpeed;

            if (Mathf.Abs(row - transform.position.x) > 0.1f || Mathf.Abs(column - transform.position.y) > 0.1f)
                transform.position = Vector2.Lerp(transform.position, moveToPos, fallSpeed);

            else
            {
                if (board.pieceArray[row, column] != gameObject)
                    board.pieceArray[row, column] = gameObject;
                
                gameObject.name = $"[{row} , {column}]";
                transform.position = moveToPos;
                fallSpeed = 0f;

                currState = PieceState.WAIT;
            }
        }
    }

    private void OnMouseDown()
    {
            //board.selectPiece = this;
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculratePiece();
    }

    private void CalculratePiece()
    {
        if (currState == PieceState.WAIT && board.currBoardState == BoardState.ORDER && Time.timeScale > 0)
        {
            if (StageController.instance.currStageState == StageState.CLEAR ||
                StageController.instance.currStageState == StageState.FAIL)
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
        if ((row + direction.x) < board.horizontal && (column + direction.y) < board.vertical &&
            board.pieceArray[row + (int)direction.x, column + (int)direction.y] != null)
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

            board.selectedPiece = this;
            board.currBoardState = BoardState.WORK;

            StageController.instance.SoundEffectPlay(SoundEffectList.SWAP);

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
    ///         <param name="effectName">
    ///         Piece의 파티클 이펙트를 재생합니다.
    ///         Piece Particle Effect Play Numbers [ 0 : PieceExplosion || 1 : ColumnBomb || 2 : CrossBomb || 3 : RowBomb || 4 : HintEffect ]
    ///         </param>
    /// </summary>
    public void PieceEffectPlay(PieceEffect effectName)
    {
        pieceEffects[(int)effectName].Stop();
        pieceEffects[(int)effectName].Play();
    }

    /// <summary>
    ///         <param name="effectName">
    ///         Piece의 파티클 이펙트를 정지합니다.
    ///         Piece Particle Effect Stop Numbers [ 0 : PieceExplosion || 1 : ColumnBomb || 2 : CrossBomb || 3 : RowBomb || 4 : HintEffect ]
    ///         </param>
    /// </summary>
    public void PieceEffectStop(PieceEffect effectName)
    {
        pieceEffects[(int)effectName].Stop();
    }

    public void SetDisabledPiece()
    {
        board.pieceArray[row, column] = null;
        pieceSprite.sprite = null;
        itemSprite.sprite = null;
        target = null;

        name = StaticVariables.DisabledPieceName;
    }

    public void TunningNull()
    {
        target.isTunning = false;
        target = null;
        isTunning = false;
        board.selectedPiece = null;

        StageController.instance.SoundEffectPlay(SoundEffectList.TUNNING);

        board.currBoardState = BoardState.ORDER;
    }
}