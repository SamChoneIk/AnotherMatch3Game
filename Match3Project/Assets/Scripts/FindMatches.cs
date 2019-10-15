using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board board;
    public List<GameObject> currMatches = new List<GameObject>();

    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchedPiece());
    }

    private IEnumerator FindAllMatchedPiece()
    {
        //yield return new WaitForSeconds(0.1f);
        yield return null;
        for (int coulmn = 0; coulmn < board.width; coulmn++)
        {
            for (int row = 0; row < board.height; row++)
            {
                GameObject currIndex = board.allPieces[coulmn, row];

                if (currIndex != null)
                {
                    Piece currPiece = currIndex.GetComponent<Piece>();

                    if (coulmn > 0 && coulmn < board.width - 1)
                    {
                        GameObject leftIndex = board.allPieces[coulmn - 1, row];
                        GameObject rightIndex = board.allPieces[coulmn + 1, row];

                        if (leftIndex != null && rightIndex != null)
                        {
                            Piece rightPiece = rightIndex.GetComponent<Piece>();
                            Piece leftPiece = leftIndex.GetComponent<Piece>();

                            if (leftPiece.value == currPiece.value && rightPiece.value == currPiece.value)
                            {
                                currMatches.Union(IsRowBomb(leftPiece, currPiece, rightPiece));
                                currMatches.Union(IsColumnBomb(leftPiece, currPiece, rightPiece));
                                currMatches.Union(IsAdjacentBomb(leftPiece, currPiece, rightPiece));

                                GetNearbyPieces(leftIndex, currIndex, rightIndex);
                            }
                        }
                    }

                    if (row > 0 && row < board.height - 1)
                    {
                        GameObject upIndex = board.allPieces[coulmn, row + 1];
                        GameObject downIndex = board.allPieces[coulmn, row - 1];

                        if (upIndex != null && downIndex != null)
                        {
                            Piece upPiece = upIndex.GetComponent<Piece>();
                            Piece downPiece = downIndex.GetComponent<Piece>();
                            if (upPiece.value == currPiece.value && downPiece.value == currPiece.value)
                            {
                                currMatches.Union(IsColumnBomb(upPiece, currPiece, downPiece));
                                currMatches.Union(IsRowBomb(upPiece, currPiece, downPiece));
                                currMatches.Union(IsAdjacentBomb(upPiece, currPiece, downPiece));
                                GetNearbyPieces(upIndex, currIndex, downIndex);
                            }
                        }
                    }
                }
            }
        }
    }

    private void GetNearbyPieces(GameObject piece1, GameObject piece2, GameObject piece3)
    {
        AddToListAndMatch(piece1);
        AddToListAndMatch(piece2);
        AddToListAndMatch(piece3);
    }

    private void AddToListAndMatch(GameObject piece)
    {
        if (!currMatches.Contains(piece))
            currMatches.Add(piece);

        piece.GetComponent<Piece>().isMatched = true;
    }

    private List<GameObject> IsAdjacentBomb(Piece piece1, Piece piece2, Piece piece3)
    {
        List<GameObject> currPieces = new List<GameObject>();

        if (piece1.isAdjacentBomb)
            currMatches.Union(GetAdjacentPieces(piece1.column, piece1.row));
        if (piece2.isAdjacentBomb)
            currMatches.Union(GetAdjacentPieces(piece2.column, piece2.row));
        if (piece3.isAdjacentBomb)
            currMatches.Union(GetAdjacentPieces(piece3.column, piece3.row));

        return currPieces;
    }

    private List<GameObject> IsRowBomb(Piece piece1, Piece piece2, Piece piece3)
    {
        List<GameObject> currPieces = new List<GameObject>();

        if (piece1.isRowBomb)
            currMatches.Union(GetRowPieces(piece1.row));
        if (piece2.isRowBomb)
            currMatches.Union(GetRowPieces(piece2.row));
        if (piece3.isRowBomb)
            currMatches.Union(GetRowPieces(piece3.row));

        return currPieces;
    }

    private List<GameObject> IsColumnBomb(Piece piece1, Piece piece2, Piece piece3)
    {
        List<GameObject> currPieces = new List<GameObject>();

        if (piece1.isColumnBomb)
            currMatches.Union(GetColumnPieces(piece1.column));
        if (piece2.isColumnBomb)
            currMatches.Union(GetColumnPieces(piece2.column));
        if (piece3.isColumnBomb)
            currMatches.Union(GetColumnPieces(piece3.column));

        return currPieces;
    }


    public void MatchPiecesOfColor(int value)
    {
        for (int column = 0; column < board.width; column++)
        {
            for (int row = 0; row < board.height; row++)
            {
                // 블럭이 존재하는 지 확인
                if (board.allPieces[column, row] != null)
                {
                    // 블럭의 값을 확인
                    if (board.GetPiece(column, row).value == value)
                    {
                        // 블럭이 일치하면 매치확인
                        board.GetPiece(column, row).isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetAdjacentPieces(int c, int r)
    {
        List<GameObject> pieces = new List<GameObject>();

        for (int column = c - 1; column <= c + 1; column++)
        {
            for (int row = r - 1; row <= r + 1; row++)
            {
                // 보드 안에 해당 블럭이 있는지 확인
                if (column >= 0 && column < board.width && row >= 0 && row < board.height)
                {
                    pieces.Add(board.allPieces[column, row]);
                    board.allPieces[column, row].GetComponent<Piece>().isMatched = true;
                }
            }
        }
        return pieces;
    }

    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> pieces = new List<GameObject>();
        for (int row = 0; row < board.height; row++)
        {
            if (board.allPieces[column, row] != null)
            {
                pieces.Add(board.allPieces[column, row]);
                board.allPieces[column, row].GetComponent<Piece>().isMatched = true;
            }
        }
        return pieces;
    }

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> pieces = new List<GameObject>();

        for (int column = 0; column < board.width; column++)
        {
            if (board.allPieces[column, row] != null)
            {
                pieces.Add(board.allPieces[column, row]);
                board.allPieces[column, row].GetComponent<Piece>().isMatched = true;
            }
        }
        return pieces;
    }

    public void CheckBombs()
    {
        if (board.currPiece != null)
        {
            // 움직인 블럭이 일치 할때
            if (board.currPiece.isMatched)
            {
                //make it unmatched
                board.currPiece.isMatched = false;
                // 아이템 생성
                if ((board.currPiece.swipeAngle > -45 && board.currPiece.swipeAngle <= 45)|| (board.currPiece.swipeAngle < -135 || board.currPiece.swipeAngle >= 135))
                    board.currPiece.MakeRowBomb();

                else
                    board.currPiece.MakeColumnBomb();
            }

            // 선택한 블럭과 위치를 바꾼 블럭을 확인
            else if (board.currPiece.swapPiece != null)
            {
                Piece swapPiece = board.currPiece.swapPiece.GetComponent<Piece>();

                if (swapPiece.isMatched)
                {
                    //Make it unmatched
                    swapPiece.isMatched = false;

                    if ((board.currPiece.swipeAngle > -45 && board.currPiece.swipeAngle <= 45)
                   || (board.currPiece.swipeAngle < -135 || board.currPiece.swipeAngle >= 135))
                        swapPiece.MakeRowBomb();

                    else
                        swapPiece.MakeColumnBomb();
                }
            }
        }
    }
}