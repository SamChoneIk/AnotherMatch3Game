using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    wait,
    move,
}

public class Board : MonoBehaviour
{
    public GameState currState = GameState.move; // 현재 게임 상태

    public int width; // 보드의 넓이
    public int height; // 보드의 높이
    public int offSet; // 블럭이 시작되는 높이

    public GameObject piecePrefab; // 블럭 모델
    public Sprite[] pieces; // 블럭들을 저장
    public GameObject[,] allPieces;

    void Start()
    {
        //board = new BlockPiece[width, height]; // 보드를 생성
        allPieces = new GameObject[width, height]; // 보드 안에 넣을 블럭들을 생성
        Init();
    }

    private void Init()
    {
        for (int column = 0; column < width; ++column)
        {
            for (int row = 0; row < height; ++row)
            {
                // 현재 위치를 가져옴
                Vector2 tempPos = new Vector2(column, row + offSet);
                // 임의의 색상 색출
                int value = Random.Range(0, pieces.Length);
                // 같은 색상 누적 값
                int maxIterations = 0;

                GameObject go = Instantiate(piecePrefab, tempPos, Quaternion.identity);

                Block piece = go.GetComponent<Block>();
                piece.Init(row, column, value);
                while (MatchesAt(column, row, piece) && maxIterations < 100)
                {
                    // 상하좌우로 3 블럭이 매치가 된 경우 다른 색상으로 변경
                    value = Random.Range(0, pieces.Length);
                    maxIterations++;
                    //Debug.Log(maxIterations);
                }
                maxIterations = 0;

                SpriteRenderer sprite = go.GetComponent<SpriteRenderer>();
                sprite.sprite = pieces[value];

                go.transform.parent = transform;
                go.name = "[ " + column + " , " + row + " ] ";
                allPieces[column, row] = go;
            }
        }
    }

    public Block GetBlock(int column, int row)
    {
        return allPieces[column, row].GetComponent<Block>();
    }

    // 같은 색상 블럭 검사
    private bool MatchesAt(int column, int row, Block piece)
    {
        if (column > 1 && row > 1)
        {
            if (allPieces[column - 1, row] != null && allPieces[column - 2, row] != null)
            {
                if (GetBlock(column - 1, row).value == piece.value && GetBlock(column - 2, row).value == piece.value)
                {
                    return true;
                }
            }

            if (allPieces[column, row - 1] != null && allPieces[column, row - 2] != null)
            {
                if (GetBlock(column, row - 1).value == piece.value && GetBlock(column, row - 2).value == piece.value)
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
                    if (GetBlock(column, row - 1).value == piece.value && GetBlock(column, row - 2).value == piece.value)
                    {
                        return true;
                    }
                }
            }
            if (column > 1)
            {
                if (allPieces[column - 1, row] != null && allPieces[column - 2, row] != null)
                {
                    if (GetBlock(column - 1, row).value == piece.value && GetBlock(column - 2, row).value == piece.value)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // 블럭을 삭제
    private void DestroyMatchesAt(int column, int row)
    {
        if (allPieces[column, row].GetComponent<Block>().isMatched)
        {
            Destroy(allPieces[column, row]);
            allPieces[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int column = 0; column < width; ++column)
        {
            for (int row = 0; row < height; ++row)
            {
                if (allPieces[column, row] != null)
                {
                    DestroyMatchesAt(column, row);
                }
            }
        }
        StartCoroutine(DecreaseRow());
    }

    // 보드 순회하며 빈공간을 찾아 블럭을 떨굼
    private IEnumerator DecreaseRow()
    {
        int nullCount = 0;

        for (int column = 0; column < width; ++column)
        {
            for (int row = 0; row < height; ++row)
            {
                if (allPieces[column, row] == null)
                {
                    nullCount++;
                }

                else if (nullCount > 0)
                {
                    allPieces[column, row].GetComponent<Block>().row -= nullCount;
                    allPieces[column, row] = null;
                }
            }

            nullCount = 0;
        }

        yield return new WaitForSeconds(0.4f);
        StartCoroutine(FillBoard());
    }

    // 빈공간에 블럭을 채움
    private void RefillBoard()
    {
        for (int column = 0; column < width; ++column)
        {
            for (int row = 0; row < height; ++row)
            {
                if (allPieces[column, row] == null)
                {
                    Vector2 tempPos = new Vector2(column, row + offSet);
                    int color = Random.Range(0, pieces.Length);

                    GameObject go = Instantiate(piecePrefab, tempPos, Quaternion.identity);

                    go.transform.parent = transform;
                    go.name = "[ " + column + " , " + row + " ] ";

                    allPieces[column, row] = go;
                    go.GetComponent<Block>().row = row;
                    go.GetComponent<Block>().column = column;
                }
            }
        }
    }

    // 매치되는 블럭을 검사
    private bool MatchesOnBoard()
    {
        for (int column = 0; column < width; ++column)
        {
            for (int row = 0; row < height; ++row)
            {
                if (allPieces[column, row] != null)
                {
                    if (allPieces[column, row].GetComponent<Block>().isMatched)
                        return true;
                }
            }
        }
        return false;
    }

    // 보드를 채움
    private IEnumerator FillBoard()
    {
        RefillBoard();
        yield return new WaitForSeconds(0.5f);

        while(MatchesOnBoard())
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }

        yield return new WaitForSeconds(0.5f);
        currState = GameState.move;

    }
}