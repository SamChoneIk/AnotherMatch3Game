using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    wait,
    move
}

public enum TileKind
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
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{
<<<<<<< Updated upstream
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
=======
    public GameState currState = GameState.move;
    [Header("Board offset")]
    public int width;
    public int height;
    public int offSet;

    [Header("Board Prefab")]
    public GameObject piecePrefab;
    public Sprite[] pieceSprite;
    public GameObject breakableTilePrefab;
    public GameObject destroyParticle;

    [Header("Board")]
    public TileType[] boardLayout;
    public Piece currPiece;
    [HideInInspector]
    public GameObject[,] allPieces;

    private List<GameObject> pieces;
    private FindMatches findMatches;
    private Tile[,] breakableTiles;

    private bool[,] blankSpaces;

    void Start()
    {
        findMatches = GetComponent<FindMatches>();
        pieces = new List<GameObject>();

        allPieces = new GameObject[width, height];

        breakableTiles = new Tile[width, height];
        blankSpaces = new bool[width, height];
        
        SetUp();
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Blank)
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
        }
>>>>>>> Stashed changes
    }

    public void GenerateBreakableTiles()
    {
        // 보드에 생성한 타일을 순회
        for (int i = 0; i < boardLayout.Length; i++)
        {
            // Breakable 타입의 타일일때 타일 생성
            if (boardLayout[i].tileKind == TileKind.Breakable)
            {
<<<<<<< Updated upstream
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
=======
                Vector2 startPos = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, startPos, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<Tile>();
            }
        }
    }

    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();

        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (!blankSpaces[column, row])
                {
                    Vector2 startPos = new Vector2(column, row + offSet);
                    int value = Random.Range(0, pieceSprite.Length);

                    GameObject pieceGo = Instantiate(piecePrefab, startPos, Quaternion.identity);
                    Piece piece = pieceGo.GetComponent<Piece>();
                    piece.InitPiece(column, row, value);

                    SpriteRenderer sprite = pieceGo.GetComponent<SpriteRenderer>();
                    sprite.sprite = pieceSprite[value];

                    MatchePiecesValue(column, row, piece);

                    piece.transform.parent = transform;
                    piece.name = "( " + column + ", " + row + " )";

                    allPieces[column, row] = pieceGo;
                }
>>>>>>> Stashed changes
            }
        }
    }

<<<<<<< Updated upstream
    public Block GetBlock(int column, int row)
    {
        return allPieces[column, row].GetComponent<Block>();
    }

    // 같은 색상 블럭 검사
    private bool MatchesAt(int column, int row, Block piece)
=======
    private void MatchePiecesValue(int column, int row, Piece piece)
>>>>>>> Stashed changes
    {
        List<GameObject> changed = new List<GameObject>();

        Vector2[] direction =
        {
            Vector2.up,
            Vector2.right,
            Vector2.down,
            Vector2.right
        };

        foreach (var dir in direction)
        {
            int equals = 0;

            for (int i = 1; i < 3; ++i)
            {
<<<<<<< Updated upstream
                if (GetBlock(column - 1, row).value == piece.value && GetBlock(column - 2, row).value == piece.value)
=======
                if (allPieces[column + (int)dir.x * i, row + (int)dir.y * i] == null)
                    break;

                if (IsValueEqualAtPieces(column, row, piece, dir * i))
>>>>>>> Stashed changes
                {
                    ++equals;

                    if (equals >= 2)
                    {
                        changed.Add(allPieces[column + (int)dir.x * i, row + (int)dir.y * i]);
                    }
                }
            }
        }
            ChangedPiecesValue(changed);
    }

    private void ChangedPiecesValue(List<GameObject> changed)
    {
        foreach(var p in changed)
        {
            Piece piece = p.GetComponent<Piece>();
            int value = 0;

            while(value == piece.value)
            {
<<<<<<< Updated upstream
                if (GetBlock(column, row - 1).value == piece.value && GetBlock(column, row - 2).value == piece.value)
                {
                    return true;
                }
=======
                value = Random.Range(0, pieceSprite.Length);
>>>>>>> Stashed changes
            }

            piece.value = value;
        }

        changed.Clear();
    }

    public Piece GetPiece(int column, int row)
    {
        return allPieces[column, row].GetComponent<Piece>();
    }

    private bool IsValueEqualAtPieces(int column, int row, Piece piece, Vector2 direction)
    {
        return GetPiece(column + (int)direction.x, row + (int)direction.y).value == piece.value;
    }

    // 블럭을 제거한 만큼 아이템 생성
    private void CheckToMakeBombs()
    {
        if (findMatches.currMatches.Count == 4 ||
            findMatches.currMatches.Count == 7)
            findMatches.CheckBombs();

        if (findMatches.currMatches.Count == 5 ||
            findMatches.currMatches.Count == 8)
        {
            if (ColumnOrRow())
            {
                //Make a color bomb
                //is the current dot matched?
                if (currPiece != null)
                {
<<<<<<< Updated upstream
                    if (GetBlock(column, row - 1).value == piece.value && GetBlock(column, row - 2).value == piece.value)
=======
                    if (currPiece.isMatched)
>>>>>>> Stashed changes
                    {
                        if (!currPiece.isColorBomb)
                        {
                            currPiece.isMatched = false;
                            currPiece.MakeColorBomb();
                        }
                    }
                    else
                    {
                        if (currPiece.otherPiece != null)
                        {
                            Piece otherPiece = currPiece.otherPiece.GetComponent<Piece>();
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
                //Make a adjacent bomb
                //is the current dot matched?
                if (currPiece != null)
                {
<<<<<<< Updated upstream
                    if (GetBlock(column - 1, row).value == piece.value && GetBlock(column - 2, row).value == piece.value)
=======
                    if (currPiece.isMatched)
>>>>>>> Stashed changes
                    {
                       if (!currPiece.isAdjacentBomb)
                        {
                            currPiece.isMatched = false;
                            currPiece.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currPiece.otherPiece != null)
                        {
                            Piece otherPiece = currPiece.otherPiece.GetComponent<Piece>();
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

    private bool ColumnOrRow()
    {
        int h = 0;
        int v = 0;
        Piece firstPiece = findMatches.currMatches[0].GetComponent<Piece>();

        if (firstPiece != null)
        {
            foreach (GameObject currentPiece in findMatches.currMatches)
            {
                Piece piece = currentPiece.GetComponent<Piece>();
                if (piece.row == firstPiece.row)
                {
                    h++;
                }

                if (piece.column == firstPiece.column)
                {
                    v++;
                }
            }
        }
        return (v == 5 || h == 5);
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allPieces[column, row].GetComponent<Block>().isMatched)
        {
<<<<<<< Updated upstream
=======
            //How many elements are in the matched pieces list from findmatches?
            if (findMatches.currMatches.Count >= 4)
            {
                CheckToMakeBombs();
            }

            // Does a tile need to break?
            if (breakableTiles[column, row] != null)
            {
                // if it does, give one damage.                 
                // breakableTiles[column, row].TakeDamage(1);
                if (breakableTiles[column, row].hitPoint <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }

            GameObject particle = Instantiate(destroyParticle,allPieces[column, row].transform.position, Quaternion.identity);
            Destroy(particle, 0.5f);
>>>>>>> Stashed changes
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
                {
                    DestroyMatchesAt(column, row);
                }
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
                //if the current spot isn't blank and is empty. . . 
                if (!blankSpaces[column, row] && allPieces[column, row] == null)
                {
<<<<<<< Updated upstream
                    nullCount++;
                }

                else if (nullCount > 0)
                {
                    allPieces[column, row].GetComponent<Block>().row -= nullCount;
                    allPieces[column, row] = null;
                }
            }

            nullCount = 0;
=======
                    //loop from the space above to the top of the column
                    for (int k = row + 1; k < height; k++)
                    {
                        //if a dot is found. . .
                        if (allPieces[column, k] != null)
                        {
                            //move that dot to this empty space
                            allPieces[column, k].GetComponent<Piece>().row = row;
                            //set that spot to be null
                            allPieces[column, k] = null;
                            //break out of the loop;
                            break;
                        }
                    }
                }
            }
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
                    Vector2 tempPos = new Vector2(column, row + offSet);
                    int color = Random.Range(0, pieces.Length);

                    GameObject go = Instantiate(piecePrefab, tempPos, Quaternion.identity);

                    go.transform.parent = transform;
                    go.name = "[ " + column + " , " + row + " ] ";

                    allPieces[column, row] = go;
                    go.GetComponent<Block>().row = row;
                    go.GetComponent<Block>().column = column;
=======
                    Vector2 startPos = new Vector2(column, row + offSet);
                    int value = Random.Range(0, pieceSprite.Length);
                    GameObject pieceGo = Instantiate(piecePrefab, startPos, Quaternion.identity);
                    Piece piece = pieceGo.GetComponent<Piece>();
                    allPieces[column, row] = pieceGo;
                    piece.InitPiece(column, row, value);
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
                    if (allPieces[column, row].GetComponent<Block>().isMatched)
=======
                    if (allPieces[column, row].GetComponent<Piece>().isMatched)
                    {
>>>>>>> Stashed changes
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoard()
    {
        RefillBoard();
        yield return new WaitForSeconds(0.5f);

        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }

<<<<<<< Updated upstream
        yield return new WaitForSeconds(0.5f);
=======
        findMatches.currMatches.Clear();
        currPiece = null;
        yield return new WaitForSeconds(0.5f);

        if (IsDeadlocked())
        {
            ShuffleBoard();
            Debug.Log("Deadlocked!!!");
        }

>>>>>>> Stashed changes
        currState = GameState.move;

    }

    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        // Take the second piece and save it in a holder
        GameObject holder = allPieces[column + (int)direction.x, row + (int)direction.y];
        // switching the first dot to be the second position
        allPieces[column + (int)direction.x, row + (int)direction.y] = allPieces[column, row];
        // Set the first dot to be the second dot
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
                    // make sure that one and two to the right are in the 
                    // board
                    if (column < width - 2)
                    {
                        // check if the pieceSprite to the right and two to the right exist
                        if (allPieces[column + 1, row] != null && allPieces[column + 2, row] != null)
                        {
                            if (IsValueEqualAtPieces(column, row, GetPiece(column, row), Vector2.right) && 
                                IsValueEqualAtPieces(column, row, GetPiece(column, row), (Vector2.right * 2)))
                            {
                                return true;
                            }
                        }
                    }
                    if (row < height - 2)
                    {
                        // check if the pieceSprite above exist
                        if (allPieces[column, row + 1] != null && allPieces[column, row + 2] != null)
                        {
                            if (IsValueEqualAtPieces(column, row, GetPiece(column, row), Vector2.up) &&
                                IsValueEqualAtPieces(column, row, GetPiece(column, row), (Vector2.up * 2)))
                            {
                                return true;
                            }
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
                        {
                            return false;
                        }
                    }
                    if (row < height - 1)
                    {
                        if (SwitchAndCheck(column, row, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private void ShuffleBoard()
    {
        // Create a list of game object
        List<GameObject> newBoard = new List<GameObject>();
        // Add every Piece to this list
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
        // for every spot on the board...
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                // if this spot shouldn't be blank
                if (!blankSpaces[column, row])
                {
                    // Pick a random Range
                    int value = Random.Range(0, newBoard.Count);

                    // Make a container for the piece
                    Piece piece = newBoard[value].GetComponent<Piece>();

                   //int maxIterations = 0;

                    //while (MatchesAt(column, row, piece) && maxIterations < 100)
                    //{
                   //     value = Random.Range(0, newBoard.Count);
                    //    maxIterations++;
                    //    Debug.Log(maxIterations);
                   //}
                    //maxIterations = 0;

                    // Assign the column to the piece
                    piece.column = column;
                    // Assign the row to the piece
                    piece.row = row;
                    // Fill in the pieceSprite array with this new Piece
                    allPieces[column, row] = newBoard[value];
                    // Remove it from the list
                    newBoard.Remove(newBoard[value]);

                }
            }
        }
        // Check if it's still deadlocked
        if (IsDeadlocked())
        {
            ShuffleBoard();
        }
    }
}