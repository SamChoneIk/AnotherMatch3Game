using System.Collections;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int value;

    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    //private HintManager HintManager;
    private FindMatches findMatches;
    private Board board;
    public GameObject swapPiece;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 startPos;

    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject adjacentMarker;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;

    public void InitPiece(int c, int r, int v)
    {
        column = c;
        row = r;
        value = v;

        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        //HintManager = FindObjectOfType<HintManager>();
        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
    }

    // 아이템 생성 디버그
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }

    void Update()
    {
        if (board.currState == GameState.move)
            return;

        targetX = column;
        targetY = row;

        if (Mathf.Abs(targetX - transform.position.x) > 0.1)
        {
            // 대상을 향해 이동
            startPos = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, startPos, 0.6f);

           if (board.allPieces[column, row] != this.gameObject)
               board.allPieces[column, row] = this.gameObject;

            findMatches.FindAllMatches();
        }

        else
        {
            // 위치 설정
            startPos = new Vector2(targetX, transform.position.y);
            transform.position = startPos;
        }

        if (Mathf.Abs(targetY - transform.position.y) > 0.1)
        {
            // 대상을 향해 이동
            startPos = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, startPos, 0.6f);

            if (board.allPieces[column, row] != this.gameObject)
                board.allPieces[column, row] = this.gameObject;

            findMatches.FindAllMatches();
        }

        else
        {
            // 위치 설정
            startPos = new Vector2(transform.position.x, targetY);
            transform.position = startPos;
        }
    }

    

    private void OnMouseDown()
    {
        // Destroy the hint
        //if (HintManager != null)
       //{
        //    HintManager.DestroyHint();
        //}

        if (board.currState == GameState.move)
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (board.currState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        // 해당 방향으로 끌고 갔을때
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();

            board.currPiece = this;
        }

        else
            board.currState = GameState.move;
    }

    void MovePieces()
    {
        //Right Swipe
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
            MovePiecesActual(Vector2.right);

        //Up Swipe
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
            MovePiecesActual(Vector2.up);

        //Left Swipe
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
            MovePiecesActual(Vector2.left);

        //Down Swipe
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
            MovePiecesActual(Vector2.down);

        else
            board.currState = GameState.move;
    }

    void MovePiecesActual(Vector2 direction)
    {
        swapPiece = board.allPieces[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;

        if (swapPiece != null)
        {
            swapPiece.GetComponent<Piece>().column += -1 * (int)direction.x;
            swapPiece.GetComponent<Piece>().row += -1 * (int)direction.y;
            column += (int)direction.x;
            row += (int)direction.y;
            StartCoroutine(CheckMove());
        }

        else
            board.currState = GameState.move;
    }

    public IEnumerator CheckMove()
    {
        Piece swapedPiece = swapPiece.GetComponent<Piece>();
        if (isColorBomb)
        {
            // 블럭이 ColorBomb일때
            findMatches.MatchPiecesOfColor(swapedPiece.value);
            isMatched = true;
        }

        else if (swapedPiece.isColorBomb)
        {
            // 다른 블럭이 ColorBomb일때
            findMatches.MatchPiecesOfColor(value);
            swapedPiece.isMatched = true;
        }

        yield return new WaitForSeconds(0.5f);

        if (swapPiece != null)
        {
            if (!isMatched && !swapPiece.GetComponent<Piece>().isMatched)
            {
                swapedPiece.row = row;
                swapedPiece.column = column;
                row = previousRow;
                column = previousColumn;

                //yield return new WaitForSeconds(0.5f);

                board.currPiece = null;
                board.currState = GameState.move;
            }

            else
                board.DestroyMatches();
        }
    }

    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftIndex = board.allPieces[column - 1, row];
            GameObject rightIndex = board.allPieces[column + 1, row];
            Piece leftPiece = leftIndex.GetComponent<Piece>();
            Piece rightPiece = rightIndex.GetComponent<Piece>();

            if (leftIndex != null && rightIndex != null)
            {
                if (leftPiece.value == value && rightPiece.value == value)
                {
                    leftPiece.isMatched = true;
                    rightPiece.isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject upIndex = board.allPieces[column, row + 1];
            GameObject downIndex = board.allPieces[column, row - 1];
            Piece upPiece = upIndex.GetComponent<Piece>();
            Piece downPiece = downIndex.GetComponent<Piece>();

            if (upIndex != null && downIndex != null)
            {
                if (upPiece.value == value && downPiece.value == value)
                {
                    upPiece.isMatched = true;
                    downPiece.isMatched = true;
                    isMatched = true;
                }
            }
        }
    }

    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColumnBomb()
    {
        isColumnBomb = true;
        GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
        color.transform.parent = this.transform;
    }

    public void MakeAdjacentBomb()
    {
        isAdjacentBomb = true;
        GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
        marker.transform.parent = this.transform;
    }
}