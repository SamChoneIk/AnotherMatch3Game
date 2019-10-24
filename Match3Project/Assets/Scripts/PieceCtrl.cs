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

    public float dragRegist = 0.2f;

	public bool isMoving;

    public Vector2 movePos;
    private Vector2 startPos;
    private Vector2 endPos;

    private BoardManager board;
	private SpriteRenderer pieceSprite;
    public PieceCtrl target;

	public void Awake()
	{
		pieceSprite = GetComponent<SpriteRenderer>();
	}

	public void InitPiece(int v, int r, int c, BoardManager b)
	{
        if (board == null)
            board = b;

        value = v;
		row = r;
		column = c;

		pieceSprite.sprite = board.pieceSprites[value];
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
		if (board.currState == BoardState.ORDER)
		{
			Vector2 dir = (endPos - startPos).normalized;

			if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
				MoveToPiece(dir.y > dragRegist ? Vector2.up : Vector2.down);

			else if (Mathf.Abs(dir.y) < Mathf.Abs(dir.x))
				MoveToPiece(dir.x > dragRegist ? Vector2.right : Vector2.left);
		}
    }

    private void MoveToPiece(Vector2 direction)
    {
        if (board.boardIndex[row + (int)direction.x, column + (int)direction.y] != null)
        {
			// 블럭이 참조할 대상
            target = board.GetPiece(row + (int)direction.x, column + (int)direction.y);
            target.target = this;

			// 블럭 이전 위치 값 초기화
            prevRow = row;
            prevColumn = column;
            target.prevRow = target.row;
            target.prevColumn = target.column;

			// 블럭 현재 위치 값 초기화
            row += (int)direction.x;
            column += (int)direction.y;
            target.row += -1 * (int)direction.x;
            target.column += -1 * (int)direction.y;

            movePos = new Vector2(row, column);
            target.movePos = new Vector2(target.row, target.column);

            // 블럭을 움직임
            isMoving = true;
            target.isMoving = true;

            board.selectPiece = this;
            board.targetPiece = target;
		}
    }

    public void movedPiece(float t)
    {
        transform.position = Vector2.Lerp(transform.position, movePos, t);

        if (Vector2.Distance(transform.position, movePos) == 0f)
        {
            SetPositionPiece();
            isMoving = false;
        }
    }

    public void SetPositionPiece()
    {
        transform.position = new Vector2(row, column);
        board.boardIndex[row, column] = gameObject;
        name = "[" + row + " , " + column + "]";
    }
}