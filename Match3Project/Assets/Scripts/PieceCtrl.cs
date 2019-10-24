using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceCtrl : MonoBehaviour
{
    public int value;

    public int row;
    public int column;

    public int prevRow;
    public int prevColumn;

	private float accumTime = 0;

	private bool isMoving;
	private bool tunning;

    private Vector2 startPos;
    private Vector2 endPos;
	private Vector2 movePos;

	private Vector2 zeroPos = Vector2.zero;

    private BoardManager board;
	private SpriteRenderer pieceSprite;
    public PieceCtrl targetPiece;

	public void Awake()
	{
		pieceSprite = GetComponent<SpriteRenderer>();
	}

	public void InitPiece(int v, int r, int c, BoardManager b)
	{
		value = v;
		row = r;
		column = c;

		if (board == null)
			board = b;

		pieceSprite.sprite = board.pieceSprites[value];
	}

	private void Update()
	{
		if(isMoving && targetPiece != null)
		{
			movePos = new Vector2(row, column);

			accumTime += Time.deltaTime / board.duration;
			transform.position = Vector2.Lerp(transform.position, movePos, accumTime);

			if (Vector2.Distance(transform.position, movePos) == 0f)
			{
				accumTime = 0;
				SetPositionPiece();

				if (board.FollowUpBoardAllCheck())
				{
					board.currState = BoardState.WORK;
				}

				else
				{

				}

				targetPiece = null;
				isMoving = false;
			}
		}
	}

	private void OnMouseDown()
    {
        startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculratePiece();
    }

    private void CalculratePiece()
    {
		if (board.currState != BoardState.WORK)
		{
			Vector2 dir = (endPos - startPos).normalized;

			if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
				MoveToPiece(dir.y > 0 ? Vector2.up : Vector2.down);

			else if (Mathf.Abs(dir.y) < Mathf.Abs(dir.x))
				MoveToPiece(dir.x > 0 ? Vector2.right : Vector2.left);
		}
    }

    private void MoveToPiece(Vector2 direction)
    {
        if (board.boardIndex[row + (int)direction.x, column + (int)direction.y] != null)
        {
			// 블럭이 참조할 대상
            targetPiece = board.GetPiece(row + (int)direction.x, column + (int)direction.y);
            targetPiece.targetPiece = this;

			// 블럭 이전 위치 값 초기화
            prevRow = row;
            prevColumn = column;
            targetPiece.prevRow = targetPiece.row;
            targetPiece.prevColumn = targetPiece.column;

			// 블럭 현재 위치 값 초기화
            row += (int)direction.x;
            column += (int)direction.y;
            targetPiece.row += -1 * (int)direction.x;
            targetPiece.column += -1 * (int)direction.y;

			// 블럭을 움직임
			isMoving = true;
			targetPiece.isMoving = true;
		}
    }

    public void SetPositionPiece()
    {
        transform.position = new Vector2(row, column);
        board.boardIndex[row, column] = gameObject;
        name = "[" + row + " , " + column + "]";
    }
}