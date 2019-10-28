using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float blockDuration;
    public float boardDuration;

    public GameObject[,] boardIndex;
    public GameObject disabledPieces;
    public Block selectPiece;
    public Block targetPiece;

    public List<Block> matchedPiece;
    public List<Block> disabledPiece;

    [Header("Piece Parts")]
    public GameObject piecePrefab;
    public Sprite[] pieceSprites;

    void Start()
    {
        boardIndex = new GameObject[width, height];

        matchedPiece = new List<Block>();
        disabledPiece = new List<Block>();

        CreateBoard();
        FindMatchedIndex();

        //transform.SetSiblingIndex(0);
    }

    private void DebugSystem()
    {
        // debug Array Check
        if (Input.GetMouseButtonDown(1))
            DebugBoardChecking(true);

        // debug Piece Moving Check
        if (Input.GetMouseButtonDown(2))
            DebugBoardChecking(false);

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
    }

    void Update()
    {
        if (currState == BoardState.WORK)
        {
            if (selectPiece != null && targetPiece != null)
            {
                if (selectPiece.currState == BlockState.WAIT && targetPiece.currState == BlockState.WAIT)
                {
                    if (selectPiece.isTunning && targetPiece.isTunning)
                    {
                        selectPiece.isTunning = false;
                        targetPiece.isTunning = false;

                        selectPiece = null;
                        targetPiece = null;

                        return;
                    }

                    if(IndexCheck())
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
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                GameObject pieceGo = Instantiate(piecePrefab, new Vector2(row, column), Quaternion.identity);
                Block piece = pieceGo.GetComponent<Block>();

                piece.InitPiece(Random.Range(0, pieceSprites.Length), row, column, this);

                pieceGo.transform.parent = transform;
                pieceGo.name = "[" + row + " , " + column + "]";

                boardIndex[piece.row, piece.column] = pieceGo;
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
                                    if (!matchedPiece.Contains(currPiece))
                                        matchedPiece.Add(currPiece);

                                    if (!matchedPiece.Contains(rightPiece))
                                        matchedPiece.Add(rightPiece);

                                    if (!matchedPiece.Contains(leftPiece))
                                        matchedPiece.Add(leftPiece);
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
                                    if (!matchedPiece.Contains(currPiece))
                                        matchedPiece.Add(currPiece);

                                    if (!matchedPiece.Contains(upPiece))
                                        matchedPiece.Add(upPiece);

                                    if (!matchedPiece.Contains(downPiece))
                                        matchedPiece.Add(downPiece);
                                }
                            }
                        }
                    }
                }
            }
        }

        MatchedPieceDisabled();
    }

    private void MatchedPieceDisabled()
    {
        if (matchedPiece.Count > 0)
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

            //StartCoroutine(FallingPieces());
            FallPieces();
        }

        else
        {
            if (DeadLockCheck())
            {
                Debug.Log("is dead lock!");
            }
            
            currState = BoardState.ORDER;
        }
    }

    private void FallPieces()
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

    public bool IndexCheck()
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

    public Block GetPiece(int row, int column)
    {
        return boardIndex[row, column].GetComponent<Block>();
    }

    public void SetPiece(Block piece)
    {
        piece.transform.position = new Vector2(piece.row, piece.column + offset);
        piece.name = "[" + piece.row + " , " + piece.column + "]";
        boardIndex[piece.row, piece.column] = piece.gameObject;

        /*if (boardIndex[selectPiece.row, selectPiece.column] != selectPiece.gameObject)
            boardIndex[selectPiece.row, selectPiece.column] = selectPiece.gameObject;*/
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
                    Block piece = GetPiece(row, column);
                    piece.gameObject.SetActive(false);

                    piece.InitPiece(Random.Range(0, pieceSprites.Length), piece.row, piece.column, this);
                    piece.transform.position = new Vector2(piece.row, piece.column + offset);
                    piece.moveToPos = new Vector2(piece.row, piece.column);
                    
                    piece.currState = BlockState.WAIT;

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
    }

    public void DebugBoardChecking(bool isPos = false)
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    Block piece = GetPiece(row, column);
                    /*if (boardIndex[row, column].GetComponent<Block>().value == GetPiece(row, column).value)
                        Debug.Log("[" + row + " , " + column + "] = " + GetPiece(row, column).value);*/

                    if (isPos)
                        Debug.Log("[" + row + " , " + column + "] = " + piece.value + ", PosX : " + piece.transform.position.x + ", PosY : " + piece.transform.position.y);

                    else
                    {
                        if (piece.currState == BlockState.MOVE)
                            Debug.Log("[" + row + " , " + column + "] is Moving");
                    }
                }
            }
        }
    }
}