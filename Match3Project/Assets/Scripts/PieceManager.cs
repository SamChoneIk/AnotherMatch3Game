using UnityEngine;

public class PieceManager : MonoBehaviour
{
    public int value;
    public int row;
    public int column;

    private int prevRow;
    private int prevColumn;

    private float accumTime = 0;

    private Vector2 startPos;
    private Vector2 endPos;

    private BoardManager board;
    public PieceManager targetPiece;

    private void Awake()
    {
        board = FindObjectOfType<BoardManager>();
    }

    public void SetPiece(int v, int r, int c)
    {
        value = v;
        row = r;
        column = c;
    }

    public void SetPosition()
    {
        transform.position = new Vector2(row, column);
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
        if (board.currState == BoardState.WORK)
            return;

        Vector2 dir = (endPos - startPos).normalized;

        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
        {
            MoveToPiece(dir.y > 0 ? Vector2.up : Vector2.down);
        }

        else if (Mathf.Abs(dir.y) < Mathf.Abs(dir.x))
        {
            MoveToPiece(dir.x > 0 ? Vector2.right : Vector2.left);
        }
    }

    private void MoveToPiece(Vector2 direction)
    {
        if (board.boardIndex[row + (int)direction.x, column + (int)direction.y] != null)
        {
            targetPiece = board.GetPiece(row + (int)direction.x, column + (int)direction.y);
            targetPiece.targetPiece = this;

            prevRow = row;
            prevColumn = column;

            targetPiece.prevRow = targetPiece.row;
            targetPiece.prevColumn = targetPiece.column;

            row += (int)direction.x;
            column += (int)direction.y;

            targetPiece.row += -1 * (int)direction.x;
            targetPiece.column += -1 * (int)direction.y;

            SetPositionPiece();
            targetPiece.SetPositionPiece();

            Debug.Log("Move Complete");
        }

        else
        {
            //board.currState = BoardState.ORDER;
        }
    }

    private void MovingPiece()
    {
        //Debug.Log(gameObject.name + " is Time : " + Time.time);
        accumTime += Time.deltaTime / board.duration;

        transform.position = Vector2.Lerp(transform.position, targetPiece.transform.position, accumTime);

        if (Vector2.Distance(transform.position, targetPiece.transform.position) < 0.1f)
        {
            accumTime = 0f;
        }
    }

    public void SetPositionPiece()
    {
        transform.position = new Vector2(row, column);
        board.boardIndex[row, column] = gameObject;
        name = "[" + row + " , " + column + "]";
    }
}