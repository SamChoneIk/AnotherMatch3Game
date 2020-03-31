using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum BoardState
{
    Init,
    Order,
    Work,
}

public class Board : MonoBehaviour
{
    [Header("Board State")]
    public BoardState currBoardState;

    [Header("Board Components")]
    public GameObject piecePrefab;
    public GameObject backgroundTilePrefab;
    public GameObject breakableTilePrefab;

    public Transform piecesParent;
    public Transform disabledPiecesParent;

    public Sprite[] pieceSprites;
    public Sprite[] itemSprites;

    public Piece selectedPiece;

    [HideInInspector]
    public List<Piece> movedPieces;
    [HideInInspector]
    public StageData stageData;
    [HideInInspector]
    public Tile[,] tiles;

    private Queue<Piece> disabledPieces;
    private List<Piece> matchedPieces;
    private List<Piece> verifiedPieces;
    private List<Piece> hintPieces;
    private CamPivot camPivot;
    private UIManager uIMgr;
    public GameStageManager gameStageMgr;

    [Header("Board Variables")]
    private int horizontal;
    public int Horizontal => horizontal;
    private int vertical;
    public int Vertical => vertical;
    public int fallOffset = 5;

    public float delayTime = 0.08f;
    public float effectDelayTime = 0.5f;
    public float pieceFallSpeed;

    private int randomValue => Random.Range(0, pieceSprites.Length);
    private bool isMatched = false;

    private float hintAccumTime;

    public void InitializeBoard(GameStageManager gsm)
    {
        uIMgr = UImenu.manager;
        gameStageMgr = gsm;

        disabledPieces = new Queue<Piece>();
        matchedPieces = new List<Piece>();
        verifiedPieces = new List<Piece>();
        hintPieces = new List<Piece>();

        stageData = GameManager.Instance.GetStageDataWithLevel(StaticVariables.LoadLevel);

        pieceSprites = stageData.pieceSprites;
        itemSprites = stageData.itemSprites;

        vertical = stageData.vertical;
        horizontal = stageData.horizontal;

        tiles = new Tile[horizontal, vertical];

        camPivot = Camera.main.GetComponent<CamPivot>();
        camPivot.SetCameraPivot(horizontal, vertical, stageData.camPivotX, stageData.camPivotY, stageData.camPivotSize);

        CreateBackgroundTile();
        CreateBreakableTile();
        CreatePiece();
    }

    private void CreateBackgroundTile()
    {
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (IsBlankSpace(row, column))
                    continue;

                Tile backTile = Instantiate(backgroundTilePrefab, new Vector2(row, column), Quaternion.identity).GetComponent<Tile>();
                tiles[row, column] = backTile;

                backTile.name = $"Tile [{row} , {column}]";
                backTile.transform.SetParent(transform);
            }
        }
    }

    public void CreateBreakableTile()
    {
        if (stageData.breakableTile.Length <= 0)
            return;

        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (IsBlankSpace(row, column) || !IsBreakableTile(row, column, true))
                    continue;

                BreakableTile breakableTile = Instantiate(breakableTilePrefab, new Vector2(row, column), Quaternion.identity).GetComponent<BreakableTile>();
                breakableTile.InitializeBreakableTile();
                tiles[row, column].breakableTile = breakableTile;

                breakableTile.name = $"BreakableTile [{row} , {column}]";
                breakableTile.transform.SetParent(tiles[row, column].transform);
            }
        }
    }

    private void CreatePiece(bool regenerate = false)
    {
        if (!regenerate)
        {
            for (int i = 0; i < horizontal * vertical + Mathf.RoundToInt((horizontal * vertical) * 0.5f); ++i)
            {
                Piece firstPiece = Instantiate(piecePrefab, Vector2.zero, Quaternion.identity).GetComponent<Piece>();

                firstPiece.name = StaticVariables.DisabledPieceName;
                firstPiece.transform.SetParent(disabledPiecesParent.transform);

                disabledPieces.Enqueue(firstPiece);
                firstPiece.gameObject.SetActive(false);
            }
        }

        if (regenerate)
            currBoardState = BoardState.Init;

        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (IsBlankSpace(row, column) || 
                    IsBreakableTile(row, column))
                    continue;

                Piece piece = null;

                if (regenerate)
                {
                    piece = GetPiece(row, column);

                    piece.SetPieceValue(randomValue);
                    piece.transform.position = new Vector2(piece.row, piece.column + fallOffset);
                }

                else
                {
                    piece = disabledPieces.Dequeue();

                    piece.SetPiece(this, randomValue, row, column);
                    SetTileInPiece(row, column, piece);

                    piece.transform.position = new Vector2(piece.row, piece.column + fallOffset);
                    piece.gameObject.SetActive(true);
                }
            }
        }

        CheckingTheMatchedPieced();

        currBoardState = BoardState.Init;
    }

    private void CheckingTheMatchedPieced()
    {
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (IsBlankSpace(row, column) ||
                    IsBreakableTile(row, column))
                    continue;

                Piece checkPiece = GetPiece(row, column);

                while (IsMatchedPiece(checkPiece))
                {
                    checkPiece.SetPieceValue(randomValue);
                }

                checkPiece.currState = PieceState.MOVE;
                movedPieces.Add(checkPiece);
            }
        }
    }

    private bool IsMatchedPiece(Piece piece)
    {
        if (piece.row < horizontal - 1 &&
            piece.row > 0)
        {
            if (!IsBlankSpace(piece.row + 1, piece.column) &&
                !IsBlankSpace(piece.row - 1, piece.column))
            {
                if (!IsBreakableTile(piece.row + 1, piece.column) &&
                    !IsBreakableTile(piece.row - 1, piece.column))
                {
                    if (EqualValuePiece(piece, GetPiece(piece.row + 1, piece.column)) &&
                        EqualValuePiece(piece, GetPiece(piece.row - 1, piece.column)))
                        return true;
                }
            }
        }

        if (piece.column < vertical - 1 &&
            piece.column > 0)
        {
            if (!IsBlankSpace(piece.row, piece.column + 1) &&
                !IsBlankSpace(piece.row, piece.column - 1))
            {
                if (!IsBreakableTile(piece.row, piece.column + 1) &&
                    !IsBreakableTile(piece.row, piece.column - 1))
                {
                    if (EqualValuePiece(piece, GetPiece(piece.row, piece.column + 1)) &&
                        EqualValuePiece(piece, GetPiece(piece.row, piece.column - 1)))
                        return true;
                }
            }
        }

        return false;
    }

    public void BoardStates()
    {
        if (FindMovingPiece()) // 선택된 피스가 있을때 입력방지
            return;

        switch (currBoardState)
        {
            case BoardState.Init:
                if (hintPieces.Count > 0) // Piece가 움직이고 있을 때 Hint Effect를 끈다.
                {
                    HintPieceSwitch(false);
                    hintPieces.Clear();
                }

                if (hintAccumTime > 0)
                    hintAccumTime = 0;

                currBoardState = BoardState.Order;
                break;

            case BoardState.Work:
                if (hintPieces.Count > 0) // Piece가 움직이고 있을 때 Hint Effect를 끈다.
                {
                    HintPieceSwitch(false);
                    hintPieces.Clear();
                }

                if (hintAccumTime > 0)
                    hintAccumTime = 0;

                if (selectedPiece == null)
                    return;

                if (selectedPiece.isTunning && selectedPiece.target.isTunning) // 매치가 안된 Piece를 되돌린다.
                {
                    selectedPiece.TunningNull();
                    return;
                }

                if (selectedPiece.crossBomb || 
                    selectedPiece.target.crossBomb)
                {
                    UsedCrossBomb(selectedPiece);
                    UsedCrossBomb(selectedPiece.target);
                    StartCoroutine(DisabledMatchedPiece());
                    return;
                }

                FindMatchedPiece(true);
                if (isMatched)
                    FindMatchedPiece();

                else
                {
                    GameManager.Instance.SoundEffectPlay(SEClip.Tunning);
                    selectedPiece.target.MoveToBack();
                    selectedPiece.MoveToBack();
                }

                break;

            case BoardState.Order:
                if (hintPieces.Count == 0)
                {
                    hintAccumTime += Time.deltaTime;

                    if (hintAccumTime > 3f)
                    {
                        if (DeadLockCheck())
                            CreatePiece(true);

                        else
                            HintPieceSwitch(true);
                    }
                }

                if (gameStageMgr.combo > 0)
                    gameStageMgr.combo = 0;

                break;
        }
    }

    private void FindMatchedPiece(bool check = false)
    {
        if (isMatched)
            isMatched = false;

        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (IsBlankSpace(row, column) || 
                    IsBreakableTile(row, column))
                    continue;

                else
                {
                    if (row > 0 && row < horizontal - 1)
                    {
                        if (!IsBlankSpace(row + 1, column) &&
                            !IsBlankSpace(row - 1, column))
                        {
                            if (!IsBreakableTile(row + 1, column) &&
                                !IsBreakableTile(row - 1, column))
                            {
                                AddMatchedPiece(GetPiece(row - 1, column), GetPiece(row, column), GetPiece(row + 1, column), check);

                                if (isMatched)
                                    return;

                            }
                        }
                    }

                    if (column > 0 && column < vertical - 1)
                    {
                        if (!IsBlankSpace(row, column + 1) &&
                            !IsBlankSpace(row, column - 1))
                        {
                            if (!IsBreakableTile(row, column + 1) &&
                                !IsBreakableTile(row, column - 1))
                            {
                                AddMatchedPiece(GetPiece(row, column + 1),
                                                GetPiece(row, column),
                                                GetPiece(row, column - 1), check);

                                if (isMatched)
                                    return;
                            }
                        }

                    }
                }
            }
        }

        if (check)
        {
            if (!isMatched)
                return;
        }

        // 매치된 피스가 있다
        if (matchedPieces.Count > 0)
        {
            GenerateItemPiece();
            StartCoroutine(DisabledMatchedPiece());
        }

        // 매치된 피스가 없다
        else
            currBoardState = BoardState.Order;
    }

    private void AddMatchedPiece(Piece sidePiece1, Piece currPiece, Piece sidePiece2, bool checking)
    {
       // if (sidePiece1.crossBomb || currPiece.crossBomb || sidePiece2.crossBomb)
         //   return ;

        if (EqualValuePiece(currPiece, sidePiece1) &&
            EqualValuePiece(currPiece, sidePiece2))
        {
            if (checking)
            {
                isMatched = true;
                return;
            }

            if (!matchedPieces.Contains(sidePiece1))
                matchedPieces.Add(sidePiece1);

            if (!matchedPieces.Contains(currPiece))
                matchedPieces.Add(currPiece);

            if (!matchedPieces.Contains(sidePiece2))
                matchedPieces.Add(sidePiece2);

            UsedItem(sidePiece1, currPiece, sidePiece2);
        }
    }

    private void GenerateItemPiece()
    {
        if (matchedPieces.Count <= 3) // Match가 3개 이하면 아이템 생성할 필요가 없다.
            return;

        int rows = 0; // 가로로 매치된 Piece를 검사
        int cols = 0; // 세로로 매치된 Piece를 검사
        int prevCount = 0; // Piece의 갯수를 저장, 저장된 기점부터 조건에 맞지않는 Piece를 빼기위함

        // Cross Bomb
        if (matchedPieces.Count > 4)
        {
            for (int i = 0; i < matchedPieces.Count; ++i) // Match된 Piece를 순회
            {
                if (verifiedPieces.Contains(matchedPieces[i])) // 검사한 Piece는 검사에서 제외한다.
                    continue;

                verifiedPieces.Add(matchedPieces[i]); // 검사할 Piece를 먼저 넣는다.
                FindDirectionMatchedPiece(matchedPieces[i], ref rows, ref cols); // 검사할 피스를 기점으로 네 방향으로 검사한다.

                if (rows >= 2 && cols >= 2) // 가로로 매치된 블럭이 2개 이상, 세로로 매치된 블럭이 2개 이상
                {
                    Piece bombPiece = matchedPieces[i]; // 아이템 Piece로 만들 대상

                    if (selectedPiece != null)
                    {
                        if (EqualValuePiece(selectedPiece, matchedPieces[i]))
                            bombPiece = selectedPiece; // 움직인 Piece가 있으면 해당 Piece를 ItemPiece로 만든다.

                        if (selectedPiece.target != null)
                        {
                            if (EqualValuePiece(selectedPiece.target, matchedPieces[i]))
                                bombPiece = selectedPiece.target;
                        }
                    }

                    else
                        bombPiece = verifiedPieces[Random.Range(prevCount, verifiedPieces.Count)];

                    bombPiece.crossBomb = true; // 대상의 CrossBomb으로 만든다.
                    bombPiece.value = pieceSprites.Length + 1; // 움직여서 사용하는 아이템, 매치가 될 수 없도록 스프라이트의 총 길이에서 1을 더해준다.
                    bombPiece.itemSprite.sprite = itemSprites[2]; // 아이템 스프라이트를 적용

                    matchedPieces.Remove(bombPiece); // 적용된 아이템은 Matched리스트에서 뺸다.

                    prevCount = verifiedPieces.Count; // Elements의 Index 끝을 저장
                    //Debug.Log($"prev Count is {prevCount}");
                }

                else
                {
                    if (prevCount != verifiedPieces.Count)
                    {
                        verifiedPieces.RemoveRange(prevCount, (verifiedPieces.Count - 1) - prevCount); // 이번에 검사한 Piece를 삭제한다.
                        //Debug.Log($"verifiedPieces Count is {verifiedPieces.Count - 1}");
                    }
                }

                rows = 0; // 가로 초기화
                cols = 0; // 세로 초기화
            }
        }

        // Row Bomb
        for (int i = 0; i < matchedPieces.Count - 1; ++i)
        {
            if (verifiedPieces.Contains(matchedPieces[i])) // 검사된 Piece는 제외
                continue;

            for (int r = 1; r < horizontal; ++r)
            {
                if (matchedPieces[i].row + r > horizontal - 1)
                    break;

                if (IsBlankSpace(matchedPieces[i].row + r, matchedPieces[i].column) ||
                    IsBreakableTile(matchedPieces[i].row + r, matchedPieces[i].column))
                    break;

                Piece check = GetPiece(matchedPieces[i].row + r,
                                       matchedPieces[i].column);

                if (matchedPieces.Contains(check) &&
                    EqualValuePiece(matchedPieces[i], check))
                {
                    verifiedPieces.Add(check);
                    rows++;
                }

                else
                    break;
            }

            if (rows > 2)
            {
                Piece bombPiece = matchedPieces[i];

                if (selectedPiece != null)
                {
                    if (EqualValuePiece(selectedPiece, matchedPieces[i]))
                        bombPiece = selectedPiece;

                    if (selectedPiece.target != null)
                    {
                        if (EqualValuePiece(selectedPiece.target, matchedPieces[i]))
                            bombPiece = selectedPiece.target;
                    }
                }

                else
                    bombPiece = verifiedPieces[Random.Range(prevCount, verifiedPieces.Count)];

                bombPiece.rowBomb = true;
                bombPiece.itemSprite.sprite = itemSprites[1];

                matchedPieces.Remove(bombPiece);

                prevCount = verifiedPieces.Count;
                //Debug.Log($"prev Count is {prevCount}");
            }

            else
            {
                if (prevCount != verifiedPieces.Count)
                {
                    verifiedPieces.RemoveRange(prevCount, (verifiedPieces.Count - 1) - prevCount);
                    //Debug.Log($"verifiedPieces Count is {verifiedPieces.Count - 1}");
                }
            }

            rows = 0;
        }

        // Column Bomb
        for (int i = 0; i < matchedPieces.Count - 1; ++i)
        {
            if (verifiedPieces.Contains(matchedPieces[i]))
                continue;

            for (int c = 1; c < vertical; ++c)
            {
                if (matchedPieces[i].column + c > vertical - 1)
                    break;

                if (IsBlankSpace(matchedPieces[i].row, matchedPieces[i].column + c) ||
                    IsBreakableTile(matchedPieces[i].row, matchedPieces[i].column + c))
                    break;

                Piece check = GetPiece(matchedPieces[i].row,
                                   matchedPieces[i].column + c);

                if (matchedPieces.Contains(check) &&
                    EqualValuePiece(matchedPieces[i], check))
                {
                    verifiedPieces.Add(check);
                    cols++;
                }

                else
                    break;
            }

            if (cols > 2)
            {
                Piece bombPiece = matchedPieces[i];

                if (selectedPiece != null)
                {
                    if (EqualValuePiece(selectedPiece, matchedPieces[i]))
                        bombPiece = selectedPiece;

                    if (selectedPiece.target != null)
                    {
                        if (EqualValuePiece(selectedPiece.target, matchedPieces[i]))
                            bombPiece = selectedPiece.target;
                    }
                }

                else
                    bombPiece = verifiedPieces[Random.Range(prevCount, verifiedPieces.Count)];

                bombPiece.columnBomb = true;
                bombPiece.itemSprite.sprite = itemSprites[0];

                matchedPieces.Remove(bombPiece);

                prevCount = verifiedPieces.Count;
                //Debug.Log($"prev Count is {prevCount}");
            }

            else
            {
                if (prevCount != verifiedPieces.Count)
                {
                    verifiedPieces.RemoveRange(prevCount, (verifiedPieces.Count - 1) - prevCount);
                    //Debug.Log($"verifiedPieces Count is {verifiedPieces.Count - 1}");
                }
            }

            cols = 0;
        }
    }

    private void FindDirectionMatchedPiece(Piece piece, ref int rows, ref int cols)
    {
        Vector2[] direction =
        {
            Vector2.up,
            Vector2.right,
            Vector2.down,
            Vector2.left
        };

        // 네 방향 순회
        for (int i = 0; i < direction.Length; ++i)
        {
            int move = 0;
            while (true)
            {
                move++;

                if (piece.row + ((int)direction[i].x * move) > horizontal - 1 || piece.column + ((int)direction[i].y * move) < 0 || // 피스가 현재 경계선에 인접해있을 때 루프 탈출
                    piece.column + ((int)direction[i].y * move) > vertical - 1 || piece.row + ((int)direction[i].x * move) < 0)
                    break;

                if (IsBlankSpace(piece.row + ((int)direction[i].x * move), piece.column + ((int)direction[i].y * move)) ||
                    IsBreakableTile(piece.row + ((int)direction[i].x * move), piece.column + ((int)direction[i].y * move)))
                    break;

                Piece check = GetPiece(piece.row + ((int)direction[i].x * move), piece.column + ((int)direction[i].y * move)); // 순회하는 방향으로 Piece를 가져온다.

                // 매치된 블럭이고 같은 색상의 Piece일때
                if (matchedPieces.Contains(check) &&
                    EqualValuePiece(piece, check))
                {
                    if (verifiedPieces.Contains(check)) // 검증된 Piece라면 반복문을 빠져나간다.
                        break;

                    // 검사가 끝난 블럭은 리스트에 저장
                    verifiedPieces.Add(check);

                    // 검사하는 블럭의 인접한 블럭이 
                    if (FindNeighborPiece(check))
                        FindDirectionMatchedPiece(check, ref rows, ref cols);

                    // 상하인 경우 cols 증가
                    if (direction[i] == direction[0] ||
                        direction[i] == direction[2])
                        ++cols;

                    // 좌우인 경우 rows 증가
                    else if (direction[i] == direction[1] || direction[i] == direction[3])
                        ++rows;
                }

                // 블럭이 없으면 즉시 취소
                else
                    break;
            }
        }
    }

    private bool FindNeighborPiece(Piece piece)
    {
        if (piece.row + 1 < horizontal - 1)
        {
            if (!IsBlankSpace(piece.row + 1, piece.column) &&
                !IsBreakableTile(piece.row + 1, piece.column))
            {
                Piece right = GetPiece(piece.row + 1, piece.column);

                if (!verifiedPieces.Contains(right)) // 검증되지않은 Piece
                {
                    if (matchedPieces.Contains(right) && // 매치된 피스이고 같은 색상의 Piece일떄
                        EqualValuePiece(piece, right))
                        return true;
                }
            }
        }

        if (piece.row - 1 >= 0)
        {
            if (!IsBlankSpace(piece.row - 1, piece.column) &&
                !IsBreakableTile(piece.row - 1, piece.column))
            {
                Piece left = GetPiece(piece.row - 1, piece.column);
                if (!verifiedPieces.Contains(left)) // 검증되지않은 Piece
                {
                    if (matchedPieces.Contains(left) && // 매치된 피스이고 같은 색상의 Piece일떄
                        EqualValuePiece(piece, left))
                        return true;
                }
            }
        }

        if (piece.column + 1 < vertical - 1)
        {
            if (!IsBlankSpace(piece.row, piece.column + 1) &&
                !IsBreakableTile(piece.row, piece.column + 1))
            {
                Piece up = GetPiece(piece.row, piece.column + 1);

                if (!verifiedPieces.Contains(up)) // 검증되지않은 Piece
                {
                    if (matchedPieces.Contains(up) && // 매치된 피스이고 같은 색상의 Piece일떄
                        EqualValuePiece(piece, up))
                        return true;
                }
            }
        }

        if (piece.column - 1 >= 0)
        {
            if (!IsBlankSpace(piece.row, piece.column - 1) &&
                !IsBreakableTile(piece.row, piece.column - 1))
            {
                Piece down = GetPiece(piece.row, piece.column - 1);
                if (!verifiedPieces.Contains(down)) // 검증되지않은 Piece
                {
                    if (matchedPieces.Contains(down) && // 매치된 피스이고 같은 색상의 Piece일떄
                        EqualValuePiece(piece, down))
                        return true;
                }
            }
        }

        return false;
    }

    private void UsedItem(Piece piece1, Piece piece2, Piece piece3)
    {
        if (piece1.rowBomb)
        {
            piece1.rowBomb = false;
            GetRowPieces(piece1.column);
            piece1.PieceEffectPlay(PieceEffect.ROWBOMB);
        }

        if (piece2.rowBomb)
        {
            piece2.rowBomb = false;
            GetRowPieces(piece2.column);
            piece2.PieceEffectPlay(PieceEffect.ROWBOMB);
        }

        if (piece3.rowBomb)
        {
            piece3.rowBomb = false;
            GetRowPieces(piece3.column);
            piece3.PieceEffectPlay(PieceEffect.ROWBOMB);
        }

        if (piece1.columnBomb)
        {
            piece1.columnBomb = false;
            GetColumnPieces(piece1.row);
            piece1.PieceEffectPlay(PieceEffect.COLUMNBOMB);
        }
        if (piece2.columnBomb)
        {
            piece2.columnBomb = false;
            GetColumnPieces(piece2.row);
            piece2.PieceEffectPlay(PieceEffect.COLUMNBOMB);
        }
        if (piece3.columnBomb)
        {
            piece3.columnBomb = false;
            GetColumnPieces(piece3.row);
            piece3.PieceEffectPlay(PieceEffect.COLUMNBOMB);
        }
    }

    private void UsedCrossBomb(Piece piece)
    {
        if (piece.crossBomb)
        {
            piece.crossBomb = false;
            GetRowPieces(piece.column);
            GetColumnPieces(piece.row);
            piece.PieceEffectPlay(PieceEffect.CROSSBOMB);
        }
    }

    private void GetRowPieces(int column)
    {
        for (int row = 0; row < horizontal; ++row)
        {
            if (IsBlankSpace(row, column))
                continue;

            if(IsBreakableTile(row, column))
            {
                GetBreakableTile(row, column).BreakableTileDamage();
                continue;
            }

            Piece rowPiece = GetPiece(row, column);

            if (rowPiece.columnBomb)
            {
                GetColumnPieces(rowPiece.row); // 없앨려는 Piece가 Column Bomb 아이템일 경우
                rowPiece.columnBomb = false; // Column Bomb을 비활성화
            }

            if (rowPiece.crossBomb)
            {
                UsedCrossBomb(rowPiece); // 없앨려는 Piece가 Cross Bomb 아이템일 경우
               //rowPiece.crossBomb = false; // Cross Bomb을 비활성화
            }

            if (!matchedPieces.Contains(rowPiece)) // 매치리스트에 넣는다.
                matchedPieces.Add(rowPiece);

            if (!verifiedPieces.Contains(rowPiece)) // 아이템에 의해 삭제된 Piece는 검사에서 제외한다.
                verifiedPieces.Add(rowPiece);
        }
    }

    private void GetColumnPieces(int row)
    {
        for (int column = 0; column < vertical; ++column)
        {
            if (IsBlankSpace(row, column))
                continue;

            if (IsBreakableTile(row, column))
            {
                GetBreakableTile(row, column).BreakableTileDamage();
                continue;
            }

            Piece columnPiece = GetPiece(row, column);

            if (columnPiece.rowBomb)
            {
                GetRowPieces(columnPiece.column);
                columnPiece.rowBomb = false;
            }

            if (columnPiece.crossBomb)
            {
                UsedCrossBomb(columnPiece);
                //columnPiece.crossBomb = false;
            }

            if (!matchedPieces.Contains(columnPiece))
                matchedPieces.Add(columnPiece);

            if (!verifiedPieces.Contains(columnPiece))
                verifiedPieces.Add(columnPiece);
        }
    }

    IEnumerator DisabledMatchedPiece()
    {
        // 선택한 Piece가 있을 때, 처음으로 움직인 것을 안다. 
        if (selectedPiece != null)
        {
            gameStageMgr.DecreaseMove(gameStageMgr.decreaseMoveValue);
            selectedPiece = null;
        }

        // 매치된 Piece는 비활성화 시킨다.
        foreach (var piece in matchedPieces)
        {
            piece.PieceEffectPlay(PieceEffect.PIECEEXPLOSION);
            NeighborBreakableTileDamage(piece.row, piece.column); // 인접한 BreakableTile에 데미지를 준다.
            piece.SetDisabledPiece();
            tiles[piece.row, piece.column].piece = null;
        }

        DamagedResetBreakableTile(); // 이번 턴에 데미지 받은 타일은 다음 턴에서 데미지를 적용 받을 수 있게 켜준다.

        GameManager.Instance.SoundEffectPlay(SEClip.Matched);

        yield return new WaitForSeconds(effectDelayTime);

        ++gameStageMgr.combo; // 콤보를 증가시킨다.

        for (int i = matchedPieces.Count - 1; i > -1; --i)
        {
            gameStageMgr.IncreaseScore(gameStageMgr.matchedScore); // 점수만큼 증가

            matchedPieces[i].transform.SetParent(disabledPiecesParent.transform);
            matchedPieces[i].gameObject.SetActive(false);

            disabledPieces.Enqueue(matchedPieces[i]);
        }

        matchedPieces.Clear();
        verifiedPieces.Clear();

        StartCoroutine(FallingPieces());
    }

    public void NeighborBreakableTileDamage(int row, int column)
    {
        Vector2[] direction =
            {
                Vector2.up,
                Vector2.right,
                Vector2.down,
                Vector2.left
            };

        for (int i = 0; i < direction.Length; ++i)
        {
            if (IsBreakableTile(row + (int)direction[i].x, column + (int)direction[i].y))
                GetBreakableTile(row + (int)direction[i].x, column + (int)direction[i].y).BreakableTileDamage();
        }
    }

    public BreakableTile GetBreakableTile(int row, int column)
    {
        return tiles[row, column].breakableTile;
    }

    IEnumerator FallingPieces()
    {
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (IsBlankSpace(row, column) ||
                    IsBreakableTile(row, column))
                    continue;

                if (TileIsNullPiece(row, column)) // 자리에 피스가 없다.
                {
                    for (int i = column + 1; i < vertical; ++i) // 빈 자리 위로 피스가 있는지 확인한다.
                    {
                        if (IsBlankSpace(row, i) ||
                            IsBreakableTile(row, i))
                            continue;

                        if (!TileIsNullPiece(row, i)) // 한칸씩 위로 이동하며 피스가 있는지 확인한다.
                        {
                            Piece fallPiece = GetPiece(row, i); // 빈 자리 위에 있는 블럭

                            fallPiece.SetPiece(this, fallPiece.value, fallPiece.row, column);
                            SetTileInPiece(fallPiece.row, fallPiece.column, fallPiece);

                            tiles[row, i].piece = null; // 그 자리를 비운다.

                            fallPiece.currState = PieceState.MOVE;
                            movedPieces.Add(fallPiece);
                            break;
                        }
                    }
                }
            }
        }

        int fall = 0;
        for (int row = 0; row < horizontal; ++row)
        {
            for (int column = 0; column < vertical; ++column)
            {
                if (IsBlankSpace(row, column) ||
                    IsBreakableTile(row, column))
                    continue;

                if (TileIsNullPiece(row, column)) // 자리에 피스가 없다.
                {
                    Piece enabledPiece = disabledPieces.Dequeue();

                    enabledPiece.SetPiece(this, Random.Range(0, pieceSprites.Length), row, column);
                    SetTileInPiece(enabledPiece.row, enabledPiece.column, enabledPiece);

                    enabledPiece.transform.position = new Vector2(enabledPiece.row, vertical + fall);
                    enabledPiece.gameObject.SetActive(true);

                    enabledPiece.currState = PieceState.MOVE;
                    movedPieces.Add(enabledPiece);
                    ++fall;
                }
            }

            fall = 0;
        }

        while (FindMovingPiece())
         {
             yield return null;
         }

        yield return new WaitForSeconds(delayTime);

        FindMatchedPiece();
    }

    public void SetTileInPiece(int row, int column, Piece piece)
    {
        tiles[row, column].piece = piece;
        piece.transform.SetParent(tiles[row, column].transform);
    }

    public Tile GetTile(int row, int column)
    {
        return tiles[row, column];
    }

    public Piece GetPiece(int row, int column)
    {
        return tiles[row, column].piece;
    }

    private bool TileIsNullPiece(int row, int column)
    { 
        return tiles[row, column].piece == null;
    }

    private bool TileIsNullBreakableTile(int row, int column)
    {
        return tiles[row, column].breakableTile == null;
    }

    private bool EqualValuePiece(Piece piece1, Piece piece2)
    {
        return piece1.value == piece2.value;
    }

    public bool IsBreakableTile(int row, int column, bool init = false)
    {
        for (int i = 0; i < stageData.breakableTile.Length; ++i)
        {
            if (stageData.breakableTile[i].x == row &&
                stageData.breakableTile[i].y == column)
            {
                if(init)
                 return true;

                if (GetTile(row, column).breakableTile.tileBreak)
                    return false;

                else
                    return true;
            }
        }

        return false;
    }

    public void DamagedResetBreakableTile()
    {
        for (int i = 0; i < stageData.breakableTile.Length; ++i)
        {
            BreakableTile b_tile = GetBreakableTile((int)stageData.breakableTile[i].x, (int)stageData.breakableTile[i].y);

            if (!b_tile.tileBreak)
                b_tile.damaged = false;
        }
    }

    public bool IsBlankSpace(int row, int column)
    {
        for (int i = 0; i < stageData.blankSpace.Length; ++i)
        {
            if (stageData.blankSpace[i].x == row &&
                stageData.blankSpace[i].y == column)
                return true;
        }

        return false;
    }

    private bool DeadLockCheck()
    {
        Vector2[] direction =
            {
                Vector2.right,
                Vector2.up
            };

        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (IsBlankSpace(row, column) || IsBreakableTile(row, column))
                    continue;

                if (row < horizontal - 1 && column < vertical - 1)
                {
                    SwapPiece(row, column, direction[0]);
                    FindMatchedPiece(true);
                    if (isMatched)
                    {
                        hintPieces.Add(GetPiece(row, column));
                        hintPieces.Add(GetPiece(row + 1, column));

                        SwapPiece(row, column, direction[0]);

                        return false;
                    }
                    SwapPiece(row, column, direction[0]);

                    SwapPiece(row, column, direction[1]);
                    FindMatchedPiece(true);
                    if(isMatched)
                    {
                        hintPieces.Add(GetPiece(row, column));
                        hintPieces.Add(GetPiece(row, column + 1));

                        SwapPiece(row, column, direction[1]);

                        return false;
                    }
                    SwapPiece(row, column, direction[1]);
                }
            }
        }

        return true;
    }

    private void SwapPiece(int row, int column, Vector2 direction)
    {
        if (!IsBlankSpace(row + (int)direction.x, column + (int)direction.y) &&
            !IsBreakableTile(row + (int)direction.x, column + (int)direction.y))
        {
            Piece swap = tiles[row + (int)direction.x, column + (int)direction.y].piece;
            tiles[row + (int)direction.x, column + (int)direction.y].piece = tiles[row, column].piece;
            tiles[row, column].piece = swap;
        }
    }

    private bool FindMovingPiece()
    {
        if (movedPieces.Count > 0)
        {
            for (int i = movedPieces.Count - 1; i >= 0; --i)
            {
                if (movedPieces[i].currState == PieceState.MOVE)
                    return true;

                else if (movedPieces[i].currState == PieceState.WAIT)
                    movedPieces.Remove(movedPieces[i]);
            }
        }

        return false;
    }

    private void HintPieceSwitch(bool hintSwitch = false)
    {
        if (hintSwitch)
        {
            hintPieces[0].PieceEffectPlay(PieceEffect.HINTEFFECT);
            hintPieces[1].PieceEffectPlay(PieceEffect.HINTEFFECT);
        }

        else
        {
            hintPieces[0].PieceEffectStop(PieceEffect.HINTEFFECT);
            hintPieces[1].PieceEffectStop(PieceEffect.HINTEFFECT);

           // hintPieces.Clear();
        }
    }

	public void DebugSystem()
    {
        // debug Array Check
        if (Input.GetKeyDown(KeyCode.A))
            DebugBoardChecking(false);

        // debug Piece Moving Check
        if (Input.GetKeyDown(KeyCode.B))
            DebugBoardChecking(true);

        // debug Board Current State Check
        if (Input.GetKeyDown(KeyCode.D))
            Debug.Log(currBoardState);

        // debug Deadlock Check
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (DeadLockCheck())
                Debug.Log("is DeadLock !!");
            else
                Debug.Log("No DeadLock");

        }

        // debug Item Check
        if (Input.GetKeyDown(KeyCode.G))
            DebugBoardItemChecking();

        // debug Board Reset
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            selectedPiece = null;
            CreatePiece(true);
        }

        // debug Generate Item
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selectedPiece != null)
            {
                selectedPiece.itemSprite.sprite = itemSprites[0];
                selectedPiece.rowBomb = true;
            }
        }

        // debug Generate Item
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (selectedPiece != null)
            {
                selectedPiece.itemSprite.sprite = itemSprites[1];
                selectedPiece.columnBomb = true;
            }
        }

        // debug Generate Item
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (selectedPiece != null)
            {
                selectedPiece.itemSprite.sprite = itemSprites[2];
                selectedPiece.crossBomb = true;
            }
        }
    }

    private void DebugBoardChecking(bool moving = false)
    {
        StringBuilder sb = new StringBuilder(1000);

        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (tiles[row, column] != null)
                {
                    Piece piece = GetPiece(row, column);
                    /*if (boardIndex[row, column].GetComponent<Block>().value == GetPiece(row, column).value)
                        Debug.Log("[" + row + " , " + column + "] = " + GetPiece(row, column).value);*/

                    if (!moving)
                    {
                        sb.Append($"[{row} , {column}] = {piece.value} || PosX : {piece.transform.position.x} , PosY :  {piece.transform.position.y} \n");
                        //Debug.Log("[" + row + " , " + column + "] = " + piece.value + ", PosX : " + piece.transform.position.x + ", PosY : " + piece.transform.position.y);
                    }

                    else
                    {
                        if (piece.currState == PieceState.MOVE)
                            Debug.Log($"[{row} , {column}]] is Moving");
                    }
                }
            }
        }

        Debug.Log(sb.ToString());
        //sb.Clear();
    }

    private void DebugBoardItemChecking()
    {
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (tiles[row, column] != null)
                {
                    Piece piece = GetPiece(row, column);

                    if (piece.rowBomb || piece.columnBomb || piece.crossBomb)
                        Debug.Log("is itemPiece");
                }
            }
        }
    }

    /*public void PieceListSort(List<Piece> pieces, bool column = false, bool descending = false)
    {
        if (column)
        {
            if (descending)
            {
                pieces.Sort(delegate (Piece a, Piece b)
                {
                    if (a.column < b.column)
                        return 1;

                    else if (a.column > b.column)
                        return -1;

                    else
                        return 0;
                });
            }

            else
            {
                pieces.Sort(delegate (Piece a, Piece b)
                {
                    if (a.column > b.column)
                        return 1;

                    else if (a.column < b.column)
                        return -1;

                    else
                        return 0;
                });
            }
        }

        else
        {
            if (descending)
            {
                pieces.Sort(delegate (Piece a, Piece b)
                {
                    if (a.row < b.row)
                        return 1;

                    else if (a.row > b.row)
                        return -1;

                    else
                        return 0;
                });
            }

            else
            {
                pieces.Sort(delegate (Piece a, Piece b)
                {
                    if (a.row > b.row)
                        return 1;

                    else if (a.row < b.row)
                        return -1;

                    else
                        return 0;
                });
            }
        }
    }*/
}