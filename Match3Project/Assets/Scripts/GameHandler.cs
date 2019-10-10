using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public int width; // 보드의 넓이
    public int height; // 보드의 높이
    public GameObject piecePrefab; // 블럭 모델
    public GameObject[] pieces; // 블럭들을 저장
    public GameObject[,] allPieces;

    private BlockPiece[,] board;

    void Start()
    {
        board = new BlockPiece[width, height]; // 보드를 생성
        allPieces = new GameObject[width, height]; // 보드 안에 넣을 블럭들을 생성
        Init();
    }

    private void Init()
    {
        for (int column = 0; column < width; ++column)
        {
            for (int row = 0; row < height; ++row)
            {
                Vector2 tempPosition = new Vector2(column, row); // 현재 위치를 가져옴
                GameObject index = Instantiate(piecePrefab, tempPosition, Quaternion.identity); //as GameObject;
                index.transform.parent = this.transform;
                index.name = "[ " + column + " , " + row + " ] ";

                int maxIterations = 0;
                int color = Random.Range(0, pieces.Length);

                while (MatchesAt(column, row, pieces[color]) && maxIterations < 100)
                {
                    // 상하좌우로 3 블럭이 매치가 된 경우 다른 색상으로 변경
                    color = Random.Range(0, pieces.Length);
                    maxIterations++;
                    Debug.Log(maxIterations);
                }
                maxIterations = 0;

                GameObject piece = Instantiate(pieces[color], tempPosition, Quaternion.identity);

                piece.transform.parent = transform;
                piece.name = "[ " + column + " , " + row + " ] ";

                allPieces[column, row] = piece;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allPieces[column - 1, row] != null && allPieces[column - 2, row] != null)
            {
                if (allPieces[column - 1, row].tag == piece.tag && allPieces[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
            if (allPieces[column, row - 1] != null && allPieces[column, row - 2] != null)
            {
                if (allPieces[column, row - 1].tag == piece.tag && allPieces[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }

        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allPieces[column, row - 1] != null && allPieces[column, row - 2] != null)
                {
                    if (allPieces[column, row - 1].tag == piece.tag && allPieces[column, row - 2].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
            if (column > 1)
            {
                if (allPieces[column - 1, row] != null && allPieces[column - 2, row] != null)
                {
                    if (allPieces[column - 1, row].tag == piece.tag && allPieces[column - 2, row].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if(allPieces[column, row].GetComponent<Block>().isMatched)
        {
            Destroy(allPieces[column, row]);
            allPieces[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for(int column = 0; column < width; ++column)
        {
            for(int row = 0; row < height; ++row)
            {
                if(allPieces[column, row] != null)
                {
                    DestroyMatchesAt(column, row);
                }
            }
        }
    }
}
