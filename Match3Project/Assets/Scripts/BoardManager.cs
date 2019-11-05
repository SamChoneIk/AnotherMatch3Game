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
    public float blockFallSpeed;

    public GameObject[,] boardIndex;
    public GameObject disabledPieces;

    [Header("Piece Parts")]
    public GameObject piecePrefab;

    public Block selectPiece;

    public List<Block> matchedPiece;
    public List<Block> disabledPiece;
    public List<Block> verifyPiece;
    public List<Block> itemList; // debug

    public Sprite[] pieceSprites;
    public Sprite[] ItemSprites;

    private float checkTime;

    void Start()
    {
        boardIndex = new GameObject[width, height];

        matchedPiece = new List<Block>();
        disabledPiece = new List<Block>();
        verifyPiece = new List<Block>();
        itemList = new List<Block>();

        StartCoroutine(CreateBoard());
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

            StartCoroutine(PieceRegenerate());
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

    void Update()
    {
        if (currState == BoardState.WORK)
        {
            checkTime = 0;

            if (selectPiece != null)
            {
                // 블럭이 움직이고 있을때
                if (!FindMovingPiece())
                    return;

                if (selectPiece.isTunning && selectPiece.target.isTunning)
                {
                    selectPiece.isTunning = false;
                    selectPiece.target.isTunning = false;

                    currState = BoardState.ORDER;
                    return;
                }

                if(selectPiece.crossBomb || selectPiece.target.crossBomb)
                {
                    UsedCrossBomb(selectPiece);
                    UsedCrossBomb(selectPiece.target);
                }

                if (FindMatched() || matchedPiece.Count > 0)
                    FindMatchedPiece();

                else
                {
                    selectPiece.MoveToBack();
                    selectPiece.target.MoveToBack();

                    return;
                }
            }

            else
                return;
        }

        else if (currState == BoardState.ORDER)
        {
            checkTime += Time.deltaTime;

            if (checkTime > 5f)
            {
                if (DeadLockCheck())
                {
                    Debug.Log("is DeadLock !!");
                    selectPiece = null;

                    StartCoroutine(PieceRegenerate());
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

    IEnumerator CreateBoard()
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

                while (FindMatched())
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

        while(!FindMovingPiece())
        {
            yield return null;
        }

        yield return new WaitForSeconds(waitTime);

        currState = BoardState.ORDER;
    }

    public void FindMatchedPiece()
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

                                    UsedItem(rightPiece, currPiece, leftPiece);
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

                                    UsedItem(upPiece, currPiece, downPiece);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (matchedPiece.Count > 0)
        {
            GenerateItemPiece();
            StartCoroutine(MatchedPieceDisabled());
        }

        else
            currState = BoardState.ORDER;
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

                Debug.Log("rows = " + rows + " & " + "columns = " + cols);

                if (rows >= 2 && cols >= 2)
                {
                    Block bombPiece = matchedPiece[i];

                    if (selectPiece != null)
                    {
                        if (selectPiece.value == matchedPiece[i].value)
                            bombPiece = selectPiece;

                        if (selectPiece.target != null && selectPiece.target.value == matchedPiece[i].value)
                            bombPiece = selectPiece.target;
                    }

                    else
                        bombPiece = verifyPiece[Random.Range(prevCount, verifyPiece.Count)];

                    Debug.Log("generate CrossBomb");

                    bombPiece.crossBomb = true;
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

                Block check = GetPiece(matchedPiece[i].row + r, matchedPiece[i].column);

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
                Block bombPiece = matchedPiece[i];

                if(selectPiece != null)
                {
                    if (selectPiece.value == matchedPiece[i].value)
                        bombPiece = selectPiece;

                    if (selectPiece.target != null && selectPiece.target.value == matchedPiece[i].value)
                        bombPiece = selectPiece.target;
                }
                
                else
                    bombPiece = verifyPiece[Random.Range(prevCount, verifyPiece.Count)];

                Debug.Log("generate RowBomb");

                bombPiece.rowBomb = true;
                bombPiece.itemSprite.sprite = ItemSprites[0];

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

                Block check = GetPiece(matchedPiece[i].row, matchedPiece[i].column + c);

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
                Block bombPiece = matchedPiece[i];

                if (selectPiece != null)
                {
                    if (selectPiece.value == matchedPiece[i].value)
                        bombPiece = selectPiece;

                    if (selectPiece.target != null && selectPiece.target.value == matchedPiece[i].value)
                        bombPiece = selectPiece.target;
                }

                else
                    bombPiece = verifyPiece[Random.Range(prevCount, verifyPiece.Count)];

                Debug.Log("generate ColumnBomb");

                bombPiece.columnBomb = true;
                bombPiece.itemSprite.sprite = ItemSprites[1];

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

    private void FindDirectionMatchedPiece(Block piece, ref int rows, ref int cols)
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

                Block check = GetPiece(piece.row + ((int)dir.x * i), piece.column + ((int)dir.y * i));

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

    private bool FindNeighborPiece(Block piece)
    {
        if (piece.row + 1 < width - 1)
        {
            Block right = GetPiece(piece.row + 1, piece.column);
            if (matchedPiece.Contains(right) && !verifyPiece.Contains(right) && 
                piece.value == right.value)
                return true;
        }

        if (piece.row - 1 > 0)
        {
            Block left = GetPiece(piece.row - 1, piece.column);
            if (matchedPiece.Contains(left) && !verifyPiece.Contains(left) && 
                piece.value == left.value)
                return true;
        }

        if (piece.column + 1 < height - 1)
        {
            Block up = GetPiece(piece.row, piece.column + 1);
            if (matchedPiece.Contains(up) && !verifyPiece.Contains(up) && 
                piece.value == up.value)
                return true;
        }

        if (piece.column - 1 > 0)
        {
            Block down = GetPiece(piece.row, piece.column - 1);
            if (matchedPiece.Contains(down) && !verifyPiece.Contains(down) &&
                piece.value == down.value)
                return true;
        }

        return false;
    }

    private void UsedItem(Block piece1, Block piece2, Block piece3)
    {
        if (piece1.rowBomb)
        {
            piece1.rowBomb = false;
            GetRowPieces(piece1.column);
        }
        if (piece2.rowBomb)
        {
            piece2.rowBomb = false;
            GetRowPieces(piece2.column);
        }
        if (piece3.rowBomb)
        {
            piece3.rowBomb = false;
            GetRowPieces(piece3.column);
        }

        if (piece1.columnBomb)
        {
            piece1.rowBomb = false;
            GetColumnPieces(piece1.row);
        }
        if (piece2.columnBomb)
        {
            piece2.rowBomb = false;
            GetColumnPieces(piece2.row);
        }
        if (piece3.columnBomb)
        {
            piece3.rowBomb = false;
            GetColumnPieces(piece3.row);
        }
    }

    public void UsedCrossBomb(Block piece)
    {
        if (piece.crossBomb)
        {
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
                Block piece = GetPiece(row, column);

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
                Block piece = GetPiece(row, column);

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

    IEnumerator MatchedPieceDisabled()
    {
        foreach (var piece in matchedPiece)
        {
            piece.AllClearPiece();
            disabledPiece.Add(piece);
        }

        matchedPiece.Clear();
        verifyPiece.Clear();
        selectPiece = null;

        yield return new WaitForSeconds(waitTime);

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
                            fallPiece.name = "[" + fallPiece.row + " , " + fallPiece.column + "]";
                            boardIndex[fallPiece.row, fallPiece.column] = fallPiece.gameObject;

                            boardIndex[row, i] = null;

                            fallPiece.currState = BlockState.MOVE;
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
                    Block enabledPiece = disabledPiece[0];

                    enabledPiece.InitPiece(Random.Range(0, pieceSprites.Length), row, column, this);
                    enabledPiece.transform.position = new Vector2(enabledPiece.row, height + fall);
                    enabledPiece.moveToPos = new Vector2(enabledPiece.row, enabledPiece.column);
                    enabledPiece.name = "[" + enabledPiece.row + " , " + enabledPiece.column + "]";
                    boardIndex[enabledPiece.row, enabledPiece.column] = enabledPiece.gameObject;

                    enabledPiece.gameObject.SetActive(true);

                    enabledPiece.currState = BlockState.MOVE;
                    disabledPiece.Remove(enabledPiece);
                    ++fall;
                }
            }

            fall = 0;
        }

        while (!FindMovingPiece())
        {
            yield return null;
        }

        yield return new WaitForSeconds(waitTime);

        FindMatchedPiece();
    }

    private void FallPiece()
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
                            fallPiece.name = "[" + fallPiece.row + " , " + fallPiece.column + "]";
                            boardIndex[fallPiece.row, fallPiece.column] = fallPiece.gameObject;

                            boardIndex[row, i] = null;

                            fallPiece.currState = BlockState.MOVE;
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
                    Block enabledPiece = disabledPiece[0];

                    enabledPiece.InitPiece(Random.Range(0, pieceSprites.Length), row, column, this);
                    enabledPiece.transform.position = new Vector2(enabledPiece.row, height + fall);
                    enabledPiece.moveToPos = new Vector2(enabledPiece.row, enabledPiece.column);
                    enabledPiece.name = "[" + enabledPiece.row + " , " + enabledPiece.column + "]";
                    boardIndex[enabledPiece.row, enabledPiece.column] = enabledPiece.gameObject;

                    enabledPiece.gameObject.SetActive(true);

                    enabledPiece.currState = BlockState.MOVE;
                    disabledPiece.Remove(enabledPiece);
                    ++fall;
                }
            }

            fall = 0;
        }

        //while (!FindMovingPiece())
        //{
        //  }

        //yield return new WaitForSeconds(waitTime);

        //FindMatchedPiece();
    }

    public Block GetPiece(int row, int column)
    {
        return boardIndex[row, column].GetComponent<Block>();
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

                    if (FindMatched())
                    {
                        SwapBoardIndex(row, column, Vector2.right);
                        return false;
                    }

                    SwapBoardIndex(row, column, Vector2.right);
                }

                if (column < height - 1)
                {
                    SwapBoardIndex(row, column, Vector2.up);

                    if (FindMatched())
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

    IEnumerator PieceRegenerate()
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

                    while (FindMatched())
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

        while(!FindMovingPiece())
        {
            yield return null;
        }

        yield return new WaitForSeconds(waitTime);

        currState = BoardState.ORDER;
    }

    private bool FindMatched()
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

    private bool FindMovingPiece()
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
                    Block piece = GetPiece(row, column);

                    if (piece.rowBomb || piece.columnBomb || piece.crossBomb)
                        Debug.Log("is itemPiece");
                }
            }
        }
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
            if (matchedPiece.Contains(right) && !verifyPiece.Contains(right))
                ++count;
        }

        if (piece.row - 1 > 0)
        {
            Block left = GetPiece(piece.row - 1, piece.column);
            if (matchedPiece.Contains(left) && !verifyPiece.Contains(left))
                ++count;
        }

        if (piece.column + 1 < height - 1)
        {
            Block up = GetPiece(piece.row, piece.column + 1);
            if (matchedPiece.Contains(up) && !verifyPiece.Contains(up))
                ++count;
        }

        if (piece.column - 1 > 0)
        {
            Block down = GetPiece(piece.row, piece.column - 1);
            if (matchedPiece.Contains(down) && !verifyPiece.Contains(down))
                ++count;
        }

        return count;
    }

    public void PieceListSort(List<Block> pieces, bool column = false, bool descending = false)
    {
        if (column)
        {
            if (descending)
            {
                pieces.Sort(delegate (Block a, Block b)
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
                pieces.Sort(delegate (Block a, Block b)
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
                pieces.Sort(delegate (Block a, Block b)
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
        }
    }
}