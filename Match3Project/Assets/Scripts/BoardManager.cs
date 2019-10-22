using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoardState
{
    INIT,
    ORDER,
    WORK,
}

public class BoardManager : MonoBehaviour
{
    [Header("Board State")]
    public BoardState currState = BoardState.INIT;
    public int width, height;
    public float duration;

    public GameObject[,] boardIndex;

    public List<PieceManager> matchedPieces;
    public GameObject disabledPiece;

    [Header("Piece Parts")]
    public GameObject piecePrefab;
    public Sprite[] pieceSprite;

    void Start()
    {
        boardIndex = new GameObject[width, height];
        matchedPieces = new List<PieceManager>();
        CreateBoard();
        FindAllBoard();
    }

    void Update()
    {
        if (currState == BoardState.WORK)
        {
            FindAllBoard();
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

                piece.InitPiece(pieceValue, row, column);

                SpriteRenderer spritePiece = pieceGo.GetComponent<SpriteRenderer>();
                spritePiece.sprite = pieceSprite[pieceValue];

                pieceGo.transform.parent = transform.parent;
                pieceGo.name = "[" + row + " , " + column + "]";
                boardIndex[row, column] = pieceGo;

            }
        }

        for (int i = 0; i < Mathf.RoundToInt(height * width / 2); ++i)
        {
            GameObject pieceGo = Instantiate(piecePrefab, Vector2.zero, Quaternion.identity);

            pieceGo.transform.parent = disabledPiece.transform;
            pieceGo.name = "DisPiece";
            pieceGo.SetActive(false);
        }
    }

    private void FindAllBoard()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] == null)
                    continue;

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

        MatchedPieceDisabled();
    }

    private void MatchedPieceDisabled()
    {
        foreach (var piece in matchedPieces)
        {
            boardIndex[piece.row, piece.column] = null;
            piece.transform.parent = disabledPiece.transform;
            piece.transform.localPosition = Vector2.zero;

            piece.InitPiece(0, 0, 0);

            piece.name = "DisPiece";
            piece.gameObject.SetActive(false);
        }

        if (matchedPieces.Count > 0)
        {
            //FallingPieces();
            StartCoroutine(FallingPiecesCo());
        }

        Debug.Log(Time.time);
        matchedPieces.Clear();
        currState = BoardState.ORDER;
    }

    private void FallingPieces()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] == null)
                {
                    for (int i = 1; i < height; i++)
                    {
                        if (column + i < height)
                        {
                            if (boardIndex[row, column + i] != null)
                            {
                                PieceManager fallPiece = GetPiece(row, column + i);

                                int fall = column;
                                while (fall >= 0)
                                {
                                    fallPiece.transform.position = new Vector2(row, fall);
                                    fallPiece.column = fall;
                                    fallPiece.gameObject.name = "[" + row + " , " + column + "]";

                                    boardIndex[row, fall] = fallPiece.gameObject;

                                    if (boardIndex[row, fall] != null)
                                        break;

                                    --fall;
                                }
                            }
                        }
                    }
                }
            }
        }
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
                            PieceManager piece = GetPiece(row, i);
                            piece.column = column;
                            piece.transform.position = new Vector3(row, column);
                            boardIndex[row, i] = null;

                            yield return new WaitForSeconds(0.32f);
                            
                            break;
                        }
                    }
                }
            }
        }
    }
    private PieceManager GetPiece(int row, int column)
    {
        return boardIndex[row, column].GetComponent<PieceManager>();
    }
}