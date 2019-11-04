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

    public List<Block> matchedPiece;
    public List<Block> disabledPiece;
    public List<Block> checkList;

    public Sprite[] pieceSprites;
    public Sprite[] ItemSprites;

    private float checkTime;

    void Start()
    {
        boardIndex = new GameObject[width, height];

        matchedPiece = new List<Block>();
        disabledPiece = new List<Block>();
        checkList = new List<Block>();

        CreateBoard();
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
        if (currState == BoardState.WORK)
        {
            checkTime = 0;

            if (selectPiece != null)
            {
                if (!FIndMovingPiece())
                    return;

                else
                {
                    if (FindMatchedPiece())
                    {
                        selectPiece = null;
                        FindMatchedIndex();
                    }

                    else
                    {
                        selectPiece.target.isTunning = true;
                        selectPiece.isTunning = true;

                        selectPiece.target.currState = BlockState.MOVE;
                        selectPiece.currState = BlockState.MOVE;
                    }
                }
            }

            else
            {
                if (!FIndMovingPiece())
                    return;

                currState = BoardState.ORDER;
            }
        }

        else if (currState == BoardState.ORDER)
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

        DebugSystem();
    }

    private void CreateBoard()
    {
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

                while (FindMatchedPiece())
                {
                    value = Random.Range(0, pieceSprites.Length);
                    piece.value = value;
                }

                piece.InitPiece(value, piece.row, piece.column, this);
                piece.moveToPos = new Vector2(piece.row, piece.column);

                piece.currState = BlockState.MOVE;
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
                                    if (!matchedPiece.Contains(downPiece))
                                        matchedPiece.Add(downPiece);

                                    if (!matchedPiece.Contains(currPiece))
                                        matchedPiece.Add(currPiece);

                                    if (!matchedPiece.Contains(upPiece))
                                        matchedPiece.Add(upPiece);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (matchedPiece.Count > 0)
        {
            //Checkingboard();
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

        if (matchedPiece.Count > 4)
            AdjacentBombCheck(ref checkList);

        // Row check
        for (int i = 0; i < matchedPiece.Count - 1; ++i)
        {
            int rows = 0;

            if (matchedPiece[i].crossBomb || matchedPiece[i].rowBomb || matchedPiece[i].columnBomb)
                continue;

            foreach (var check in matchedPiece)
            {
                if (matchedPiece[i] == check)
                    continue;

                if (check.crossBomb || check.rowBomb || check.columnBomb)
                    break;

                if (matchedPiece[i].column == check.column)
                    ++rows;

            }

            if (rows >= 3)
            {
                Debug.Log(matchedPiece[i].name + "Generate Row Bomb");
                matchedPiece[i].rowBomb = true;
                matchedPiece[i].itemSprite.sprite = ItemSprites[0];
                matchedPiece.Remove(matchedPiece[i]);
            }

        }
    }

    private void AdjacentBombCheck(ref List<Block> checkList)
    {
        int rows = 0;
        int cols = 0;
        int prevCount = 0;

        for (int i = 0; i < matchedPiece.Count - 1; ++i)
        {
            if (checkList.Contains(matchedPiece[i]))
                continue;

            DirCheck(matchedPiece[i], ref rows, ref cols, ref checkList);

            if (rows >= 2 && cols >= 2)
            {
                Block idx = checkList[Mathf.RoundToInt((checkList.Count - prevCount) / 2)];
                var bombPiece = checkList.Find(p => p == idx);
                Debug.Log("generate CrossBomb");

                bombPiece.crossBomb = true;
                bombPiece.itemSprite.sprite = ItemSprites[2];
                matchedPiece.Remove(bombPiece);

                prevCount = checkList.Count - 1;
            }

            else
            {
                if (checkList.Count > 1)
                    checkList.RemoveRange(prevCount, (checkList.Count - 1) - prevCount);

                else
                    checkList.Clear();
            }

            rows = 0;
            cols = 0;
        }
    }

    private void DirCheck(Block piece, ref int rows, ref int cols, ref List<Block> checkList)
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
                    piece.row + ((int)dir.x * i) < 0         || piece.column + ((int)dir.y * i) < 0)
                    break;

                Block check = GetPiece(piece.row + ((int)dir.x * i), piece.column + ((int)dir.y * i));

                // 매치된 블럭일 때
                if (matchedPiece.Contains(check) && !checkList.Contains(check))
                {
                    checkList.Add(check);

                    // 검사하는 블럭에 상하좌우에 다른 매치된 블럭이 있을 경우
                    if (FindNeighborPiece(check, checkList))
                        DirCheck(check, ref rows, ref cols, ref checkList);

                    // 상하인 경우 cols 증가
                    if (dir == direction[0] || dir == direction[2])
                        ++cols; // 체크가 끝난 블럭은 검사에서 제외

                    // 좌우인 경우 rows 증가
                    else if (dir == direction[1] || dir == direction[3])
                        ++rows;  // 체크가 끝난 블럭은 검사에서 제외
                }

                // 블럭이 없으면 즉시 취소
                else
                    break;
            }
        }
    }

    private bool FindNeighborPiece(Block piece, List<Block> checkList)
    {
        if (piece.row + 1 < width - 1)
        {
            Block right = GetPiece(piece.row + 1, piece.column);
            if (matchedPiece.Contains(right) && !checkList.Contains(right))
                return true;
        }

        if (piece.row - 1 > 0)
        {
            Block left = GetPiece(piece.row - 1, piece.column);
            if (matchedPiece.Contains(left) && !checkList.Contains(left))
                return true;
        }

        if (piece.column + 1 < height - 1)
        {
            Block up = GetPiece(piece.row, piece.column + 1);
            if (matchedPiece.Contains(up) && !checkList.Contains(up))
                return true;
        }

        if (piece.column - 1 > 0)
        {
            Block down = GetPiece(piece.row, piece.column - 1);
            if (matchedPiece.Contains(down) && !checkList.Contains(down))
                return true;
        }

            return false;
    }

    private int isCenterPiece(Block piece)
    {
        int count = 0;

        Vector2[] direction =
   {
            Vector2.up,
            Vector2.right,
            Vector2.down,
            Vector2.left
        };

        if (piece.row + 1 < width - 1)
        {
            Block right = GetPiece(piece.row + 1, piece.column);
            if (matchedPiece.Contains(right) && !checkList.Contains(right))
                ++count;
        }

        if (piece.row - 1 > 0)
        {
            Block left = GetPiece(piece.row - 1, piece.column);
            if (matchedPiece.Contains(left) && !checkList.Contains(left))
                ++count;
        }

        if (piece.column + 1 < height - 1)
        {
            Block up = GetPiece(piece.row, piece.column + 1);
            if (matchedPiece.Contains(up) && !checkList.Contains(up))
                ++count;
        }

        if (piece.column - 1 > 0)
        {
            Block down = GetPiece(piece.row, piece.column - 1);
            if (matchedPiece.Contains(down) && !checkList.Contains(down))
                ++count;
        }

        return count;
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

    private void CrossBombPieces(Block piece)
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
        selectPiece = null;

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
                            Block fallPiece = GetPiece(row, i); // 빈자리의 위에 있는 피스

                            fallPiece.InitPiece(fallPiece.value, fallPiece.row, column, this);
                            fallPiece.moveToPos = new Vector2(fallPiece.row, fallPiece.column);
                            fallPiece.name = "[" + row + " , " + column + "]";
                            boardIndex[fallPiece.row, fallPiece.column] = fallPiece.gameObject;

                            boardIndex[row, i] = null;
                            fallPiece.currState = BlockState.MOVE;

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
                }
            }
        }

        while(!FIndMovingPiece())
        {
            yield return null;
        }

        yield return new WaitForSeconds(waitTime);

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

    private void PieceRegenerate()
    {
        currState = BoardState.WORK;

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

                    while (FindMatchedPiece())
                    {
                        value = Random.Range(0, pieceSprites.Length);
                        piece.value = value;
                    }

                    piece.InitPiece(value, piece.row, piece.column, this);

                    piece.transform.position = new Vector2(piece.row, piece.column + offset);
                    piece.moveToPos = new Vector2(piece.row, piece.column);

                    piece.currState = BlockState.MOVE;

                    piece.gameObject.SetActive(true);
                }
            }
        }
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

                    if (FindMatchedPiece())
                    {
                        SwapBoardIndex(row, column, Vector2.right);
                        return false;
                    }

                    SwapBoardIndex(row, column, Vector2.right);
                }

                if (column < height - 1)
                {
                    SwapBoardIndex(row, column, Vector2.up);

                    if (FindMatchedPiece())
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

    private bool FindMatchedPiece()
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
                                    return true;
                            }
                        }

                        if (column > 0 && column < height - 1)
                        {
                            if (boardIndex[row, column + 1] != null && boardIndex[row, column - 1] != null)
                            {
                                Block upPiece = GetPiece(row, column + 1);
                                Block downPiece = GetPiece(row, column - 1);

                                if (currPiece.value == upPiece.value && currPiece.value == downPiece.value)
                                    return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    private bool FIndMovingPiece()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    Block currPiece = GetPiece(row, column);

                    // Piece Moving Check
                        if (currPiece.currState == BlockState.MOVE)
                            return false;
                }
            }
        }
            return true;
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