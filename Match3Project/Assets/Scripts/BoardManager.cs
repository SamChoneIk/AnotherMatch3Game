using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoardState
{
	SETUP,
    ORDER,
    WORK,
}

public class BoardManager : MonoBehaviour
{
    [Header("Board State")]
    public BoardState currState = BoardState.SETUP;
    public int width, height;
	public float waitTime = 0.16f;
	public float duration;
	public float accumTime;

    public GameObject[,] boardIndex;
    public GameObject disPieces;
	public GameObject selectPiece;

    public List<PieceCtrl> matchedPieces;
    public List<PieceCtrl> disabledPieces;

    [Header("Piece Parts")]
    public GameObject piecePrefab;
    public Sprite[] pieceSprites;

    void Start()
    {
        boardIndex = new GameObject[width, height];

        matchedPieces = new List<PieceCtrl>();
        disabledPieces = new List<PieceCtrl>();

        CreateBoard();
		FindAllBoard();
	}

    void Update()
	{ 
        if (currState == BoardState.WORK)
        {
			FindAllBoard();
			Debug.Log($"Board Checking Start Time {Time.time}");

			if (matchedPieces.Count > 0)
			{
				Debug.Log("Find match");
				MatchedPieceDisabled();
			}

			else
			{
				//Debug.Log($"Board Checking End Time {Time.time}");
				currState = BoardState.ORDER;
			}
        }
    }

    private void CreateBoard()
    {
        for (int column = 0; column < height; ++column)
        {
            for (int row = 0; row < width; ++row)
            {
                Vector2 piecePos = new Vector2(row, column);
                int pieceValue = Random.Range(0, pieceSprites.Length);

                GameObject pieceGo = Instantiate(piecePrefab, piecePos, Quaternion.identity);
                PieceCtrl piece = pieceGo.GetComponent<PieceCtrl>();

                piece.InitPiece(pieceValue, row, column, this);

                pieceGo.transform.parent = transform;
                pieceGo.name = "[" + row + " , " + column + "]";
                boardIndex[row, column] = pieceGo;
            }
        }

        for (int i = 0; i < Mathf.RoundToInt(height * width / 2); ++i)
        {
            GameObject pieceGo = Instantiate(piecePrefab, Vector2.zero, Quaternion.identity);
            PieceCtrl piece = pieceGo.GetComponent<PieceCtrl>();

			piece.InitPiece(0, 0, 0, this);

            pieceGo.transform.parent = disPieces.transform;
            pieceGo.name = "DisPiece";
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
					PieceCtrl currPiece = GetPiece(row, column);

					if (currPiece != null)
					{
						if (row > 0 && row < width - 1)
						{
							if (boardIndex[row + 1, column] != null && boardIndex[row - 1, column] != null)
							{
								PieceCtrl rightPiece = GetPiece(row + 1, column);
								PieceCtrl leftPiece = GetPiece(row - 1, column);

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
								PieceCtrl upPiece = GetPiece(row, column + 1);
								PieceCtrl downPiece = GetPiece(row, column - 1);

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

		if (currState == BoardState.SETUP)
		{
			if (matchedPieces.Count > 0)
			{
				//Debug.Log("Find match");
				MatchedPieceDisabled();
			}

			else
				currState = BoardState.ORDER;
		}
	}

    private void MatchedPieceDisabled()
    {
		//Debug.Log($"Disable Time {Time.time}");
		foreach (var piece in matchedPieces)
        {
            //Debug.Log($"disabledPieces {piece.row} , {piece.column}");
            boardIndex[piece.row, piece.column] = null;
            
            piece.transform.parent = disPieces.transform;
			piece.InitPiece(0, 0, 0, this);

            piece.name = "DisPiece";
            piece.gameObject.SetActive(false);
            disabledPieces.Add(piece);
        }
        matchedPieces.Clear();

        StartCoroutine(FallingPieces());
    }

    IEnumerator FallingPieces()
    {
		//Debug.Log($"FallingPiece Time {Time.time}");
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
                            //yield return new WaitForSeconds(waitTime);

                            PieceCtrl fallPiece = GetPiece(row, i); // 빈자리의 위에 있는 피스
							Vector2 fallPos = new Vector2(row, column);

							while (Vector2.Distance(fallPiece.transform.position, fallPos) > 0.1f)
							{
								//Debug.Log(Vector2.Distance(fallPiece.transform.position, fallPos));

								accumTime += Time.deltaTime / duration;
								fallPiece.transform.position = Vector2.Lerp(fallPiece.transform.position, fallPos, accumTime);

								yield return null;
							}
							accumTime = 0;

							fallPiece.column = column;
                            fallPiece.SetPositionPiece();

                            boardIndex[row, i] = null;

                            //yield return new WaitForSeconds(waitTime);

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
                    //yield return new WaitForSeconds(waitTime);

                    PieceCtrl enabledPiece = EnabledPiece(row, height - 1);
					Vector2 fallPos = new Vector2(row, column);

					while (Vector2.Distance(enabledPiece.transform.position, fallPos) > 0.1f)
					{
						//Debug.Log(Vector2.Distance(enabledPiece.transform.position, fallPos));

						accumTime += Time.deltaTime / duration;
						enabledPiece.transform.position = Vector2.Lerp(enabledPiece.transform.position, fallPos, accumTime);

						yield return null;
					}
					accumTime = 0;

					enabledPiece.column = column;
                    enabledPiece.SetPositionPiece();

					if (boardIndex[row, height - 1] != null)
					{
						//yield return new WaitForSeconds(waitTime);
						continue;
					}

                    boardIndex[row, height - 1] = null;

                   // yield return new WaitForSeconds(waitTime);
                }
            }
        }

		if (!FollowUpBoardAllCheck())
			currState = BoardState.ORDER;
	}

	public bool FollowUpBoardAllCheck()
	{
		for (int column = 0; column < height; ++column)
		{
			for (int row = 0; row < width; ++row)
			{
				if (boardIndex[row, column] != null)
				{
					PieceCtrl currPiece = GetPiece(row, column);

					if (currPiece != null)
					{
						if (row > 0 && row < width - 1)
						{
							if (boardIndex[row + 1, column] != null && boardIndex[row - 1, column] != null)
							{
								PieceCtrl rightPiece = GetPiece(row + 1, column);
								PieceCtrl leftPiece = GetPiece(row - 1, column);

								if (currPiece.value == rightPiece.value && currPiece.value == leftPiece.value)
									return true;
							}
						}

						if (column > 0 && column < height - 1)
						{
							if (boardIndex[row, column + 1] != null && boardIndex[row, column - 1] != null)
							{
								PieceCtrl upPiece = GetPiece(row, column + 1);
								PieceCtrl downPiece = GetPiece(row, column - 1);

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

    private PieceCtrl EnabledPiece(int row, int column)
    {
        PieceCtrl enabledPiece = disabledPieces[0];

        enabledPiece.InitPiece(Random.Range(0, pieceSprites.Length), row, column, this);

        enabledPiece.transform.parent = transform;
		enabledPiece.transform.position = new Vector2(row, column + 5);

        enabledPiece.gameObject.SetActive(true);

        disabledPieces.RemoveAt(0);

        return enabledPiece;
    }

    public PieceCtrl GetPiece(int row, int column)
    {
        return boardIndex[row, column].GetComponent<PieceCtrl>();
    }
}