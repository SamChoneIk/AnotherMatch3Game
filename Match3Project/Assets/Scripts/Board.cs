using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public enum BoardState
{
    ORDER,
    WORK,
    CLEAR,
    FAIL,
}

public class Board : MonoBehaviour
{
    [Header("Board State")]
    public BoardState currState = BoardState.WORK;

    public int width = 6;
    public int height = 8;
    public int offset = 5;

    public float waitTime = 0.08f;
    public float fallSpeed;

    public GameObject[,] boardIndex;
    public Piece selectPiece;

    [Header("Piece Parts")]
    public GameObject piecePrefab;
    public GameObject disabledPieces;

    public Sprite[] pieceSprites;
    public Sprite[] ItemSprites;

    public List<Piece> matchedPiece;
    public List<Piece> disabledPiece;
    public List<Piece> verifyPiece;
    public List<Piece> itemList; // debug

    private float checkTime;

    private void Start()
    {
        boardIndex = new GameObject[width, height];

        matchedPiece = new List<Piece>();
        disabledPiece = new List<Piece>();
        verifyPiece = new List<Piece>();
        itemList = new List<Piece>();

        pieceSprites = Resources.LoadAll<Sprite>("Arts/PieceSprite");
        ItemSprites = Resources.LoadAll<Sprite>("Arts/ItemSprite");

        CreatePiece();

    }

    private void CreatePiece()
    {
        for (int i = 0; i < width * height + Mathf.RoundToInt((width * height) * 0.5f); ++i)
        {
            GameObject pieceGo = Instantiate(piecePrefab, Vector2.zero, Quaternion.identity);
       
            Piece firstPiece = pieceGo.GetComponent<Piece>();

            firstPiece.name = "DefaultPiece";
            firstPiece.transform.parent = disabledPieces.transform;

            disabledPiece.Add(firstPiece);
            pieceGo.SetActive(false);
        }

        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] == null)
                {
                    Piece initPiece = disabledPiece[0];

                    initPiece.SettingPiece(this, "[" + row + " , " + column + "]", Random.Range(0, pieceSprites.Length), row, column);

                    initPiece.transform.parent = transform;
                    initPiece.transform.position = new Vector2(initPiece.row, initPiece.column + offset);

                    initPiece.gameObject.SetActive(true);
                    disabledPiece.RemoveAt(0);
                }
            }
        }

        StartCoroutine(GeneratePiece());
    }

    IEnumerator GeneratePiece(bool Regenerate = false)
    {
        if (Regenerate)
        {
            for (int column = 0; column < height; ++column)
            {
                for (int row = 0; row < width; ++row)
                {
                    if (boardIndex[row, column] != null)
                    {
                        Piece regenPiece = GetPiece(row, column);
                        regenPiece.SetPieceValue(Random.Range(0, pieceSprites.Length));
                        regenPiece.transform.position = new Vector2(regenPiece.row, regenPiece.column + offset);
                    }
                }
            }
        }

        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    Piece checkPiece = GetPiece(row, column);

                    while (IsMatchedPiece(checkPiece))
                        checkPiece.SetPieceValue(Random.Range(0, pieceSprites.Length));

                    checkPiece.currState = PieceState.MOVE;
                }
            }
        }

        while (FindMovingPiece())
            yield return null;

        yield return new WaitForSeconds(waitTime);

        currState = BoardState.ORDER;
    }

    private bool IsMatchedPiece(Piece piece)
    {
        if (piece.row < width - 1 && piece.row  > 0)
        {
            if (boardIndex[piece.row + 1, piece.column] != null && 
                boardIndex[piece.row - 1, piece.column] != null)
            {
                if (piece.value == GetPiece(piece.row + 1, piece.column).value && 
                    piece.value == GetPiece(piece.row - 1, piece.column).value)
                    return true;
            }
        }

        if (piece.column < height - 1 && piece.column > 0)
        {
            if (boardIndex[piece.row, piece.column + 1] != null && 
                boardIndex[piece.row, piece.column - 1] != null)
            {
                if (piece.value == GetPiece(piece.row, piece.column + 1).value && 
                    piece.value == GetPiece(piece.row, piece.column - 1).value)
                    return true;
            }
        }

        return false;
    }

    void Update()
    {
        if (currState != BoardState.CLEAR || currState != BoardState.FAIL)
        {
            switch (currState)
            {
                case BoardState.WORK:
                    {
                        checkTime = 0;

                        // 블럭이 움직이고 있을때
                        if (FindMovingPiece() || selectPiece == null)
                            return;

                        if (selectPiece.isTunning && selectPiece.target.isTunning)
                        {
                            selectPiece.isTunning = false;
                            selectPiece.target.isTunning = false;

                            StageManager.instance.SoundEffectPlay(1);

                            selectPiece.target = null;
                            selectPiece = null;

                            currState = BoardState.ORDER;
                            return;
                        }

                        if (selectPiece.crossBomb || selectPiece.target.crossBomb)
                        {
                            UsedCrossBomb(selectPiece);
                            UsedCrossBomb(selectPiece.target);
                        }

                        if (CurrentFindMatched() || matchedPiece.Count > 0)
                            FindMatchedPiece();

                        else
                        {
                            selectPiece.MoveToBack();
                            selectPiece.target.MoveToBack();
                        }

                        break;
                    }

                case BoardState.ORDER:
                    {
                        checkTime += Time.deltaTime;

                        if (checkTime > 3f)
                        {
                            if (DeadLockCheck())
                            {
                                selectPiece = null;

                                StartCoroutine(GeneratePiece(true));
                            }

                            else
                            {
                                Vector2 dir;
                                // hint effect
                                Piece piece = FindHintMatched(out dir);
                                Piece adjacency = GetPiece(piece.row + (int)dir.x, piece.column + (int)dir.y);

                                piece.PieceEffectPlay(4);
                                adjacency.PieceEffectPlay(4);
                                checkTime = 0f;
                            }
                        }

                        break;
                    }
            }
        }

        DebugSystem();
    }

    public void FindMatchedPiece()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    if (row > 0 && row < width - 1)
                    {
                        if (boardIndex[row + 1, column] != null && boardIndex[row - 1, column] != null)
                            AddMatchedPiece(GetPiece(row, column), GetPiece(row + 1, column), GetPiece(row - 1, column));
                    }

                    if (column > 0 && column < height - 1)
                    {
                        if (boardIndex[row, column + 1] != null && boardIndex[row, column - 1] != null)
                            AddMatchedPiece(GetPiece(row, column), GetPiece(row, column + 1), GetPiece(row, column - 1));
                    }
                }
            }
        }

        if (matchedPiece.Count > 0)
        {
            GenerateItemPiece();
            ++StageManager.instance.combo;
            StartCoroutine(DisabledMatchedPiece());
        }

        else
        {
            StageManager.instance.combo = 0;

            if (currState == BoardState.CLEAR || currState == BoardState.FAIL)
                return;

            currState = BoardState.ORDER;
        }
    }

    private void AddMatchedPiece(Piece currPiece, Piece sidePiece1, Piece sidePiece2)
    {
        if (currPiece.value == sidePiece1.value && currPiece.value == sidePiece2.value)
        {
            if (!matchedPiece.Contains(sidePiece1))
                matchedPiece.Add(sidePiece1);

            if (!matchedPiece.Contains(currPiece))
                matchedPiece.Add(currPiece);

            if (!matchedPiece.Contains(sidePiece2))
                matchedPiece.Add(sidePiece2);

            UsedItem(sidePiece1, currPiece, sidePiece2);
        }
    }

    private void GenerateItemPiece()
    {
        if (matchedPiece.Count < 3)
            return;

        int rows = 0;
        int cols = 0;
        int prevCount = 0;

        // Cross Bomb
        if (matchedPiece.Count > 4)
        {
            for (int i = 0; i < matchedPiece.Count - 1; ++i)
            {
                if (verifyPiece.Contains(matchedPiece[i]))
                    continue;

                verifyPiece.Add(matchedPiece[i]);

                FindDirectionMatchedPiece(matchedPiece[i], ref rows, ref cols);

               // Debug.Log("rows = " + rows + " & " + "columns = " + cols);

                if (rows >= 2 && cols >= 2)
                {
                    Piece bombPiece = matchedPiece[i];

                    if (selectPiece != null)
                    {
                        if (selectPiece.value == matchedPiece[i].value)
                            bombPiece = selectPiece;

                        if (selectPiece.target != null && selectPiece.target.value == matchedPiece[i].value)
                            bombPiece = selectPiece.target;
                    }

                    else
                        bombPiece = verifyPiece[Random.Range(prevCount, verifyPiece.Count)];

                    //Debug.Log("generate CrossBomb");

                    bombPiece.crossBomb = true;
                    bombPiece.value = pieceSprites.Length + 1;
                    bombPiece.itemSprite.sprite = ItemSprites[2];

                    itemList.Add(bombPiece);
                    matchedPiece.Remove(bombPiece);

                    prevCount = verifyPiece.Count - 1;
                }

                else
                {
                    if (verifyPiece.Count >= 3)
                        verifyPiece.RemoveRange(prevCount, (verifyPiece.Count - 1) - prevCount);

                    else
                        verifyPiece.Clear();
                }

                rows = 0;
                cols = 0;
            }
        }

        // Row Bomb
        for (int i = 0; i < matchedPiece.Count - 1; ++i)
        {
            if (verifyPiece.Contains(matchedPiece[i]))
                continue;

            for (int r = 1; r < width - 1; ++r)
            {
                if (matchedPiece[i].row + r > width - 1)
                    break;

                Piece check = GetPiece(matchedPiece[i].row + r, matchedPiece[i].column);

                if (matchedPiece.Contains(check) && matchedPiece[i].value == check.value)
                {
                    verifyPiece.Add(check);
                    rows++;
                }

                else
                    break;
            }

            if (rows >= 3)
            {
                Piece bombPiece = matchedPiece[i];

                if(selectPiece != null)
                {
                    if (selectPiece.value == matchedPiece[i].value)
                        bombPiece = selectPiece;

                    if (selectPiece.target != null && selectPiece.target.value == matchedPiece[i].value)
                        bombPiece = selectPiece.target;
                }
                
                else
                    bombPiece = verifyPiece[Random.Range(prevCount, verifyPiece.Count)];

                //Debug.Log("generate RowBomb");

                bombPiece.rowBomb = true;
                bombPiece.itemSprite.sprite = ItemSprites[1];

                itemList.Add(bombPiece);
                matchedPiece.Remove(bombPiece);

                prevCount = verifyPiece.Count - 1;
            }

            else
            {
                if (verifyPiece.Count >= 3)
                    verifyPiece.RemoveRange(prevCount, (verifyPiece.Count - 1) - prevCount);

                else
                    verifyPiece.Clear();
            }

            rows = 0;
        }

        // Column Bomb
        for (int i = 0; i < matchedPiece.Count - 1; ++i)
        {
            if (verifyPiece.Contains(matchedPiece[i]))
                continue;

            for (int c = 1; c < height - 1; ++c)
            {
                if (matchedPiece[i].column + c > height - 1)
                    break;

                Piece check = GetPiece(matchedPiece[i].row, matchedPiece[i].column + c);

                if (matchedPiece.Contains(check) && matchedPiece[i].value == check.value)
                {
                    verifyPiece.Add(check);
                    cols++;
                }

                else
                    break;
            }

            if (cols >= 3)
            {
                Piece bombPiece = matchedPiece[i];

                if (selectPiece != null)
                {
                    if (selectPiece.value == matchedPiece[i].value)
                        bombPiece = selectPiece;

                    if (selectPiece.target != null && selectPiece.target.value == matchedPiece[i].value)
                        bombPiece = selectPiece.target;
                }

                else
                    bombPiece = verifyPiece[Random.Range(prevCount, verifyPiece.Count)];

                //Debug.Log("generate ColumnBomb");

                bombPiece.columnBomb = true;
                bombPiece.itemSprite.sprite = ItemSprites[0];

                itemList.Add(bombPiece);
                matchedPiece.Remove(bombPiece);

                prevCount = verifyPiece.Count - 1;
            }

            else
            {
                if (verifyPiece.Count >= 3)
                    verifyPiece.RemoveRange(prevCount, verifyPiece.Count  - prevCount);

                else
                    verifyPiece.Clear();
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
                if (piece.row + ((int)dir.x * i) > width - 1 || piece.column + ((int)dir.y * i) > height - 1 ||
                    piece.row + ((int)dir.x * i) < 0 || piece.column + ((int)dir.y * i) < 0)
                    break;

                Piece check = GetPiece(piece.row + ((int)dir.x * i), piece.column + ((int)dir.y * i));

                // 매치된 블럭일 때
                if (matchedPiece.Contains(check) && !verifyPiece.Contains(check) &&
                  check.value == piece.value)
                {
                    // 체크가 끝난 블럭은 검사에서 제외
                    verifyPiece.Add(check);

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
        if (piece.row + 1 < width - 1)
        {
            Piece right = GetPiece(piece.row + 1, piece.column);
            if (matchedPiece.Contains(right) && !verifyPiece.Contains(right) && 
                piece.value == right.value)
                return true;
        }

        if (piece.row - 1 > 0)
        {
            Piece left = GetPiece(piece.row - 1, piece.column);
            if (matchedPiece.Contains(left) && !verifyPiece.Contains(left) && 
                piece.value == left.value)
                return true;
        }

        if (piece.column + 1 < height - 1)
        {
            Piece up = GetPiece(piece.row, piece.column + 1);
            if (matchedPiece.Contains(up) && !verifyPiece.Contains(up) && 
                piece.value == up.value)
                return true;
        }

        if (piece.column - 1 > 0)
        {
            Piece down = GetPiece(piece.row, piece.column - 1);
            if (matchedPiece.Contains(down) && !verifyPiece.Contains(down) &&
                piece.value == down.value)
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
            piece1.PieceEffectPlay(3);
        }
        if (piece2.rowBomb)
        {
            piece2.rowBomb = false;
            GetRowPieces(piece2.column);
            piece2.PieceEffectPlay(3);
        }
        if (piece3.rowBomb)
        {
            piece3.rowBomb = false;
            GetRowPieces(piece3.column);
            piece3.PieceEffectPlay(3);
        }

        if (piece1.columnBomb)
        {
            piece1.columnBomb = false;
            GetColumnPieces(piece1.row);
            piece1.PieceEffectPlay(1);
        }
        if (piece2.columnBomb)
        {
            piece2.columnBomb = false;
            GetColumnPieces(piece2.row);
            piece2.PieceEffectPlay(1);
        }
        if (piece3.columnBomb)
        {
            piece3.columnBomb = false;
            GetColumnPieces(piece3.row);
            piece3.PieceEffectPlay(1);
        }
    }

    public void UsedCrossBomb(Piece piece)
    {
        if (piece.crossBomb)
        {
            piece.PieceEffectPlay(2);
            piece.crossBomb = false;
            GetRowPieces(piece.column);
            GetColumnPieces(piece.row);
        }
    }

    private void GetRowPieces(int column)
    {
        for (int row = 0; row < width; ++row)
        {
            if (boardIndex[row, column] != null)
            {
                Piece piece = GetPiece(row, column);

                if(piece.columnBomb)
                {
                    GetColumnPieces(piece.row);
                    piece.columnBomb = false;
                }

                if(piece.crossBomb)
                {
                    UsedCrossBomb(piece);
                    piece.crossBomb = false;
                }

                if (!matchedPiece.Contains(piece))
                    matchedPiece.Add(piece);

                if (!verifyPiece.Contains(piece))
                    verifyPiece.Add(piece);
            }
        }
    }

    private void GetColumnPieces(int row)
    {
        for (int column = 0; column < height; ++column)
        {
            if (boardIndex[row, column] != null)
            {
                Piece piece = GetPiece(row, column);

                if (piece.rowBomb)
                {
                    GetRowPieces(piece.column);
                    piece.rowBomb = false;
                }

                if (piece.crossBomb)
                {
                    UsedCrossBomb(piece);
                    piece.crossBomb = false;
                }

                if (!matchedPiece.Contains(piece))
                    matchedPiece.Add(piece);

                if (!verifyPiece.Contains(piece))
                    verifyPiece.Add(piece);
            }
        }
    }

    IEnumerator DisabledMatchedPiece()
    {
        if (selectPiece != null)
        {
            StageManager.instance.DecreaseMove(1);
            selectPiece = null;
        }

        foreach (var piece in matchedPiece)
        {
            piece.AllClearPiece();

            // 아이템 이펙트가 재생중인지 검사
            if (piece.IsEffectPlaying())
                piece.PieceEffectPlay(0);
        }

        StageManager.instance.SoundEffectPlay(2);

        yield return new WaitForSeconds(0.5f);

        foreach (var piece in matchedPiece)
        {
            piece.SetDisabledPiece();
            disabledPiece.Add(piece);
            StageManager.instance.IncreaseScore(30);
        }

        matchedPiece.Clear();
        verifyPiece.Clear();

        //yield return new WaitForSeconds(waitTime);

        StartCoroutine(FallPieces());
    }

    IEnumerator FallPieces()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] == null)
                {
                    for (int i = column + 1; i < height; ++i)
                    {
                        if (boardIndex[row, i] != null)
                        {
                            Piece fallPiece = GetPiece(row, i); // 빈 자리의 위에 있는 블럭

                            fallPiece.SettingPiece(this, "[" + fallPiece.row + " , " + fallPiece.column + "]",fallPiece.value, fallPiece. row, column);

                            boardIndex[row, i] = null;

                            fallPiece.currState = PieceState.MOVE;
                            break;
                        }
                    }
                }
            }
        }

        int fall = 0;

        for (int row = 0; row < width; ++row)
        {
            for (int column = 0; column < height; ++column)
            {
                if (boardIndex[row, column] == null)
                {
                    Piece enabledPiece = disabledPiece[0];

                    enabledPiece.SettingPiece(this, "[" + row + " , " + column + "]", Random.Range(0, pieceSprites.Length), row, column);

                    enabledPiece.transform.parent = transform;
                    enabledPiece.transform.position = new Vector2(enabledPiece.row, height + fall);

                    enabledPiece.gameObject.SetActive(true);

                    enabledPiece.currState = PieceState.MOVE;

                    disabledPiece.Remove(enabledPiece);
                    ++fall;
                }
            }

            fall = 0;
        }

        while (FindMovingPiece())
            yield return null;

        yield return new WaitForSeconds(waitTime);

        FindMatchedPiece();
    }

    public Piece GetPiece(int row, int column)
    {
        return boardIndex[row, column].GetComponent<Piece>();
    }

    private bool DeadLockCheck()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (row < width - 1)
                {
                    SwapBoardIndex(row, column, Vector2.right);

                    if (CurrentFindMatched())
                    {
                        SwapBoardIndex(row, column, Vector2.right);
                        return false;
                    }

                    SwapBoardIndex(row, column, Vector2.right);
                }

                if (column < height - 1)
                {
                    SwapBoardIndex(row, column, Vector2.up);

                    if (CurrentFindMatched())
                    {
                        SwapBoardIndex(row, column, Vector2.up);
                        return false;
                    }

                    SwapBoardIndex(row, column, Vector2.up);
                }
            }
        }

        return true;
    }

    private void SwapBoardIndex(int row, int column, Vector2 dir)
    {
        GameObject temp = boardIndex[row + (int)dir.x, column + (int)dir.y];
        boardIndex[row + (int)dir.x, column + (int)dir.y] = boardIndex[row, column];
        boardIndex[row, column] = temp;
    }

    private bool CurrentFindMatched()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    if (row > 0 && row < width - 1)
                    {
                        if (boardIndex[row + 1, column] != null && 
                            boardIndex[row - 1, column] != null)
                        {
                            if (GetPiece(row, column).value == GetPiece(row + 1, column).value &&
                                GetPiece(row, column).value == GetPiece(row - 1, column).value)
                                return true;
                        }
                    }

                    if (column > 0 && column < height - 1)
                    {
                        if (boardIndex[row, column + 1] != null && 
                            boardIndex[row, column - 1] != null)
                        {
                            if (GetPiece(row, column).value == GetPiece(row, column + 1).value && 
                                GetPiece(row, column).value == GetPiece(row, column - 1).value)
                                return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private Piece FindHintMatched(out Vector2 dir)
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (row < width - 1)
                {
                    SwapBoardIndex(row, column, Vector2.right);

                    if (CurrentFindMatched())
                    {
                        SwapBoardIndex(row, column, Vector2.right);
                        dir = Vector2.right;
                        return GetPiece(row, column);
                    }

                    SwapBoardIndex(row, column, Vector2.right);
                }

                if (column < height - 1)
                {
                    SwapBoardIndex(row, column, Vector2.up);

                    if (CurrentFindMatched())
                    {
                        SwapBoardIndex(row, column, Vector2.up);
                        dir = Vector2.up;
                        return GetPiece(row, column);
                    }

                    SwapBoardIndex(row, column, Vector2.up);
                }
            }
        }

        dir = Vector2.zero;
        return null;
    }

    public bool FindMovingPiece()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    Piece currPiece = GetPiece(row, column);

                    // Piece Moving Check
                        if (currPiece.currState == PieceState.MOVE)
                            return true;
                }
            }
        }
            return false;
    }

    public void AllStopEffect()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                Piece piece = GetPiece(row, column);

                foreach (var e in piece.pieceEffects)
                {
                    e.Stop();
                }
            }
        }
    }

    private void DebugSystem()
    {
        // debug Array Check
        if (Input.GetKeyDown(KeyCode.A))
            DebugBoardChecking(false);

        // debug Piece Moving Check
        if (Input.GetKeyDown(KeyCode.B))
            DebugBoardChecking(true);

        // debug Board Current State Check
        if (Input.GetKeyDown(KeyCode.D))
            Debug.Log(currState);

        // debug Deadlock Check
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (DeadLockCheck())
                Debug.Log("is DeadLock !!");
        }

        // debug Item Check
        if (Input.GetKeyDown(KeyCode.G))
        {
            DebugBoardItemChecking();
        }

        // debug Board Reset
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            selectPiece = null;

            StartCoroutine(GeneratePiece(true));
        }

        // debug Generate Item
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selectPiece != null)
            {
                selectPiece.itemSprite.sprite = ItemSprites[0];
                selectPiece.rowBomb = true;
            }
        }

        // debug Generate Item
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (selectPiece != null)
            {
                selectPiece.itemSprite.sprite = ItemSprites[1];
                selectPiece.columnBomb = true;
            }
        }

        // debug Generate Item
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (selectPiece != null)
            {
                selectPiece.itemSprite.sprite = ItemSprites[2];
                selectPiece.crossBomb = true;
            }
        }
    }

    private void DebugBoardChecking(bool moving = false)
    {
        StringBuilder sb = new StringBuilder(1000);

        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
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
                            Debug.Log("[" + row + " , " + column + "] is Moving");
                    }
                }
            }
        }

        Debug.Log(sb.ToString());
        //sb.Clear();
    }

    private void DebugBoardItemChecking()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    Piece piece = GetPiece(row, column);

                    if (piece.rowBomb || piece.columnBomb || piece.crossBomb)
                        Debug.Log("is itemPiece");
                }
            }
        }
    }

    public void PieceListSort(List<Piece> pieces, bool column = false, bool descending = false)
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
    }
}