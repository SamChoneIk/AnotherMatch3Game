using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

public enum BoardState
{
    ORDER,
    WORK,
}

public class BoardManager : MonoBehaviour
{
    [Header("Board State")]
    public BoardState currState = BoardState.WORK;
    public int width, height, offset;

    public float waitTime = 0.08f;
    public float blockDuration;

    public GameObject[,] boardIndex;
    public GameObject disabledPieces;

    [Header("Piece Parts")]
    public GameObject piecePrefab;

    public Block selectPiece;
    public Block targetPiece;

    public List<Block> matchedPiece;
    public List<Block> disabledPiece;
    public List<Block> itemPiece;

    public Sprite[] pieceSprites;
    public Sprite[] ItemSprites;

    private float checkTime;

    void Start()
    {
        boardIndex = new GameObject[width, height];

        matchedPiece = new List<Block>();
        disabledPiece = new List<Block>();

        CreateBoard();
        //FindMatchedIndex();
    }

    private void DebugSystem()
    {
        // debug Array Check
        if (Input.GetKeyDown(KeyCode.LeftAlt))
            DebugBoardChecking(false);

        // debug Piece Moving Check
        if (Input.GetMouseButtonDown(2))
            DebugBoardChecking(true);

        // debug Board Current State Check
        if (Input.GetKeyDown(KeyCode.LeftShift))
            Debug.Log(currState);

        // debug Deadlock Check
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (DeadLockCheck())
            {
                selectPiece = null;
                targetPiece = null;

                Debug.Log("is DeadLock !!");

                PieceRegenerate();
            }

            else
                Debug.Log(DeadLockCheck());
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
            PieceRegenerate();

        // debug Generate Item
        if (Input.GetMouseButtonDown(1))
        {
            if (selectPiece != null)
            {
                selectPiece.itemSprite.sprite = ItemSprites[2];
                selectPiece.crossBomb = true;
            }
        }
    }

    void Update()
    {
        if (currState == BoardState.ORDER)
        {
            checkTime += Time.deltaTime;

            if (checkTime > 5f)
            {
                if (DeadLockCheck())
                {
                    PieceRegenerate();
                    Debug.Log("is DeadLock !!");
                }

                else
                {
                    // hint effect
                    checkTime = 0;
                }
            }
        }

        if (currState == BoardState.WORK)
        {
            checkTime = 0;

            if (selectPiece != null && targetPiece != null)
            {
                if (!IndexCheck(true))
                    return;

                else
                {
                    if (selectPiece.isTunning && targetPiece.isTunning)
                    {
                        selectPiece.isTunning = false;
                        targetPiece.isTunning = false;

                        selectPiece = null;
                        targetPiece = null;

                        currState = BoardState.ORDER;

                        return;
                    }

                    if (IndexCheck())
                    {
                        selectPiece = null;
                        targetPiece = null;

                        FindMatchedIndex();
                    }

                    else
                    {
                        selectPiece.row = selectPiece.prevRow;
                        selectPiece.column = selectPiece.prevColumn;

                        targetPiece.row = targetPiece.prevRow;
                        targetPiece.column = targetPiece.prevColumn;

                        selectPiece.moveToPos = new Vector2(selectPiece.prevRow, selectPiece.prevColumn);
                        targetPiece.moveToPos = new Vector2(targetPiece.prevRow, targetPiece.prevColumn);

                        selectPiece.isTunning = true;
                        targetPiece.isTunning = true;

                        selectPiece.currState = BlockState.MOVE;
                        targetPiece.currState = BlockState.MOVE;
                    }
                }
            }
        }

        DebugSystem();
    }

    private void CreateBoard()
    {
        List<Block> initPieces = new List<Block>();

        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                int value = Random.Range(0, pieceSprites.Length);

                GameObject pieceGo = Instantiate(piecePrefab, new Vector2(row, column + offset), Quaternion.identity);
                Block piece = pieceGo.GetComponent<Block>();
                piece.InitPiece(value, row, column, this);

                pieceGo.transform.parent = transform;
                pieceGo.name = "[" + row + " , " + column + "]";

                boardIndex[piece.row, piece.column] = pieceGo;

                while (IndexCheck())
                {
                    value = Random.Range(0, pieceSprites.Length);
                    piece.value = value;
                }

                piece.InitPiece(value, piece.row, piece.column, this);
                piece.moveToPos = new Vector2(piece.row, piece.column);

                initPieces.Add(piece);
            }
        }

        for (int i = 0; i < Mathf.RoundToInt(height * width / 2); ++i)
        {
            GameObject pieceGo = Instantiate(piecePrefab, Vector2.zero, Quaternion.identity);
            Block piece = pieceGo.GetComponent<Block>();

            piece.InitPiece(0, 0, 0, this);

            pieceGo.transform.parent = disabledPieces.transform;
            pieceGo.name = "DefaultPiece";

            pieceGo.SetActive(false);
            disabledPiece.Add(piece);
        }

        foreach (var piece in initPieces)
        {
            piece.currState = BlockState.MOVE;
        }

        initPieces.Clear();

        currState = BoardState.ORDER;
    }

    public void FindMatchedIndex()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    Block currPiece = GetPiece(row, column);

                    if (currPiece != null)
                    {
                        if (row > 0 && row < width - 1)
                        {
                            if (boardIndex[row + 1, column] != null && boardIndex[row - 1, column] != null)
                            {
                                Block rightPiece = GetPiece(row + 1, column);
                                Block leftPiece = GetPiece(row - 1, column);

                                if (currPiece.value == rightPiece.value && currPiece.value == leftPiece.value)
                                {
                                    if (!matchedPiece.Contains(leftPiece))
                                        matchedPiece.Add(leftPiece);

                                    if (!matchedPiece.Contains(currPiece))
                                        matchedPiece.Add(currPiece);

                                    if (!matchedPiece.Contains(rightPiece))
                                        matchedPiece.Add(rightPiece);

                                    //Debug.Log("prev matched Count = " + matchedPiece.Count);

                                    ItemPieces(leftPiece);
                                    ItemPieces(currPiece);
                                    ItemPieces(rightPiece);

                                    //Debug.Log("curr matched Count = " + matchedPiece.Count);
                                }

                                if (currPiece.crossBomb || rightPiece.crossBomb || leftPiece.crossBomb)
                                {
                                    if (currPiece.crossBomb)
                                        CrossBomb(currPiece);
                                    if (rightPiece.crossBomb)
                                        CrossBomb(rightPiece);
                                    if (leftPiece.crossBomb)
                                        CrossBomb(leftPiece);
                                }
                            }
                        }

                        if (column > 0 && column < height - 1)
                        {
                            if (boardIndex[row, column + 1] != null && boardIndex[row, column - 1] != null)
                            {
                                Block upPiece = GetPiece(row, column + 1);
                                Block downPiece = GetPiece(row, column - 1);

                                if (currPiece.value == upPiece.value && currPiece.value == downPiece.value)
                                {
                                    // matchedPiece.Union(RowBombPieces(upPiece, currPiece, downPiece));
                                    // matchedPiece.Union(ColumnBombPieces(upPiece, currPiece, downPiece));
                                    // matchedPiece.Union(CrossBombPieces(upPiece, currPiece, downPiece));

                                    if (!matchedPiece.Contains(downPiece))
                                        matchedPiece.Add(downPiece);

                                    if (!matchedPiece.Contains(currPiece))
                                        matchedPiece.Add(currPiece);

                                    if (!matchedPiece.Contains(upPiece))
                                        matchedPiece.Add(upPiece);

                                    ItemPieces(downPiece);
                                    ItemPieces(currPiece);
                                    ItemPieces(upPiece);
                                }

                                if (currPiece.crossBomb || upPiece.crossBomb || downPiece.crossBomb)
                                {
                                    if (currPiece.crossBomb)
                                        CrossBomb(currPiece);
                                    if (upPiece.crossBomb)
                                        CrossBomb(upPiece);
                                    if (downPiece.crossBomb)
                                        CrossBomb(downPiece);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (matchedPiece.Count > 0)
        {
            Checkingboard();
            //CheckingBomb();
            MatchedPieceDisabled();
        }

        else
            currState = BoardState.ORDER;
    }

    private void Checkingboard()
    {
        if (matchedPiece.Count < 3)
            return;

        List<Block> checkpiece = new List<Block>();
        int pieceCount = 0;
        int rows = 0;

        for(int piece = 0; piece < matchedPiece.Count -1; ++piece)
        {
            if (checkpiece.Contains(matchedPiece[piece]))
                continue;

            for (int i = 1; ; ++i)
            {
                if (matchedPiece[piece].row + i > width - 1)
                    break;

                Block check = GetPiece(matchedPiece[piece].row + i, matchedPiece[piece].column);

                if (checkpiece.Contains(check))
                    continue;

                if (matchedPiece.Contains(check) && matchedPiece[piece].row + i == check.row)
                {
                    ++rows;
                    checkpiece.Add(check);
                }

                else
                    break;

                Debug.Log("currCount = " + checkpiece.Count);
            }

            if (checkpiece.Count > 2)
            {
                Debug.Log("generate row");
                matchedPiece[piece].rowBomb = true;
                matchedPiece[piece].itemSprite.sprite = ItemSprites[0];
                matchedPiece.Remove(matchedPiece[piece + 1]);
                pieceCount = checkpiece.Count;
            }

            else
            {
                if (pieceCount != 0)
                    checkpiece.RemoveRange(pieceCount - 1, checkpiece.Count - pieceCount);

                Debug.Log("remove after Count = " + checkpiece.Count);
            }
        }

       /* for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                Block piece = 

            }
        }*/
    }

    private void CheckingBomb()
    {
        if (matchedPiece.Count <= 3)
            return;

        PieceListSort(matchedPiece);
        int rows = 0;
        int columns = 0;

        // Cross check
        /*if (matchedPiece.Count > 4)
        {
            for(int i = 0; i < matchedPiece.Count -1; ++i)
            {
                rows = 0;
                columns = 0;

                if (matchedPiece[i].crossBomb || matchedPiece[i].rowBomb || matchedPiece[i].columnBomb || itemPiece.Contains(matchedPiece[i]))
                    continue;

                foreach (var check in matchedPiece)
                {
                    if (matchedPiece[i] == check || check.crossBomb || check.rowBomb || check.columnBomb ||itemPiece.Contains(check))
                        continue;

                    if (matchedPiece[i].row == check.row)
                        ++columns;

                    else if (matchedPiece[i].column == check.column)
                        ++rows;
                }

               // Debug.Log("column = " + columns + " & row = " + rows);

                if (rows >= 2 && columns >= 2)
                {

                    Debug.Log(matchedPiece[i].name + "Generate Cross Bomb");
                    matchedPiece[i].crossBomb = true;
                    matchedPiece[i].itemSprite.sprite = ItemSprites[2];
                    matchedPiece.Remove(matchedPiece[i]);
                }
            }
        }*/

        // Row check
        for (int i = 0; i < matchedPiece.Count - 1; ++i)
        {
            rows = 0;
            
            if (matchedPiece[i].crossBomb || matchedPiece[i].rowBomb || matchedPiece[i].columnBomb)
                continue;

            foreach (var check in matchedPiece)
            {
                if (matchedPiece[i] == check)
                    continue;

                if (check.crossBomb || check.rowBomb || check.columnBomb)
                    break;

                if (matchedPiece[i].column == check.column)
                {
                    ++rows;
                }
            }

            Debug.Log("row = " + rows);

            if (rows >= 3)
            {
                Debug.Log(matchedPiece[i].name + "Generate Row Bomb");
                matchedPiece[i].rowBomb = true;
                matchedPiece[i].itemSprite.sprite = ItemSprites[0];
                matchedPiece.Remove(matchedPiece[i]);
            }

        }

        // Column check
        /* for (int i = 0; i < matchedPiece.Count - 1; ++i)
         {
             columns = 0;

             if (matchedPiece[i].crossBomb || matchedPiece[i].rowBomb || matchedPiece[i].columnBomb || itemPiece.Contains(matchedPiece[i]))
                 continue;

             foreach (var check in matchedPiece)
             {
                 if (matchedPiece[i] == check || check.crossBomb || check.rowBomb || check.columnBomb || itemPiece.Contains(check))
                     continue;

                 if (matchedPiece[i].row == check.row)
                     ++columns;
             }

             Debug.Log("column = " + columns);

             if (columns >= 3)
             {
                 Debug.Log(matchedPiece[i].name + "Generate Column Bomb");
                 matchedPiece[i].columnBomb = true;
                 matchedPiece[i].itemSprite.sprite = ItemSprites[1];
                 matchedPiece.Remove(matchedPiece[i]);
             }
         }*/
    }

    private void ItemPieces(Block piece)
    {
        if (piece.rowBomb)
            GetRowPieces(piece.column);

        if (piece.columnBomb)
            GetColumnPieces(piece.row);
    }

    private void CrossBomb(Block piece)
    {
        if (piece.crossBomb)
        {
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
                itemPiece.Add(GetPiece(row, column));

                if (!matchedPiece.Contains(GetPiece(row, column)))
                    matchedPiece.Add(GetPiece(row, column));
            }
        }
    }

    private void GetColumnPieces(int row)
    {
        for (int column = 0; column < width; ++column)
        {
            if (boardIndex[row, column] != null)
            {
                itemPiece.Add(GetPiece(row, column));

                if (!matchedPiece.Contains(GetPiece(row, column)))
                    matchedPiece.Add(GetPiece(row, column));
            }
        }
    }

    private void MatchedPieceDisabled()
    {
        foreach (var piece in matchedPiece)
        {
            piece.AllClearPiece();
            disabledPiece.Add(piece);
        }

        matchedPiece.Clear();
        itemPiece.Clear();

        selectPiece = null;
        targetPiece = null;

        StartCoroutine(FallPieces());
    }

    IEnumerator FallPieces()
    {
        List<Block> fallPieces = new List<Block>();

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
                            Block fallPiece = GetPiece(row, i); // 빈자리의 위에 있는 피스

                            fallPiece.InitPiece(fallPiece.value, fallPiece.row, column, this);
                            fallPiece.moveToPos = new Vector2(fallPiece.row, fallPiece.column);
                            fallPiece.name = "[" + row + " , " + column + "]";
                            boardIndex[fallPiece.row, fallPiece.column] = fallPiece.gameObject;

                            boardIndex[row, i] = null;
                            fallPiece.currState = BlockState.MOVE;
                            fallPieces.Add(fallPiece);

                            break;
                        }
                    }
                }
            }
        }

        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] == null)
                {
                    Block enabledPiece = EnabledPiece(row, column);
                    enabledPiece.moveToPos = new Vector2(enabledPiece.row, enabledPiece.column);
                    boardIndex[enabledPiece.row, enabledPiece.column] = enabledPiece.gameObject;

                    enabledPiece.currState = BlockState.MOVE;
                    fallPieces.Add(enabledPiece);
                }
            }
        }

        while (!IndexCheck(true))
        {
            yield return null;
        }
        yield return new WaitForSeconds(waitTime);

        fallPieces.Clear();

        FindMatchedIndex();
    }

    private Block EnabledPiece(int row, int column)
    {
        Block enabledPiece = disabledPiece[0];

        enabledPiece.InitPiece(Random.Range(0, pieceSprites.Length), row, column, this);
        enabledPiece.transform.position = new Vector2(row, column + offset);
        enabledPiece.transform.parent = transform;
        enabledPiece.gameObject.SetActive(true);

        disabledPiece.RemoveAt(0);

        return enabledPiece;
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

                    if (IndexCheck())
                    {
                        SwapBoardIndex(row, column, Vector2.right);
                        return false;
                    }

                    SwapBoardIndex(row, column, Vector2.right);
                }

                if (column < height - 1)
                {
                    SwapBoardIndex(row, column, Vector2.up);

                    if (IndexCheck())
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

    private void PieceRegenerate()
    {
        List<Block> refillPieces = new List<Block>();

        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    int value = Random.Range(0, pieceSprites.Length);

                    Block piece = GetPiece(row, column);
                    piece.gameObject.SetActive(false);

                    piece.InitPiece(value, piece.row, piece.column, this);

                    while (IndexCheck())
                    {
                        value = Random.Range(0, pieceSprites.Length);
                        piece.value = value;
                    }

                    piece.InitPiece(value, piece.row, piece.column, this);

                    piece.transform.position = new Vector2(piece.row, piece.column + offset);
                    piece.moveToPos = new Vector2(piece.row, piece.column);

                    refillPieces.Add(piece);
                }
            }
        }

        foreach (var pieces in refillPieces)
        {
            pieces.gameObject.SetActive(true);
            pieces.currState = BlockState.MOVE;
        }

        refillPieces.Clear();

        FindMatchedIndex();
    }

    private bool IndexCheck(bool moveCheck = false)
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    Block currPiece = GetPiece(row, column);

                    // Piece Moving Check
                    if (moveCheck)
                    {
                        if (currPiece.currState == BlockState.MOVE)
                            return false;
                    }

                    // Piece Color Value Check
                    else
                    {
                        if (currPiece != null)
                        {
                            if (row > 0 && row < width - 1)
                            {
                                if (boardIndex[row + 1, column] != null && boardIndex[row - 1, column] != null)
                                {
                                    Block rightPiece = GetPiece(row + 1, column);
                                    Block leftPiece = GetPiece(row - 1, column);

                                    if (currPiece.crossBomb || rightPiece.crossBomb || leftPiece.crossBomb)
                                        return true;

                                    if (currPiece.value == rightPiece.value && currPiece.value == leftPiece.value)
                                        return true;
                                }
                            }

                            if (column > 0 && column < height - 1)
                            {
                                if (boardIndex[row, column + 1] != null && boardIndex[row, column - 1] != null)
                                {
                                    Block upPiece = GetPiece(row, column + 1);
                                    Block downPiece = GetPiece(row, column - 1);

                                    if (currPiece.crossBomb || upPiece.crossBomb || downPiece.crossBomb)
                                        return true;

                                    if (currPiece.value == upPiece.value && currPiece.value == downPiece.value)
                                        return true;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (moveCheck)
            return true;

        else
            return false;
    }

    public Block GetPiece(int row, int column)
    {
        return boardIndex[row, column].GetComponent<Block>();
    }

    public void PieceListSort(List<Block> pieces, bool column = false)
    {
        if (column)
            pieces.Sort(delegate (Block a, Block b)
            {
                if (a.column > b.column)
                    return 1;

                else if (a.column < b.column)
                    return -1;

                else
                    return 0;
            });

        else
            pieces.Sort(delegate (Block a, Block b)
            {
                if (a.row > b.row)
                    return 1;

                else if (a.row < b.row)
                    return -1;

                else
                    return 0;
            });
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
                    Block piece = GetPiece(row, column);
                    /*if (boardIndex[row, column].GetComponent<Block>().value == GetPiece(row, column).value)
                        Debug.Log("[" + row + " , " + column + "] = " + GetPiece(row, column).value);*/

                    if (!moving)
                    {
                        sb.Append($"[{row} , {column}] = {piece.value} || PosX : {piece.transform.position.x} , PosY :  {piece.transform.position.y} \n");
                        //Debug.Log("[" + row + " , " + column + "] = " + piece.value + ", PosX : " + piece.transform.position.x + ", PosY : " + piece.transform.position.y);
                    }

                    else
                    {
                        if (piece.currState == BlockState.MOVE)
                            Debug.Log("[" + row + " , " + column + "] is Moving");
                    }
                }
            }
        }

        Debug.Log(sb.ToString());
       // sb.Clear();
    }
}