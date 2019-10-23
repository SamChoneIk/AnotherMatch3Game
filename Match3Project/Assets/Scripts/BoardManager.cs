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
    public BoardState currState = BoardState.ORDER;
    public int width, height;
    public float duration;

    public GameObject[,] boardIndex;
    public GameObject disPieces;
    public float waitTime = 0.16f;

    public List<PieceManager> matchedPieces;
    public List<PieceManager> disabledPieces;

    [Header("Piece Parts")]
    public GameObject piecePrefab;
    public Sprite[] pieceSprite;

    void Start()
    {
        boardIndex = new GameObject[width, height];

        matchedPieces = new List<PieceManager>();
        disabledPieces = new List<PieceManager>();

        CreateBoard();
    }

    void Update()
    {
        // debug board Checking
        if (Input.GetMouseButtonDown(1))
        {
            FindAllBoard();
        }

        if (currState == BoardState.WORK)
        {
            if (matchedPieces.Count > 0)
                MatchedPieceDisabled();

            else
                currState = BoardState.ORDER;
        }
    }

    private void CreateBoard()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                Vector2 piecePos = new Vector2(row, column);
                int pieceValue = Random.Range(0, pieceSprite.Length);

                GameObject pieceGo = Instantiate(piecePrefab, piecePos, Quaternion.identity);
                PieceManager piece = pieceGo.GetComponent<PieceManager>();

                piece.SetPiece(pieceValue, row, column);

                pieceGo.GetComponent<SpriteRenderer>().sprite = pieceSprite[piece.value];

                pieceGo.transform.parent = transform;
                pieceGo.name = "[" + row + " , " + column + "]";
                boardIndex[row, column] = pieceGo;
            }
        }

        for (int i = 0; i < Mathf.RoundToInt(height * width / 2); ++i)
        {
            GameObject pieceGo = Instantiate(piecePrefab, Vector2.zero, Quaternion.identity);
            PieceManager piece = pieceGo.GetComponent<PieceManager>();

            pieceGo.transform.parent = disPieces.transform;
            pieceGo.name = "DisPiece";
            pieceGo.SetActive(false);

            disabledPieces.Add(piece);
        }
    }

    public void FindAllBoard()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    PieceManager currPiece = GetPiece(row, column);

                    if (currPiece != null)
                    {
                        if (row > 0 && row < width - 1)
                        {
                            if (boardIndex[row + 1, column] != null && boardIndex[row - 1, column] != null)
                            {
                                PieceManager rightPiece = GetPiece(row + 1, column);
                                PieceManager leftPiece = GetPiece(row - 1, column);

                                if (currPiece.value == rightPiece.value && currPiece.value == leftPiece.value)
                                {
                                    if (!matchedPieces.Contains(currPiece))
                                        matchedPieces.Add(currPiece);

                                    if (!matchedPieces.Contains(rightPiece))
                                        matchedPieces.Add(rightPiece);

                                    if (!matchedPieces.Contains(leftPiece))
                                        matchedPieces.Add(leftPiece);
                                }
                            }
                        }

                        if (column > 0 && column < height - 1)
                        {
                            if (boardIndex[row, column + 1] != null && boardIndex[row, column - 1] != null)
                            {
                                PieceManager upPiece = GetPiece(row, column + 1);
                                PieceManager downPiece = GetPiece(row, column - 1);

                                if (currPiece.value == upPiece.value && currPiece.value == downPiece.value)
                                {
                                    if (!matchedPieces.Contains(currPiece))
                                        matchedPieces.Add(currPiece);

                                    if (!matchedPieces.Contains(upPiece))
                                        matchedPieces.Add(upPiece);

                                    if (!matchedPieces.Contains(downPiece))
                                        matchedPieces.Add(downPiece);
                                }
                            }
                        }
                    }
                }
            }
        }
        currState = BoardState.WORK;
    }

    private void MatchedPieceDisabled()
    {
        foreach (var piece in matchedPieces)
        {
            //Debug.Log($"disabledPieces {piece.row} , {piece.column}");
            boardIndex[piece.row, piece.column] = null;
            
            piece.transform.parent = disPieces.transform;
            piece.transform.localPosition = Vector2.zero;

            piece.SetPiece(0, 0, 0);

            piece.name = "DisPiece";
            piece.gameObject.SetActive(false);
            disabledPieces.Add(piece);
        }
        matchedPieces.Clear();

        StartCoroutine(FallingPiecesCo());
    }

    IEnumerator FallingPiecesCo()
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
                            yield return new WaitForSeconds(waitTime);

                            PieceManager fallPiece = GetPiece(row, i); // 빈자리의 위에 있는 피스

                            fallPiece.column = column;
                            fallPiece.SetPositionPiece();

                            boardIndex[row, i] = null;

                            yield return new WaitForSeconds(waitTime);

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
                    yield return new WaitForSeconds(waitTime);

                    PieceManager enabledPiece = EnabledPiece(row, height - 1);

                    enabledPiece.column = column;


                    //enabledPiece.transform.position = new Vector2(row, enabledPiece.column);
                    // enabledPiece.name = "[" + row + " , " + column + "]";
                    //boardIndex[row, column] = enabledPiece.gameObject;


                    enabledPiece.SetPositionPiece();

                    if (boardIndex[row, height - 1] != null)
                        continue;

                    boardIndex[row, height - 1] = null;

                    yield return new WaitForSeconds(waitTime);
                }
            }
        }
    }

    private PieceManager EnabledPiece(int row, int column)
    {
        PieceManager piece = disabledPieces[0];

        piece.SetPiece(Random.Range(0, pieceSprite.Length), row, column);

        piece.GetComponent<SpriteRenderer>().sprite = pieceSprite[piece.value];
        piece.transform.parent = transform;

        piece.gameObject.SetActive(true);

        disabledPieces.RemoveAt(0);

        return piece;
    }

    public PieceManager GetPiece(int row, int column)
    {
        return boardIndex[row, column].GetComponent<PieceManager>();
    }
}