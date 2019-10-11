using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board board; // 현재 게임의 보드
    public List<GameObject> currMatches = new List<GameObject>(); // 매치된 블럭을 저장

    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindMatche());
    }

    private IEnumerator FindMatche()
    {
        yield return new WaitForSeconds(0.2f);

        for (int column = 0; column < board.width; ++column)
        {
            for (int row = 0; row < board.height; ++row)
            {
                GameObject currPiece = board.allPieces[column, row];
                if (currPiece != null)
                {
                    if (column > 0 && column < board.width - 1)
                    {
                        GameObject leftPiece = board.allPieces[column - 1, row]; // 왼쪽 블럭
                        GameObject rightPiece = board.allPieces[column + 1, row]; // 오른쪽 블럭

                        if (leftPiece != null && rightPiece != null)
                        {
                            // 현재 위치에서 좌우
                            if (leftPiece.tag == currPiece.tag && rightPiece.tag == currPiece.tag)
                            {
                                if(currPiece.GetComponent<Piece>().isRowBomb||
                                    leftPiece.GetComponent<Piece>().isRowBomb||
                                  rightPiece.GetComponent<Piece>().isRowBomb)
                                {
                                    foreach (var curr in currMatches.Union(GetColumnPieces(column)))
                                    {
                                        Debug.Log(curr);
                                    }

                                    currMatches.Union(GetRowPieces(row));
                                }

                                if(currPiece.GetComponent<Piece>().isColumnBomb)
                                {
                                    currMatches.Union(GetColumnPieces(column));
                                   
                                }

                                if (leftPiece.GetComponent<Piece>().isColumnBomb)
                                {
                                    currMatches.Union(GetColumnPieces(column - 1));
                                  
                                }

                                if (rightPiece.GetComponent<Piece>().isColumnBomb)
                                {
                                    currMatches.Union(GetColumnPieces(column + 1));
                                 
                                }

                                if (!currMatches.Contains(leftPiece))
                                {
                                    currMatches.Add(leftPiece);
                                }
                                leftPiece.GetComponent<Piece>().isMatched = true;

                                if(!currMatches.Contains(rightPiece))
                                {
                                    currMatches.Add(rightPiece);
                                }
                                rightPiece.GetComponent<Piece>().isMatched = true;

                                if (!currMatches.Contains(currPiece))
                                {
                                    currMatches.Add(currPiece);
                                }
                                currPiece.GetComponent<Piece>().isMatched = true;
                            }
                        }
                    }

                    if (row > 0 && row < board.height - 1)
                    {
                        GameObject upPiece = board.allPieces[column, row - 1]; // 위 블럭
                        GameObject downPiece = board.allPieces[column, row + 1]; // 아래 블럭

                        if (upPiece != null && downPiece != null)
                        {
                            // 현재 위치에서 상하
                            if (upPiece.tag == currPiece.tag && downPiece.tag == currPiece.tag)
                            {
                                // 아이템 블럭 매치
                                if (currPiece.GetComponent<Piece>().isColumnBomb ||
                                   upPiece.GetComponent<Piece>().isColumnBomb ||
                                 downPiece.GetComponent<Piece>().isColumnBomb)
                                {
                                    foreach (var curr in currMatches.Union(GetColumnPieces(column)))
                                    {
                                        Debug.Log(curr);
                                    }

                                    currMatches.Union(GetColumnPieces(column));
                                }
                                // 현재 블럭이 아이템(RowBomb)
                                if (currPiece.GetComponent<Piece>().isRowBomb)
                                {
                                    currMatches.Union(GetRowPieces(row));
                                 
                                }
                                // 위 블럭이 아이템(RowBomb)
                                if (upPiece.GetComponent<Piece>().isRowBomb)
                                {
                                    currMatches.Union(GetRowPieces(row + 1));
                                
                                }
                                // 아래 블럭이 아이템(RowBomb)
                                if (downPiece.GetComponent<Piece>().isRowBomb)
                                {
                                    currMatches.Union(GetRowPieces(row - 1));
                                }
                                if (!currMatches.Contains(upPiece))
                                {
                                    currMatches.Add(upPiece);
                                }
                                upPiece.GetComponent<Piece>().isMatched = true;

                                if (!currMatches.Contains(downPiece))
                                {
                                    currMatches.Add(downPiece);
                                }
                                downPiece.GetComponent<Piece>().isMatched = true;

                                if (!currMatches.Contains(currPiece))
                                {
                                    currMatches.Add(currPiece);
                                }
                                currPiece.GetComponent<Piece>().isMatched = true;
                            }
                        }
                    }
                }
            }
        }
    }

    private List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> pieces = new List<GameObject>();
        for (int row = 0; row < board.height; ++row)
        {
            if(board.allPieces[column, row] != null)
            {
                pieces.Add(board.allPieces[column, row]);
                board.allPieces[column, row].GetComponent<Piece>().isMatched = true;
            }
        }

        return pieces;
    }

    private List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> pieces = new List<GameObject>();
        for (int column = 0; column < board.width; ++column)
        {
            if (board.allPieces[column, row] != null)
            {
                pieces.Add(board.allPieces[column, row]);
                board.allPieces[column, row].GetComponent<Piece>().isMatched = true;
            }
        }

        return pieces;
    }

    // 블럭 아이템 체크
    public void CheckBombs()
    {
        // 블럭을 움직였을때
        if(board.currPiece !=null)
        {
            // 움직인 블럭이 일치할 때
            if(board.currPiece.isMatched)
            {
                // make it unmatched
                board.currPiece.isMatched = false;
                // 아이템 임의 생성
               /* int typeOfBomb = Random.Range(0, 100);
                if(typeOfBomb < 50)
                {
                    // 아이템(RowBomb) 생성
                    board.currPiece.MakeRowBomb();
                }
                else if(typeOfBomb >= 50)
                {
                    // 아이템(column Bomb) 생성
                    board.currPiece.MakeColumnBomb();
                }*/
                if(board.currPiece.swipeAngle > -45 && board.currPiece.swipeAngle <= 45 ||
                   board.currPiece.swipeAngle < -135 || board.currPiece.swipeAngle >= 135)
                {
                    board.currPiece.MakeRowBomb();
                }
                else
                {
                    board.currPiece.MakeColumnBomb();
                }
            }
            // 다른 블럭이 일치할때
             else if(board.currPiece.otherPiece !=null)
            {
                Piece otherPiece = board.currPiece.otherPiece.GetComponent<Piece>();
                // 다른 블럭이 일치할 때
                /*if(otherPiece.isMatched)
                {
                    otherPiece.isMatched = false;
                    int typeOfBomb = Random.Range(0, 100);
                    if (typeOfBomb < 50)
                    {
                        // 아이템(RowBomb) 생성
                        otherPiece.MakeRowBomb();
                    }
                    else if (typeOfBomb >= 50)
                    {
                        // 아이템(column Bomb) 생성
                        otherPiece.MakeColumnBomb();
                    }
                }*/

                if (board.currPiece.swipeAngle > -45 && board.currPiece.swipeAngle <= 45 ||
                   board.currPiece.swipeAngle < -135 || board.currPiece.swipeAngle >= 135)
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
