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
    public Block selectPiece;
    public Block targetPiece;

    public List<Block> matchedPiece;

    public List<Block> rowMatchedPiece;
    public List<Block> columnMatchedPiece;

    public List<Block> disabledPiece;
    public List<Block> itemPieces;

    [Header("Piece Parts")]
    public GameObject piecePrefab;
    public Sprite[] pieceSprites;

    private float checkTime;

    void Start()
    {
        boardIndex = new GameObject[width, height];

        matchedPiece = new List<Block>();
        disabledPiece = new List<Block>();
        itemPieces = new List<Block>();

        CreateBoard();
        //FindMatchedIndex();
    }

    private void DebugSystem()
    {
        // debug Array Check
        if (Input.GetMouseButtonDown(1))
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
        {
            PieceRegenerate();
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
            //Debug.Log("matched");
            // GetItemAtMatchedPiece();
            CheckingBomb();
            MatchedPieceDisabled();
        }

        else
            currState = BoardState.ORDER;
    }

    private void CheckingBomb()
    {
        if (matchedPiece.Count <= 3)
            return;

        CrossbombCheck();

        int idx = 0;
        int rows = 0;
        int columns = 0;

        for (int column = 0; column < height; ++column)
        {
            rows = 0;
            for (int row = 0; row < width; ++row)
            {
                //Debug.Log("index " + "[" + row + " , " + column + "] = " + idx);

                if (matchedPiece[idx].crossBomb)
                    break;

                if (boardIndex[row, column] == matchedPiece[idx].gameObject)
                {
                    idx = idx != matchedPiece.Count - 1 ? ++idx : matchedPiece.Count - 1;
                }


                if (rows > 3)
                {
                    Debug.Log("Generate Row Bomb");
                }

                rows = 0;
            }
        }

        idx = 0;

        for (int row = 0; row < width; ++row)
        {
            columns = 0;
            for (int column = 0; column < height; ++column)
            {
                //Debug.Log("index " + "[" + row + " , " + column + "] = " + idx);

                if(matchedPiece[idx].crossBomb)
                    break;

                if (boardIndex[row, column] == matchedPiece[idx].gameObject)
                {
                    ++columns;
                    idx = idx != matchedPiece.Count - 1 ? ++idx : matchedPiece.Count - 1;
                }

                else
                {
                    if (columns > 3)
                    {
                        Debug.Log("Generate Column Bomb");
                    }

                    columns = 0;
                }
            }
        }


    }

    private void CrossbombCheck()
    {
        List<Block> generateItemPiece = new List<Block>();
        int rows = 0;
        int columns = 0;

        foreach (var piece in matchedPiece)
        {
            foreach (var check in matchedPiece)
            {
                if (piece == check || check.crossBomb)
                    continue;

                if (piece.row == check.row)
                    columns++;

                else if (piece.column == check.column)
                    rows++;

                else
                {
                    if (rows > 1 && columns > 1)
                    {
                        Debug.Log("Generate Cross Bomb");
                        piece.crossBomb = true;
                    }
                }
            }
        }
    }

    private void MatchedPieceDisabled()
    {
        foreach (var piece in matchedPiece)
        {
            boardIndex[piece.row, piece.column] = null;

            piece.target = null;

            piece.InitPiece(0, 0, 0, this);

            piece.name = "DefaultPiece";
            piece.transform.parent = disabledPieces.transform;
            piece.transform.position = new Vector2(piece.row, piece.column);

            piece.gameObject.SetActive(false);
            disabledPiece.Add(piece);
        }

        matchedPiece.Clear();

        selectPiece = null;
        targetPiece = null;

        StartCoroutine(FallPieces());
    }

    IEnumerator FallPieces()
    {
        List<Block> fallPieces = new List<Block>();

        for (int column = 0; column < height; ++column)
        {
            /*while (!IndexCheck(true))
            {
                //Debug.Log("Piece Moving " + Time.time);
                //yield return new WaitForSeconds(waitTime);
                yield return null;
            }*/

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
            /*while (!IndexCheck(true))
            {
                //Debug.Log("Piece Moving " + Time.time);
                //yield return new WaitForSeconds(waitTime);
                yield return null;
            }*/
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
            //Debug.Log("Piece Moving " + Time.time);
            //yield return new WaitForSeconds(waitTime);
            yield return null;
        }
        yield return new WaitForSeconds(waitTime);
        //yield return null;

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