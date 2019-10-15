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

    private void FindAllMatches()
    {
        for (int column = 0; column < board.width; column++)
        {
            for (int row = 0; row < board.height; row++)
            {
                GameObject currIndex = board.allPieces[column, row];

                if (currIndex != null)
                {
                    Piece currPiece = currIndex.GetComponent<Piece>();

                    if (column > 0 && column < board.width - 1)
                    {
                        GameObject leftIndex = board.allPieces[column - 1, row];
                        GameObject rightIndex = board.allPieces[column + 1, row];

                        if (leftIndex != null && rightIndex != null)
                        {
                            Piece rightPiece = rightIndex.GetComponent<Piece>();
                            Piece leftPiece = leftIndex.GetComponent<Piece>();
                            if (leftPiece.value == currPiece.value && leftPiece.value == currPiece.value)
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
                        GameObject upPiece = board.allPieces[column, row + 1];
                        GameObject downPiece = board.allPieces[column, row - 1];

                        if (upPiece != null && downPiece != null)
                        {
                            Piece downPiecePiece = downPiece.GetComponent<Piece>();
                            Piece upPiecePiece = upPiece.GetComponent<Piece>();

                            if (upPiecePiece.value == currPiece.value && downPiecePiece.value == currPiece.value)
                            {
                                currMatches.Union(IsColumnBomb(upPiecePiece, currPiece, downPiecePiece));
                                currMatches.Union(IsRowBomb(upPiecePiece, currPiece, downPiecePiece));
                                currMatches.Union(IsAdjacentBomb(upPiecePiece, currPiece, downPiecePiece));
                                GetNearbyPieces(upPiece, currIndex, downPiece);
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
        {
            currMatches.Add(piece);
        }

        piece.GetComponent<Piece>().isMatched = true;
    }

    /*public void MatchPiecesOfColor(string color)
    {
        for (int column = 0; column < board.width; column++)
        {
            for (int j = 0; j < board.height; j++)
            {
                //Check if that piece exists
                if (board.allPieces[column, j] != null)
                {
                    //Check the tag on that dot
                    if (board.allPieces[column, j].tag == color)
                    {
                        //Set that dot to be matched
                        board.allPieces[column, j].GetComponent<Piece>().isMatched = true;
                    }
                }
            }
        }
    }*/

    public void CheckBombs()
    {
        //Did the player move something?
        if (board.currPiece != null)
        {
            //Is the piece they moved matched?
            if (board.currPiece.isMatched)
            {
                //make it unmatched
                board.currPiece.isMatched = false;
                //Decide what kind of bomb to make
                if ((board.currPiece.swipeAngle > -45 && board.currPiece.swipeAngle <= 45)
                   || (board.currPiece.swipeAngle < -135 || board.currPiece.swipeAngle >= 135))
                {
                    board.currPiece.MakeRowBomb();
                }

                else
                {
                    board.currPiece.MakeColumnBomb();
                }
            }
            //Is the other piece matched?
            else if (board.currPiece.otherPiece != null)
            {
                Piece otherPiece = board.currPiece.otherPiece.GetComponent<Piece>();
                //Is the other Piece matched?
                if (otherPiece.isMatched)
                {
                    //Make it unmatched
                    otherPiece.isMatched = false;
                    if ((board.currPiece.swipeAngle > -45 && board.currPiece.swipeAngle <= 45)
                   || (board.currPiece.swipeAngle < -135 || board.currPiece.swipeAngle >= 135))
                    {
                        otherPiece.MakeRowBomb();
                    }
                    else
                    {
                        otherPiece.MakeColumnBomb();
                    }
                }
            }
        }
    }

    private List<GameObject> IsAdjacentBomb(Piece piece1, Piece piece2, Piece piece3)
    {
        List<GameObject> currentPieces = new List<GameObject>();

        if (piece1.isAdjacentBomb)
            currMatches.Union(GetAdjacentPieces(piece1.column, piece1.row));

        if (piece2.isAdjacentBomb)
            currMatches.Union(GetAdjacentPieces(piece2.column, piece2.row));

        if (piece3.isAdjacentBomb)
            currMatches.Union(GetAdjacentPieces(piece3.column, piece3.row));

        return currentPieces;
    }

    private List<GameObject> IsRowBomb(Piece piece1, Piece piece2, Piece piece3)
    {
        List<GameObject> currentPieces = new List<GameObject>();
        if (piece1.isRowBomb)
            currMatches.Union(GetRowPieces(piece1.row));

        if (piece2.isRowBomb)
            currMatches.Union(GetRowPieces(piece2.row));

        if (piece3.isRowBomb)
            currMatches.Union(GetRowPieces(piece3.row));

        return currentPieces;
    }

    private List<GameObject> IsColumnBomb(Piece piece1, Piece piece2, Piece piece3)
    {
        List<GameObject> currentPieces = new List<GameObject>();
        if (piece1.isColumnBomb)
            currMatches.Union(GetColumnPieces(piece1.column));

        if (piece2.isColumnBomb)
            currMatches.Union(GetColumnPieces(piece2.column));

        if (piece3.isColumnBomb)
            currMatches.Union(GetColumnPieces(piece3.column));

        return currentPieces;
    }

    List<GameObject> GetAdjacentPieces(int c, int r)
    {
        List<GameObject> pieces = new List<GameObject>();

        for (int column = c - 1; column <= c + 1; column++)
        {
            for (int row = r - 1; row <= r + 1; row++)
            {
                //Check if the piece is inside the board
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
}