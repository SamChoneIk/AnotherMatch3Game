using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Board Variables")]
    public int value = 0;
    public int column; // 현재 가로 위치
    public int row; // 현재 세로 위치
    public int prevColumn; // 이전 가로 위치
    public int prevRow; // 이전 세로 위치
    public int targetX; // 가로 비교값
    public int targetY; // 세로 비교값
    public bool isMatched = false; // 매치되었는지 확인

    private Board board; // 현재 게임의 블럭들을 저장
    private GameObject otherPiece; // 위치를 바꾼 블럭
    private Vector2 firstTouchPosition; // 초기 마우스 좌표
    private Vector2 finalTouchPosition; // 마지막 마우스 좌표
<<<<<<< Updated upstream:Match3Project/Assets/Scripts/Block.cs
    private Vector2 movePos; // 이동할 좌표를 저장
    public float swipeAngle = 0; // 누른 방향으로 각도 계산
    public float swipeResist = 1f; // 누른 상태에서 

    public void Init(int r, int c, int v)
    {
        board = FindObjectOfType<Board>();
        row = r;
        column = c;
        value = v;
    }

    public void ChangedPieceValue(int v)
    {
        value = v;
    }

    /*void Start()
    {
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //row = targetY;
        //column = targetX;
        //prevRow = row;
        //prevColumn = column;
    }*/

    void Update()
    {
        FindMatches();

        // 블럭이 매치되었을 때
        if(isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1f, 1f, 1f, .2f);
        }
=======
    private Vector2 movePos; // 이동할 좌표를 저장

    [Header("Swipe Stuff")]
    public float swipeAngle = 0; // 누른 방향으로 각도 계산
    public float swipeResist = 1f; // 누른 상태에서 

    [Header("Powerup Stuff")]
    public bool isColumnBomb; 
    public bool isRowBomb;
    public bool isColorBomb;
    public bool isAdjacentBomb;

    public GameObject columnArrow;
    public GameObject rowArrow; 
    public GameObject colorBomb;
    public GameObject adjacentMarker;

    public void InitPiece(int c, int r, int v)
    {
        column = c;
        row = r;
        value = v;

        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
    }

    // 아이템 테스트
    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(1))
        {
            isColumnBomb = true;
            GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }

>>>>>>> Stashed changes:Match3Project/Assets/Scripts/Piece.cs

    private void PieceMoving()
    {
        // 위치를 변경
        targetX = column;
        targetY = row;

        // 좌우
        if (Mathf.Abs(targetX - transform.position.x) > 0.1)
        {
            // 대상으로 이동
            movePos = new Vector2(targetX, transform.position.y);
            // 이동하는 움직임을 표현
<<<<<<< Updated upstream:Match3Project/Assets/Scripts/Block.cs
            transform.position = Vector2.Lerp(transform.position, movePos, 0.6f);
            if(board.allPieces[column, row] != this.gameObject)
=======
            transform.position = Vector2.Lerp(transform.position, movePos, 0.6f);

            if (board.allPieces[column, row] != this.gameObject)
>>>>>>> Stashed changes:Match3Project/Assets/Scripts/Piece.cs
                board.allPieces[column, row] = this.gameObject;
        }

        else
        {
            // 위치를 변경
            movePos = new Vector2(targetX, transform.position.y);
            transform.position = movePos;
        }

        // 상하
        if (Mathf.Abs(targetY - transform.position.y) > 0.1)
        {
            movePos = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, movePos, 0.6f);
            if (board.allPieces[column, row] != this.gameObject)
                board.allPieces[column, row] = this.gameObject;
<<<<<<< Updated upstream:Match3Project/Assets/Scripts/Block.cs
=======
        }

        else
        {
            movePos = new Vector2(transform.position.x, targetY);
            transform.position = movePos;
        }
    }

    private void MovePiecesActual(Vector2 direction)
    {
        otherPiece = board.allPieces[row + (int)direction.x, column + (int)direction.y];
        Piece movedPiece = otherPiece.GetComponent<Piece>();
        prevRow = row;
        prevColumn = column;
        if (otherPiece != null)
        {
            movedPiece.row += -1 * (int)direction.x;
            movedPiece.column += -1 * (int)direction.y;
            column += (int)direction.x;
            row += (int)direction.y;
            PieceMoving();
            PiecesCheck();
>>>>>>> Stashed changes:Match3Project/Assets/Scripts/Piece.cs
        }

        else
        {
            board.currState = GameState.move;
        }
    }

    public IEnumerator PiecesCheck()
    {
        yield return new WaitForSeconds(0.1f);

        // 매치가 안된 블럭은 다시 되돌린다.
        if (otherPiece != null)
        {
<<<<<<< Updated upstream:Match3Project/Assets/Scripts/Block.cs
            // 매치가 안된 블럭은 다시 되돌린다.
            if(!isMatched && !otherPiece.GetComponent<Block>().isMatched)
=======
            if(!isMatched && !otherPiece.GetComponent<Piece>().isMatched)
>>>>>>> Stashed changes:Match3Project/Assets/Scripts/Piece.cs
            {
                otherPiece.GetComponent<Block>().row = row;
                otherPiece.GetComponent<Block>().column = column;
                row = prevRow;
                column = prevColumn;

<<<<<<< Updated upstream:Match3Project/Assets/Scripts/Block.cs
                // 현재 게임 상태를 활성화한다.
                yield return new WaitForSeconds(0.5f);
=======
                board.currPiece = null;
>>>>>>> Stashed changes:Match3Project/Assets/Scripts/Piece.cs
                board.currState = GameState.move;
            }

            else
            {
                board.DestroyMatches();
            }
<<<<<<< Updated upstream:Match3Project/Assets/Scripts/Block.cs

            otherPiece = null;
=======
>>>>>>> Stashed changes:Match3Project/Assets/Scripts/Piece.cs
        }
    }

    private void OnMouseDown()
    {
        if (board.currState == GameState.move)
        {
            Debug.Log("asdsa");
            // 클릭한 기점으로 마우스 좌표 저장
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currState == GameState.move)
        {
            Debug.Log("asdsa");
            // 놓았을 때 기점으로 마우스 좌표 저장
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    // 방향 값
    private void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist ||
            Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            // degree 각도로 변환
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, 
                finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;

            //Debug.Log(swipeAngle);
            MovePieces();
            board.currState = GameState.wait;
        }

        else
        {
            board.currState = GameState.move;
        }
    }

    private void MovePieces()
    {
        // 오른쪽
<<<<<<< Updated upstream:Match3Project/Assets/Scripts/Block.cs
        if((swipeAngle > -45 && swipeAngle <= 45) && column < board.width - 1)
        {
            otherPiece = board.allPieces[column + 1, row];
            prevRow = row;
            prevColumn = column;
            otherPiece.GetComponent<Block>().column -= 1;
            column += 1;
        }

        // 위
        else if ((swipeAngle > 45 && swipeAngle <= 135) && row < board.height - 1)
        {
            otherPiece = board.allPieces[column, row + 1];
            prevRow = row;
            prevColumn = column;
            otherPiece.GetComponent<Block>().row -= 1;
            row += 1;
        }

        // 왼쪽
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            otherPiece = board.allPieces[column - 1, row];
            prevRow = row;
            prevColumn = column;
            otherPiece.GetComponent<Block>().column += 1;
            column -= 1;
        }

        // 밑
        else if ((swipeAngle < -45 && swipeAngle >= -135) && row > 0)
        {
            otherPiece = board.allPieces[column, row - 1];
            prevRow = row;
            prevColumn = column;
            otherPiece.GetComponent<Block>().row += 1;
            row -= 1;
        }
=======
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
            MovePiecesActual(Vector2.right);

        // 위
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
            MovePiecesActual(Vector2.up);

        // 왼쪽
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
            MovePiecesActual(Vector2.left);

        // 아래
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
            MovePiecesActual(Vector2.down);
>>>>>>> Stashed changes:Match3Project/Assets/Scripts/Piece.cs

        // 입력값이 없을때 다시 재개
        else
            board.currState = GameState.move;
    }

<<<<<<< Updated upstream:Match3Project/Assets/Scripts/Block.cs
    private void FindMatches()
=======
    

    /*private void FindMatches()
>>>>>>> Stashed changes:Match3Project/Assets/Scripts/Piece.cs
    {
        // 열에 해당하는 블럭을 검사
        if(column > 0 && column < board.width -1)
        {
            Block leftBlock1 = board.GetBlock(column - 1, row);
            Block rightBlock1 = board.GetBlock(column + 1, row);
            if (leftBlock1 != null && rightBlock1 != null)
            {
                if (leftBlock1.value == value && rightBlock1.value == value)
                {
                    leftBlock1.GetComponent<Block>().isMatched = true;
                    rightBlock1.GetComponent<Block>().isMatched = true;
                    isMatched = true;
                }
            }
        }

        // 행에 해당하는 블럭을 검사
        if (row > 0 && row < board.height - 1)
        {
            Block upBlock1 = board.GetBlock(column, row + 1);
            Block downBlock1 = board.GetBlock(column, row - 1);
            if (upBlock1 != null && downBlock1 != null)
            {
                if (upBlock1.value == value && downBlock1.value == value)
                {
                    upBlock1.GetComponent<Block>().isMatched = true;
                    downBlock1.GetComponent<Block>().isMatched = true;
                    isMatched = true;
                }
            }
        }
<<<<<<< Updated upstream:Match3Project/Assets/Scripts/Block.cs
=======
    }*/

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
>>>>>>> Stashed changes:Match3Project/Assets/Scripts/Piece.cs
    }
}
