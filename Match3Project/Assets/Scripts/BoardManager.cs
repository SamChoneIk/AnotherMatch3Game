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
    public GameObject[,] board;

    [Header("Piece Parts")]
    public GameObject piecePrefab;
    public Sprite[] pieceSprite;

    void Start()
    {
        board = new GameObject[width, height];
        CreateBoard();
    }

    void Update()
    {
        if (currState == BoardState.WORK)
        {
              
        }
    }

    private void CreateBoard()
    {
        for(int column = 0; column < height; ++column)
        {
            for(int row = 0; row < width; ++row)
            {
                Vector2 piecePos = new Vector2(row, column);
                int pieceValue = Random.Range(0, pieceSprite.Length);

                GameObject pieceGo = Instantiate(piecePrefab, piecePos, Quaternion.identity);
                PieceManager piece = pieceGo.GetComponent<PieceManager>();

                piece.InitPiece(pieceValue, row, column);

                SpriteRenderer spritePiece = pieceGo.GetComponent<SpriteRenderer>();
                spritePiece.sprite = pieceSprite[pieceValue];

                pieceGo.name = "[" + row + " , " + column + "]";
                board[row, column] = pieceGo;
            }
        }
    }
}