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

    public PieceManager selectPiece;

    [Header("Piece Parts")]
    public GameObject piecePrefab;
    public Sprite[] pieceSprite;

    void Start()
    {
        boardIndex = new GameObject[width, height];

        matchedPieces = new List<PieceManager>();
        disabledPieces = new List<PieceManager>();

        CreateBoard();
        FindAllBoard();
    }

    void Update()
    {
        if (matchedPieces.Count > 0)
            MatchedPieceDisabled();

        else
            currState = BoardState.ORDER;

        /*if (currState == BoardState.ORDER)
        {
            return;
        }


        else
        {
            //selectPiece.TurnBackPiece();
            //Debug.Log(Time.time);
            //currState = BoardState.ORDER;
        }*/
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
    }

    private void MatchedPieceDisabled()
    {
        foreach (var piece in matchedPieces)
        {
            boardIndex[piece.row, piece.column] = null;
            piece.transform.parent = disPieces.transform;
            piece.transform.localPosition = Vector2.zero;

            piece.SetPiece(0, 0, 0);

            piece.name = "DisPiece";
            piece.gameObject.SetActive(false);
            disabledPieces.Add(piece);
        }
        matchedPieces.Clear();

        StartCoroutine(FallingPieces());
    }

    IEnumerator FallingPieces()
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

                            PieceManager piece = GetPiece(row, i); // 빈자리의 위에 있는 피스
                            piece.SetPositionPiece(row, column); // 떨어질 곳으로 위치 변경
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
                    PieceManager enabledPiece = EnabledPiece(row);

                    yield return new WaitForSeconds(waitTime);

                    enabledPiece.SetPiece(enabledPiece.value, row, column);
                    enabledPiece.SetPosition();

                    enabledPiece.name = "[" + enabledPiece.row + " , " + enabledPiece.column + "]";
                    boardIndex[row, column] = enabledPiece.gameObject;

                    yield return new WaitForSeconds(waitTime);
                }
            }
        }

        FindAllBoard();
    }

    private PieceManager EnabledPiece(int row)
    {
        PieceManager piece = disabledPieces[0];
        piece.gameObject.SetActive(true);

        piece.SetPiece(Random.Range(0, pieceSprite.Length), row, height - 1);
        piece.SetPosition();

        piece.GetComponent<SpriteRenderer>().sprite = pieceSprite[piece.value];
        piece.transform.parent = transform;

        disabledPieces.RemoveAt(0);

        return piece;
    }

    private PieceManager GetPiece(int row, int column)
    {
        return boardIndex[row, column].GetComponent<PieceManager>();
    }
}