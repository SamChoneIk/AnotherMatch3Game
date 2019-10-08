using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject piecePrefab;
    public GameObject[] pieces;
    public GameObject[,] allDots;

    private BlockPiece[,] board;
    

    void Start()
    {
        board = new BlockPiece[width, height];
        allDots = new GameObject[width, height];
        Init();
    }

    private void Init()
    {
        for (int column = 0; column < height; column++)
        {
            for (int row = 0; row < width; row++)
            {
                Vector2 tempPosition = new Vector2(row, column); // 넓이, 높이만큼 보드를 생성
                GameObject index = Instantiate(piecePrefab, tempPosition, Quaternion.identity) as GameObject;
                index.transform.parent = this.transform;
                index.name = "[ " + row + " , " + column + " ] ";

                int color = Random.Range(0, pieces.Length);
                GameObject piece = Instantiate(pieces[color], tempPosition, Quaternion.identity);
                piece.transform.parent = transform;
                piece.name = "[ " + row + " , " + column + " ] ";

                allDots[row, column] = piece;
            }
        }
    }
}
