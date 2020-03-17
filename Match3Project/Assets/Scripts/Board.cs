using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum BoardState
{
    ORDER,
    WORK,
}

public class Board : MonoBehaviour
{
    [Header("Board Parts")]
    public BoardState currBoardState;

    private int horizontal;
    public int Horizontal => horizontal;

    private int vertical;
    public int Vertical => vertical;

    public int fallOffset = 5;

    public float delayTime = 0.08f;
    public float effectDelayTime = 0.5f;
    public float pieceFallSpeed;

    public StageDataScriptableObject stagedata_so;
    private StageData stageData;
    public StageController stageCtrl;

    [Header("Piece Parts")]
    public Tile[,] tiles;

    public GameObject piecePrefab;
    public GameObject backgroundTilePrefab;
    public GameObject breakableTilePrefab;

    public Piece selectedPiece;

    public Transform disabledPiecesParent;

    public Sprite[] pieceSprites;
    public Sprite[] itemSprites;

    private Queue<Piece> disabledPieces;
    private List<Piece> matchedPieces;
    private List<Piece> verifiedPieces;
    private List<Piece> hintPieces;

    private int randomValue => Random.Range(0, pieceSprites.Length);
    private bool isMatched = false;

    private float hintAccumTime;
    private CamPivot camPivot;
    public void InitializeBoard()
    {
        currBoardState = BoardState.WORK;

        stageCtrl.board = this;

        pieceSprites = Resources.LoadAll<Sprite>(StaticVariables.PieceSpritesPath);
        itemSprites = Resources.LoadAll<Sprite>(StaticVariables.ItemSpritesPath);

        disabledPieces = new Queue<Piece>();

        matchedPieces = new List<Piece>();
        verifiedPieces = new List<Piece>();
        hintPieces = new List<Piece>();

        stageData = stagedata_so.stageDatas.Find(s => s.stageLevel == 1);
        vertical = stageData.vertical;
        horizontal = stageData.horizontal;

        camPivot = Camera.main.GetComponent<CamPivot>();
        camPivot.SetCameraPivot(horizontal, vertical);

        tiles = new Tile[horizontal, vertical];

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
        if(!regenerate)
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

        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (IsBlankSpace(row, column) || IsBreakableTile(row, column))
                    continue;

                if (regenerate)
                {
                    Piece regenPiece = GetPiece(row, column);

                    regenPiece.SetPieceValue(randomValue);
                    regenPiece.transform.position = new Vector2(regenPiece.row, regenPiece.column + fallOffset);
                }

                else
                {
                    Piece createPiece = disabledPieces.Dequeue();

                    createPiece.SetPiece(this, randomValue, row, column);
                    SetTileInPiece(row, column, createPiece);

                    createPiece.transform.position = new Vector2(createPiece.row, createPiece.column + fallOffset);
                    createPiece.gameObject.SetActive(true);
                }
            }
        }

        StartCoroutine(CheckingTheMatchedPiece());
    }

    IEnumerator CheckingTheMatchedPiece()
    {
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (IsBlankSpace(row,column) || IsBreakableTile(row, column))
                    continue;

                Piece checkPiece = GetPiece(row, column);

                while (IsMatchedPiece(checkPiece))
                {
                    checkPiece.SetPieceValue(randomValue);
                }

                checkPiece.currState = PieceState.MOVE;
            }
        }

       // yield return new WaitUntil(() => !FindMovingPiece());
       while(FindMovingPiece())
        {
            yield return null;
        }

        yield return new WaitForSeconds(delayTime);

        currBoardState = BoardState.ORDER;
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
                    if (EqualPiece(piece, GetPiece(piece.row + 1, piece.column)) &&
                        EqualPiece(piece, GetPiece(piece.row - 1, piece.column)))
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
                    if (EqualPiece(piece, GetPiece(piece.row, piece.column + 1)) &&
                        EqualPiece(piece, GetPiece(piece.row, piece.column - 1)))
                        return true;
                }
            }
        }

        return false;
    }

    public void BoardStates()
    {
        DebugSystem();

        switch (currBoardState)
        {
            case BoardState.WORK:
                if (hintPieces.Count > 0) // Piece가 움직이고 있을 때 Hint Effect를 끈다.
                {
                    HintPieceSwitch(false);
                    hintPieces.Clear();

                    hintAccumTime = 0;
                }

                if (FindMovingPiece() || selectedPiece == null) // 선택된 피스가 있거나 움직이는 피스가 있을때 입력 방지
                    return;

                if (selectedPiece.isTunning && selectedPiece.target.isTunning) // 매치가 안된 Piece를 되돌린다.
                {
                    selectedPiece.TunningNull();
                    return;
                }

                if (selectedPiece.crossBomb || selectedPiece.target.crossBomb)
                {
                    UsedCrossBomb(selectedPiece);
                    UsedCrossBomb(selectedPiece.target);
                    FindMatchedPiece();
                    return;
                }

                FindMatchedPiece(true);

                if (isMatched)
                    FindMatchedPiece();

                else
                {
                    //stageCtrl.SoundEffectPlay(SoundEffectList.TUNNING);
                    selectedPiece.target.MoveToBack();
                    selectedPiece.MoveToBack();
                }

                break;

            case BoardState.ORDER:
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
                if (IsBlankSpace(row, column) || IsBreakableTile(row, column))
                    continue;

                else // 매치된 피스를 검사
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
                                                GetPiece(row, column - 1),check);

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
            //GenerateItemPiece();
            StartCoroutine(DisabledMatchedPiece());
        }

        // 매치된 피스가 없다
        else
        {
            stageCtrl.combo = 0;
            currBoardState = BoardState.ORDER;
        }
    }

    private void AddMatchedPiece(Piece sidePiece1, Piece currPiece, Piece sidePiece2, bool checking)
    {
        if (EqualPiece(currPiece, sidePiece1) &&
            EqualPiece(currPiece, sidePiece2))
        {
            if(checking)
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
        if (matchedPieces.Count < 3)
            return;

        int rows = 0;
        int cols = 0;
        int prevCount = 0;

        // Cross Bomb
        if (matchedPieces.Count > 4)
        {
            for (int i = 0; i < matchedPieces.Count - 1; ++i)
            {
                if (verifiedPieces.Contains(matchedPieces[i]))
                    continue;

                verifiedPieces.Add(matchedPieces[i]);
                FindDirectionMatchedPiece(matchedPieces[i], ref rows, ref cols);

                if (rows >= 2 && cols >= 2)
                {
                    Piece bombPiece = matchedPieces[i];

                    if (selectedPiece != null)
                    {
                        if (selectedPiece.value == matchedPieces[i].value)
                            bombPiece = selectedPiece;

                        if (selectedPiece.target != null && selectedPiece.target.value == matchedPieces[i].value)
                            bombPiece = selectedPiece.target;
                    }

                    else
                        bombPiece = verifiedPieces[Random.Range(prevCount, verifiedPieces.Count)];

                    bombPiece.crossBomb = true;
                    bombPiece.value = pieceSprites.Length + 1;
                    bombPiece.itemSprite.sprite = itemSprites[2];

                    matchedPieces.Remove(bombPiece);

                    prevCount = verifiedPieces.Count - 1;
                }

                else
                {
                    if (verifiedPieces.Count >= 3)
                        verifiedPieces.RemoveRange(prevCount, (verifiedPieces.Count - 1) - prevCount);

                    else
                        verifiedPieces.Clear();
                }

                rows = 0;
                cols = 0;
            }
        }

        // Row Bomb
        for (int i = 0; i < matchedPieces.Count - 1; ++i)
        {
            if (verifiedPieces.Contains(matchedPieces[i]))
                continue;

            for (int r = 1; r < horizontal - 1; ++r)
            {
                if (matchedPieces[i].row + r > horizontal - 1)
                    break;

                Piece check = GetPiece(matchedPieces[i].row + r, matchedPieces[i].column);

                if (matchedPieces.Contains(check) && matchedPieces[i].value == check.value)
                {
                    verifiedPieces.Add(check);
                    rows++;
                }

                else
                    break;
            }

            if (rows >= 3)
            {
                Piece bombPiece = matchedPieces[i];

                if(selectedPiece != null)
                {
                    if (selectedPiece.value == matchedPieces[i].value)
                        bombPiece = selectedPiece;

                    if (selectedPiece.target != null && selectedPiece.target.value == matchedPieces[i].value)
                        bombPiece = selectedPiece.target;
                }
                
                else
                    bombPiece = verifiedPieces[Random.Range(prevCount, verifiedPieces.Count)];

                bombPiece.rowBomb = true;
                bombPiece.itemSprite.sprite = itemSprites[1];

                matchedPieces.Remove(bombPiece);

                prevCount = verifiedPieces.Count - 1;
            }

            else
            {
                if (verifiedPieces.Count >= 3)
                    verifiedPieces.RemoveRange(prevCount, (verifiedPieces.Count - 1) - prevCount);

                else
                    verifiedPieces.Clear();
            }

            rows = 0;
        }

        // Column Bomb
        for (int i = 0; i < matchedPieces.Count - 1; ++i)
        {
            if (verifiedPieces.Contains(matchedPieces[i]))
                continue;

            for (int c = 1; c < vertical - 1; ++c)
            {
                if (matchedPieces[i].column + c > vertical - 1)
                    break;

                Piece check = GetPiece(matchedPieces[i].row, matchedPieces[i].column + c);

                if (matchedPieces.Contains(check) && matchedPieces[i].value == check.value)
                {
                    verifiedPieces.Add(check);
                    cols++;
                }

                else
                    break;
            }

            if (cols >= 3)
            {
                Piece bombPiece = matchedPieces[i];

                if (selectedPiece != null)
                {
                    if (selectedPiece.value == matchedPieces[i].value)
                        bombPiece = selectedPiece;

                    if (selectedPiece.target != null && selectedPiece.target.value == matchedPieces[i].value)
                        bombPiece = selectedPiece.target;
                }

                else
                    bombPiece = verifiedPieces[Random.Range(prevCount, verifiedPieces.Count)];

                bombPiece.columnBomb = true;
                bombPiece.itemSprite.sprite = itemSprites[0];

                matchedPieces.Remove(bombPiece);

                prevCount = verifiedPieces.Count - 1;
            }

            else
            {
                if (verifiedPieces.Count >= 3)
                    verifiedPieces.RemoveRange(prevCount, verifiedPieces.Count  - prevCount);

                else
                    verifiedPieces.Clear();
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

        foreach (var dir in direction)
        {
            for (int i = 1; ; ++i)
            {
                if (piece.row + ((int)dir.x * i) >= horizontal || piece.column + ((int)dir.y * i) >= vertical ||
                    piece.row + ((int)dir.x * i) <= -1         || piece.column + ((int)dir.y * i) <= 1)
                    break;

                Piece check = GetPiece(piece.row    + ((int)dir.x * i), 
                                       piece.column + ((int)dir.y * i));

                // 매치된 블럭일 때
                if (matchedPieces.Contains(check) && check.value == piece.value)
                {
                    if (verifiedPieces.Contains(check))
                        break;

                    // 체크가 끝난 블럭은 검사에서 제외
                    verifiedPieces.Add(check);

                    // 검사하는 블럭에 상하좌우에 다른 매치된 블럭이 있을 경우
                    if (FindNeighborPiece(check))
                        FindDirectionMatchedPiece(check, ref rows, ref cols);

                    // 상하인 경우 cols 증가
                    if (dir == direction[0] || dir == direction[2])
                        ++cols;

                    // 좌우인 경우 rows 증가
                    else if (dir == direction[1] || dir == direction[3])
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
            Piece right = GetPiece(piece.row + 1, piece.column);
            if (matchedPieces.Contains(right) && !verifiedPieces.Contains(right) &&  piece.value == right.value)
                return true;
        }

        if (piece.row - 1 > 0)
        {
            Piece left = GetPiece(piece.row - 1, piece.column);
            if (matchedPieces.Contains(left) && !verifiedPieces.Contains(left) && piece.value == left.value)
                return true;
        }

        if (piece.column + 1 < vertical - 1)
        {
            Piece up = GetPiece(piece.row, piece.column + 1);
            if (matchedPieces.Contains(up) && !verifiedPieces.Contains(up) && piece.value == up.value)
                return true;
        }

        if (piece.column - 1 > 0)
        {
            Piece down = GetPiece(piece.row, piece.column - 1);
            if (matchedPieces.Contains(down) && !verifiedPieces.Contains(down) && piece.value == down.value)
                return true;
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
            if (tiles[row, column] != null)
            {
                Piece rowPiece = GetPiece(row, column);

                if(rowPiece.columnBomb)
                {
                    GetColumnPieces(rowPiece.row);
                    rowPiece.columnBomb = false;
                }

                if(rowPiece.crossBomb)
                {
                    UsedCrossBomb(rowPiece);
                    rowPiece.crossBomb = false;
                }

                if (!matchedPieces.Contains(rowPiece))
                    matchedPieces.Add(rowPiece);

                if (!verifiedPieces.Contains(rowPiece))
                    verifiedPieces.Add(rowPiece);
            }
        }
    }

    private void GetColumnPieces(int row)
    {
        for (int column = 0; column < vertical; ++column)
        {
            if (tiles[row, column] != null)
            {
                Piece columnPiece = GetPiece(row, column);

                if (columnPiece.rowBomb)
                {
                    GetRowPieces(columnPiece.column);
                    columnPiece.rowBomb = false;
                }

                if (columnPiece.crossBomb)
                {
                    UsedCrossBomb(columnPiece);
                    columnPiece.crossBomb = false;
                }

                if (!matchedPieces.Contains(columnPiece))
                    matchedPieces.Add(columnPiece);

                if (!verifiedPieces.Contains(columnPiece))
                    verifiedPieces.Add(columnPiece);
            }
        }
    }

    IEnumerator DisabledMatchedPiece()
    {
        // 선택한 Piece가 있을 때, 처음으로 움직인 것을 안다. 
        if (selectedPiece != null)
        {
            stageCtrl.DecreaseMove(stageCtrl.decreaseMoveValue);
            selectedPiece = null;
        }

        // 매치된 Piece는 비활성화 시킨다.
        foreach (var piece in matchedPieces)
        {
            //piece.PieceEffectPlay(PieceEffect.PIECEEXPLOSION);
            NeighborBreakableTileDamage(piece.row, piece.column); // 인접한 BreakableTile에 데미지를 준다.
            piece.SetDisabledPiece();
            tiles[piece.row, piece.column].piece = null;
        }

        DamagedResetBreakableTile(); // 이번 턴에 데미지 받은 타일은 다시 리셋

       //stageCtrl.SoundEffectPlay(SoundEffectList.MATCHED);

        yield return new WaitForSeconds(effectDelayTime);

        ++stageCtrl.combo; // 콤보를 증가시킨다.

        for(int i = matchedPieces.Count - 1; i > -1; --i)
        { 
            stageCtrl.IncreaseScore(stageCtrl.matchedScore); // 점수만큼 증가

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
                if (IsBlankSpace(row, column) || IsBreakableTile(row, column))
                    continue;

                if (TileIsNullPiece(row, column)) // 자리에 피스가 없다.
                {
                    for (int i = column + 1; i < vertical; ++i) // 빈 자리 위로 피스가 있는지 확인한다.
                    {
                        if (IsBlankSpace(row, i) || IsBreakableTile(row, i))
                            continue;

                        if (!TileIsNullPiece(row, i)) // 한칸씩 위로 이동하며 피스가 있는지 확인한다.
                        {
                            Piece fallPiece = GetPiece(row, i); // 빈 자리 위에 있는 블럭

                            fallPiece.SetPiece(this, fallPiece.value, fallPiece.row, column);
                            SetTileInPiece(fallPiece.row, fallPiece.column, fallPiece);

                            tiles[row, i].piece = null; // 그 자리를 비운다.

                            fallPiece.currState = PieceState.MOVE;
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
                if (IsBlankSpace(row, column) || IsBreakableTile(row,column))
                    continue;

                if (TileIsNullPiece(row, column)) // 자리에 피스가 없다.
                {
                    Piece enabledPiece = disabledPieces.Dequeue();

                    enabledPiece.SetPiece(this, Random.Range(0, pieceSprites.Length), row, column);
                    SetTileInPiece(enabledPiece.row, enabledPiece.column, enabledPiece);

                    enabledPiece.transform.position = new Vector2(enabledPiece.row, vertical + fall);
                    enabledPiece.gameObject.SetActive(true);

                    enabledPiece.currState = PieceState.MOVE;
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

        FindMatchedPiece(true);

        if (isMatched)
            FindMatchedPiece();

        else
            currBoardState = BoardState.ORDER;
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

    private bool EqualPiece(Piece piece1, Piece piece2)
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
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (IsBlankSpace(row, column))
                    continue;

                if (!TileIsNullPiece(row, column))
                {
                    Piece movePiece = GetPiece(row, column);

                    if (movePiece.currState == PieceState.MOVE)
                        return true;
                }
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