using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    wait,
    move
}

public enum Tiles
{
    Breakable,
    Blank,
    Normal
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public Tiles tiles;
}

public class Board : MonoBehaviour
{
    public GameState currState = GameState.move;

    [Header("Board offset")]
    public int width;
    public int height;
    public int offSet;

    [Header("Board Prefab")]
    public GameObject pieceModel;
    public Sprite[] pieceSprite;
    public GameObject breakableTileModel;
    public GameObject destroyParticle;

    [Header("Board")]
    public TileType[] boardLayout;
    public Piece currPiece;

    [HideInInspector]
    public GameObject[,] allPieces;
    private Tile[,] breakableTiles;
    private bool[,] blankSpaces;

    private FindMatches findMatches;

    void Start()
    {
        allPieces = new GameObject[width, height];

        findMatches = GetComponent<FindMatches>();
        breakableTiles = new Tile[width, height];
        blankSpaces = new bool[width, height];

        GenerateBlankSpaces();
        GenerateBreakableTiles();
        InitBoard();
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tiles == Tiles.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tiles == Tiles.Breakable)
            {
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakableTileModel, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<Tile>();
            }
        }
    }

    private void InitBoard()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (!blankSpaces[column, row])
                {
                    Vector2 startPos = new Vector2(column, row + offSet);

                    int value = Random.Range(0, pieceSprite.Length);
                    int maxIterations = 0;

                    GameObject pieceGo = Instantiate(pieceModel, startPos, Quaternion.identity);
                    Piece piece = pieceGo.GetComponent<Piece>();
                    piece.value = value;

                    while (MatchesAt(column, row, piece) && maxIterations < 100)
                    {
                        value = Random.Range(0, pieceSprite.Length);
                        maxIterations++;
                        Debug.Log(maxIterations);
                    }
                    maxIterations = 0;

                    piece.InitPiece(column, row, value);

                    SpriteRenderer sprite = pieceGo.GetComponent<SpriteRenderer>();
                    sprite.sprite = pieceSprite[piece.value];

                    pieceGo.transform.parent = this.transform;
                    pieceGo.name = "( " + column + ", " + row + " )";
                    allPieces[column, row] = pieceGo;
                }
            }
        }
    }

    public Piece GetPiece(int column, int row)
    {
        return allPieces[column, row].GetComponent<Piece>();
    }

    private bool MatchesAt(int column, int row, Piece piece)
    {
        if (column > 1 && row > 1)
        {
            if (allPieces[column - 1, row] != null && allPieces[column - 2, row] != null)
            {
                if (GetPiece(column - 1, row).value == piece.value && GetPiece(column - 2, row).value == piece.value)
                    return true;
            }

            if (allPieces[column, row - 1] != null && allPieces[column, row - 2] != null)
            {
                if (GetPiece(column, row-1).value == piece.value && GetPiece(column, row - 2).value == piece.value)
                    return true;
            }
        }

        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allPieces[column, row - 1] != null && allPieces[column, row - 2] != null)
                {
                    if (GetPiece(column, row - 1).value == piece.value && GetPiece(column, row - 2).value == piece.value)
                        return true;
                }
            }

            if (column > 1)
            {
                if (allPieces[column - 1, row] != null && allPieces[column - 2, row] != null)
                {
                    if (GetPiece(column - 1, row).value == piece.value && GetPiece(column - 2, row).value == piece.value)
                        return true;
                }
            }
        }

        return false;
    }

    private bool ColumnOrRow()
    {
        int horizontal = 0;
        int vertical = 0;
        Piece firstPiece = findMatches.currMatches[0].GetComponent<Piece>();
        if (firstPiece != null)
        {
            foreach (GameObject currentPiece in findMatches.currMatches)
            {
                Piece piece = currentPiece.GetComponent<Piece>();

                if (piece.row == firstPiece.row)
                    horizontal++;

                if (piece.column == firstPiece.column)
                    vertical++;
            }
        }

        return (vertical == 5 || horizontal == 5);
    }

    private void CheckToMakeBombs()
    {
        if (findMatches.currMatches.Count == 4 || findMatches.currMatches.Count == 7)
            findMatches.CheckBombs();

        if (findMatches.currMatches.Count == 5 || findMatches.currMatches.Count == 8)
        {
            if (ColumnOrRow())
            { 
                if (currPiece != null)
                {
                    if (currPiece.isMatched)
                    {
                        if (!currPiece.isColorBomb)
                        {
                            currPiece.isMatched = false;
                            currPiece.MakeColorBomb();
                        }
                    }

                    else
                    {
                        if (currPiece.swapPiece != null)
                        {
                            Piece otherPiece = currPiece.swapPiece.GetComponent<Piece>();
                            if (otherPiece.isMatched)
                            {
                                if (!otherPiece.isColorBomb)
                                {
                                    otherPiece.isMatched = false;
                                    otherPiece.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (currPiece != null)
                {
                    if (currPiece.isMatched)
                    {
                        if (!currPiece.isAdjacentBomb)
                        {
                            currPiece.isMatched = false;
                            currPiece.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currPiece.swapPiece != null)
                        {
                            Piece otherPiece= currPiece.swapPiece.GetComponent<Piece>();
                            if (otherPiece.isMatched)
                            {
                                if (!otherPiece.isAdjacentBomb)
                                {
                                    otherPiece.isMatched = false;
                                    otherPiece.MakeAdjacentBomb();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (GetPiece(column, row).isMatched)
        {
            if (findMatches.currMatches.Count >= 4)
                CheckToMakeBombs();

            if (breakableTiles[column, row] != null)
            {            
                breakableTiles[column, row].TakeDamage(1);

                if (breakableTiles[column, row].hitPoint <= 0)
                    breakableTiles[column, row] = null;
            }

            GameObject particle = Instantiate(destroyParticle,allPieces[column, row].transform.position,Quaternion.identity);
            Destroy(particle, .5f);
            Destroy(allPieces[column, row]);
            allPieces[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allPieces[column, row] != null)
                    DestroyMatchesAt(column, row);
            }
        }

        findMatches.currMatches.Clear();
        StartCoroutine(DecreaseRow());
    }

    private IEnumerator DecreaseRow()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (!blankSpaces[column, row] && allPieces[column, row] == null)
                {
                    for (int k = row + 1; k < height; k++)
                    {
                        if (allPieces[column, k] != null)
                        {
                            allPieces[column, k].GetComponent<Piece>().row = row;
                            allPieces[column, k] = null;
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoard());
    }

   
    private void RefillBoard()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allPieces[column, row] == null && !blankSpaces[column, row])
                {
                    Vector2 startPos = new Vector2(column, row + offSet);

                    int value = Random.Range(0, pieceSprite.Length);

                    GameObject pieceGo = Instantiate(pieceModel, startPos, Quaternion.identity);
                    Piece piece = pieceGo.GetComponent<Piece>();

                    piece.InitPiece(column, row, value);

                    pieceGo.transform.parent = this.transform;
                    pieceGo.name = "( " + column + ", " + row + " )";
                    allPieces[column, row] = pieceGo;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allPieces[column, row] != null)
                {
                    if (allPieces[column, row].GetComponent<Piece>().isMatched)
                        return true;
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoard()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        findMatches.currMatches.Clear();
        currPiece = null;
        yield return new WaitForSeconds(.5f);

        if (IsDeadlocked())
        {
            ShuffleBoard();
            Debug.Log("Deadlocked!!!");
        }

        currState = GameState.move;
    }

    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        GameObject holder = allPieces[column + (int)direction.x, row + (int)direction.y];
        allPieces[column + (int)direction.x, row + (int)direction.y] = allPieces[column, row];
        allPieces[column, row] = holder;
    }

    private bool CheckForMatches()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allPieces[column, row] != null)
                {
                    if (column < width - 2)
                    {
                        if (allPieces[column + 1, row] != null && allPieces[column + 2, row] != null)
                        {
                            if (GetPiece(column + 1, row).value == GetPiece(column, row).value && GetPiece(column + 2, row).value == GetPiece(column, row).value)
                                return true;
                        }
                    }
                    if (row < height - 2)
                    {
                        if (allPieces[column, row + 1] != null && allPieces[column, row + 2] != null)
                        {
                            if (GetPiece(column, row+ 1).value == GetPiece(column, row).value && GetPiece(column, row + 2).value == GetPiece(column, row).value)
                                return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);

        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }

        SwitchPieces(column, row, direction);
        return false;
    }

    private bool IsDeadlocked()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allPieces[column, row] != null)
                {
                    if (column < width - 1)
                    {
                        if (SwitchAndCheck(column, row, Vector2.right))
                            return false;
                    }

                    if (row < height - 1)
                    {
                        if (SwitchAndCheck(column, row, Vector2.up))
                            return false;
                    }
                }
            }
        }

        return true;
    }

    private void ShuffleBoard()
    {
        List<GameObject> newBoard = new List<GameObject>();

        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allPieces[column, row] != null)
                {
                    newBoard.Add(allPieces[column, row]);
                }
            }
        }
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (!blankSpaces[column, row])
                {
                    int pieceToUse = Random.Range(0, newBoard.Count);
                    Piece piece = newBoard[pieceToUse].GetComponent<Piece>();

                    int maxIterations = 0;

                    while (MatchesAt(column, row, piece) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                        Debug.Log(maxIterations);
                    }
                    maxIterations = 0;

                    piece.column = column;
                    piece.row = row;
                    allPieces[column, row] = newBoard[pieceToUse];
                    newBoard.Remove(newBoard[pieceToUse]);

                }
            }
        }

        if (IsDeadlocked())
            ShuffleBoard();
    }
}