using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PieceState
{
    WAIT,
    MOVE
}

public enum PieceEffect
{
    PIECEEXPLOSION = 0,
    COLUMNBOMB = 1,
    CROSSBOMB = 2,
    ROWBOMB = 3,
    HINTEFFECT = 4,
}

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

    public void SetPiece(Board b, int v, int r, int c)
    {
        if (board == null)
            board = b;

        name = $"Piece [{row} , {column}]";
        value = v;
        row = r;
        column = c;

        moveToPos = new Vector2(row, column);

        if (!crossBomb)
            pieceSprite.sprite = board.pieceSprites[value];
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
                board.SetTileInPiece(row, column, this);
                gameObject.name = $"Piece [{row} , {column}]";

                transform.position = moveToPos;
                fallSpeed = 0f;

                currState = PieceState.WAIT;
            }
        }
    }

    private void OnMouseDown()
    {
        if(board.currBoardState == BoardState.Work)
            return;

        board.selectedPiece = this; // 클릭하면 현재 선택된 Piece로 할당한다.
        startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (board.selectedPiece == null)
            return;

        endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculratePiece();
    }

    private void CalculratePiece()
    {
        if (currState == PieceState.WAIT && board.currBoardState == BoardState.Order && Time.timeScale > 0)
        {
            Vector2 dir = endPos - startPos;

            if (Mathf.Abs(dir.x) > dragRegist || // 일정 거리 이상 움직였을때
                Mathf.Abs(dir.y) > dragRegist)
            {
                if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x)) // 상하로 움직였을 때
                    MoveToPiece(dir.y > 0 ? Vector2.up : Vector2.down); // 움직인 방향에 따라 위 아래로 움직인다.

                else if (Mathf.Abs(dir.y) < Mathf.Abs(dir.x)) // 좌우로 움직였을 때
                    MoveToPiece(dir.x > 0 ? Vector2.right : Vector2.left); // 움직인 방향에 따라 오른쪽 왼쪽으로 움직인다.
            }
        }
    }

    private void MoveToPiece(Vector2 direction)
    {
        if ((row + direction.x) < board.Horizontal && (row + direction.x) >= 0 &&     // 너비 검사
            (column + direction.y) < board.Vertical && (column + direction.y) >= 0 && // 높이 검사
            !board.IsBlankSpace(row + (int)direction.x, column + (int)direction.y) && // Tile이 BlankSpeace가 아닐 시
            !board.IsBreakableTile(row + (int)direction.x, column + (int)direction.y)) // Tile이 BreakableTile이 아닐 시
        {
            target = board.GetPiece(row + (int)direction.x, column + (int)direction.y); // 자리를 바꿀 Piece
            target.target = this;                                                       // 타겟의 타겟이 자신이 된다.

            // 블럭 이전 위치 값 초기화
            prevRow = row; // 현재 row
            prevColumn = column; // 현재 column
            target.prevRow = target.row; // 타겟의 현재 row
            target.prevColumn = target.column; // 타겟의 현재 column

            // 블럭 현재 위치 값 초기화
            row += (int)direction.x; // 해당 방향으로 움직인다.
            column += (int)direction.y;
            target.row += -1 * (int)direction.x; // 타겟은 반대 방향으로 움직인다.
            target.column += -1 * (int)direction.y;

            moveToPos = new Vector2(row, column); // 움직일 위치를 지정
            target.moveToPos = new Vector2(target.row, target.column); // 타겟의 움직일 위치를 지정

            currState = PieceState.MOVE; // Piece를 이동
            target.currState = PieceState.MOVE;

            board.movedPieces.Add(this);
            board.movedPieces.Add(target);

            board.selectedPiece = this;
            board.currBoardState = BoardState.Work;

            GameManager.Instance.SoundEffectPlay(SEClip.Swap);
        }
    }

    public void MoveToBack()
    {
        row = prevRow;
        column = prevColumn;

        moveToPos = new Vector2(prevRow, prevColumn);

        isTunning = true;

        currState = PieceState.MOVE;
        board.movedPieces.Add(this);
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

        //GameManager.Instance.SoundEffectPlay(SEClip.Tunning);

        board.currBoardState = BoardState.Order;
    }
}