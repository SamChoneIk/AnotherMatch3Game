using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoardState
{
    ORDER,
    WORK,
}

public class BoardManager : MonoBehaviour
{
    [Header("Board State")]
    public BoardState currState = BoardState.WORK;
    public int width, height;
    public float blockDuration;
    public float boardDuration;
    public float accumTime;

    public GameObject[,] boardIndex;
    public GameObject disPieces;
    public Block selectPiece;
    public Block targetPiece;

    public List<Block> matchedPieces;
    public List<Block> disabledPieces;

    [Header("Piece Parts")]
    public GameObject piecePrefab;
    public Sprite[] pieceSprites;

    void Start()
    {
        boardIndex = new GameObject[width, height];

        matchedPieces = new List<Block>();
        disabledPieces = new List<Block>();

        CreateBoard();
        FindAllBoard();
    }

    void Update()
    {
        if (selectPiece != null && targetPiece != null)
        {
            if (selectPiece.currState == BlockState.WAIT &&
            targetPiece.currState == BlockState.WAIT &&
            currState == BoardState.WORK)
            {
                SetPiece(selectPiece);
                SetPiece(targetPiece);

                selectPiece = null;
                targetPiece = null;

                FindAllBoard();
            }
        }
    }

    public void BoardStateCtrl(BoardState state)
    {
        currState = state;

        switch (currState)
        {
            case BoardState.ORDER:
                break;

            case BoardState.WORK:
                {
                    if (selectPiece != null && targetPiece != null)
                    {
                        selectPiece.currState = BlockState.MOVE;
                        targetPiece.currState = BlockState.MOVE;
                    }

                    MatchedPieceDisabled();
                    break;
                }
        }
    }

    private void CreateBoard()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                GameObject pieceGo = Instantiate(piecePrefab, new Vector2(row, column), Quaternion.identity);
                Block piece = pieceGo.GetComponent<Block>();

                piece.InitPiece(Random.Range(0, pieceSprites.Length), row, column, this);

                pieceGo.transform.parent = transform;
                pieceGo.name = "[" + row + " , " + column + "]";

                boardIndex[row, column] = pieceGo;
            }
        }

        for (int i = 0; i < Mathf.RoundToInt(height * width / 2); ++i)
        {
            GameObject pieceGo = Instantiate(piecePrefab, Vector2.zero, Quaternion.identity);
            Block piece = pieceGo.GetComponent<Block>();

            piece.InitPiece(0, 0, 0, this);

            pieceGo.transform.parent = disPieces.transform;
            pieceGo.name = "DefaultPiece";

            pieceGo.SetActive(false);
            disabledPieces.Add(piece);
        }
    }

    public void FindAllBoard()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] != null)
                {
                    Block currPiece = GetPiece(row, column);

                    if (currPiece != null)
                    {
                        if (row > 0 && row < width - 1)
                        {
                            if (boardIndex[row + 1, column] != null && boardIndex[row - 1, column] != null)
                            {
                                Block rightPiece = GetPiece(row + 1, column);
                                Block leftPiece = GetPiece(row - 1, column);

                                if (currPiece.value == rightPiece.value && currPiece.value == leftPiece.value)
                                {
                                    if (!matchedPieces.Contains(currPiece))
                                        matchedPieces.Add(currPiece);

                                    if (!matchedPieces.Contains(rightPiece))
                                        matchedPieces.Add(rightPiece);

                                    if (!matchedPieces.Contains(leftPiece))
                                        matchedPieces.Add(leftPiece);
                                }
                            }
                        }

                        if (column > 0 && column < height - 1)
                        {
                            if (boardIndex[row, column + 1] != null && boardIndex[row, column - 1] != null)
                            {
                                Block upPiece = GetPiece(row, column + 1);
                                Block downPiece = GetPiece(row, column - 1);

                                if (currPiece.value == upPiece.value && currPiece.value == downPiece.value)
                                {
                                    if (!matchedPieces.Contains(currPiece))
                                        matchedPieces.Add(currPiece);

                                    if (!matchedPieces.Contains(upPiece))
                                        matchedPieces.Add(upPiece);

                                    if (!matchedPieces.Contains(downPiece))
                                        matchedPieces.Add(downPiece);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (matchedPieces.Count > 0)
            BoardStateCtrl(BoardState.WORK);

        else
            BoardStateCtrl(BoardState.ORDER);
    }

    private void MatchedPieceDisabled()
    {
        if (matchedPieces.Count <= 0)
            return;

         foreach (var piece in matchedPieces)
        {
            //Debug.Log($"disabledPieces {piece.row} , {piece.column}");
            boardIndex[piece.row, piece.column] = null;

            piece.target = null;
           
            piece.InitPiece(0, 0, 0, this);

            piece.name = "DefaultPiece";
            piece.transform.parent = disPieces.transform;
            piece.transform.position = new Vector2(piece.row, piece.column);
            

            piece.gameObject.SetActive(false);
            disabledPieces.Add(piece);
        }

        matchedPieces.Clear();

        StartCoroutine(FallingPieces());
    }

    IEnumerator FallingPieces()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] == null)
                {
                    for (int i = column + 1; i < height; ++i)
                    {
                        if (boardIndex[row, i] != null)
                        {
                            Block fallPiece = GetPiece(row, i); // 빈자리의 위에 있는 피스
							Vector2 fallPos = new Vector2(row, column);

							while (Vector2.Distance(fallPiece.transform.position, fallPos) > 0.2f)
							{
								//Debug.Log(Vector2.Distance(fallPiece.transform.position, fallPos));

								accumTime += Time.deltaTime / boardDuration;
								fallPiece.transform.position = Vector2.Lerp(fallPiece.transform.position, fallPos, accumTime);

								yield return null;
							}

							accumTime = 0;

                            fallPiece.InitPiece(fallPiece.value, fallPiece.row, column, this);

                            fallPiece.transform.position = new Vector2(row, column);
                            fallPiece.name = "[" + row + " , " + column + "]";
                            boardIndex[row, column] = fallPiece.gameObject;

                            boardIndex[row, i] = null;

                            break;
                        }
                    }
                }
            }
        }

       for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] == null)
                {
                    Block enabledPiece = EnabledPiece(row, height - 1);
					Vector2 fallPos = new Vector2(row, column);

					while (Vector2.Distance(enabledPiece.transform.position, fallPos) > 0.1f)
					{
						accumTime += Time.deltaTime / boardDuration;
						enabledPiece.transform.position = Vector2.Lerp(enabledPiece.transform.position, fallPos, accumTime);

						yield return null;
					}

					accumTime = 0;

                    enabledPiece.InitPiece(enabledPiece.value, enabledPiece.row, column, this);

                    enabledPiece.transform.position = new Vector2(row, column);
                    enabledPiece.name = "[" + row + " , " + column + "]";
                    boardIndex[row, column] = enabledPiece.gameObject;

                    if (boardIndex[row, height - 1] != null)
						continue;

                    boardIndex[row, height - 1] = null;
                }
            }
        }

        if (FollowUpBoardAllCheck())
            FindAllBoard();

        else
            BoardStateCtrl(BoardState.ORDER);
    }

	public bool FollowUpBoardAllCheck()
	{
		for (int column = 0; column < height; ++column)
		{
			for (int row = 0; row < width; ++row)
			{
				if (boardIndex[row, column] != null)
				{
					Block currPiece = GetPiece(row, column);

					if (currPiece != null)
					{
						if (row > 0 && row < width - 1)
						{
							if (boardIndex[row + 1, column] != null && boardIndex[row - 1, column] != null)
							{
								Block rightPiece = GetPiece(row + 1, column);
								Block leftPiece = GetPiece(row - 1, column);

								if (currPiece.value == rightPiece.value && currPiece.value == leftPiece.value)
									return true;
							}
						}

						if (column > 0 && column < height - 1)
						{
							if (boardIndex[row, column + 1] != null && boardIndex[row, column - 1] != null)
							{
								Block upPiece = GetPiece(row, column + 1);
								Block downPiece = GetPiece(row, column - 1);

								if (currPiece.value == upPiece.value && currPiece.value == downPiece.value)
									return true;
							}
						}
					}
				}
			}
		}

		return false;
	}

    public void DebugBoardChecking()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                if (boardIndex[row, column] == null)
                    Debug.Log("[" + row + " , " + column + "] = is Null!!");
            }
        }
    }

    private Block EnabledPiece(int row, int column)
    {
        Block enabledPiece = disabledPieces[0];

        enabledPiece.InitPiece(Random.Range(0, pieceSprites.Length), row, column, this);
        enabledPiece.transform.parent = transform;
		enabledPiece.transform.position = new Vector2(row, column + 5);
        enabledPiece.gameObject.SetActive(true);

        disabledPieces.RemoveAt(0);

        return enabledPiece;
    }

    public Block GetPiece(int row, int column)
    {
        return boardIndex[row, column].GetComponent<Block>();
    }

    public void SetPiece(Block piece)
    {
        piece.transform.position = new Vector2(piece.row, piece.column);
        piece.name = "[" + piece.row + " , " + piece.column + "]";
        boardIndex[piece.row, piece.column] = piece.gameObject;
    }
}