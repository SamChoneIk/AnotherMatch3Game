using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Parts")]
    public BoardState currBoardState;

    public int horizontal = 6;
    public int vertical = 8;
    public int fallOffset = 5;

    public float delayTime = 0.08f;
    public float effectDelayTime = 0.5f;
    public float pieceFallSpeed;

    public GameObject[,] pieceArray;
    public Piece selectedPiece;

    [Header("Piece Parts")]
    public GameObject piecePrefab;
    public GameObject disabledPiecesParent;

    public Sprite[] pieceSprites;
    public Sprite[] itemSprites;

    public StageController stageCtrl;

    private List<Piece> matchedPieces;
    private List<Piece> disabledPieces;
    private List<Piece> verifyedPieces;
    private List<Piece> hintPieces;

    private float hintAccumTime;
    private bool isMatched = false;

    private void Start()
    {
        currBoardState = BoardState.WORK;

        pieceArray = new GameObject[horizontal, vertical];

        matchedPieces = new List<Piece>();
        disabledPieces = new List<Piece>();
        verifyedPieces = new List<Piece>();
        hintPieces = new List<Piece>();

        pieceSprites = Resources.LoadAll<Sprite>(StaticVariables.PieceSpritesPath);
        itemSprites = Resources.LoadAll<Sprite>(StaticVariables.ItemSpritesPath);

        stageCtrl = GameObject.FindGameObjectWithTag(StaticVariables.StageController).GetComponent<StageController>();
        stageCtrl.board = this;

        GeneratePiece();
    }

    private void GeneratePiece(bool regenerate = false)
    {
        if(!regenerate)
        {
            for (int i = 0; i < horizontal * vertical + Mathf.RoundToInt((horizontal * vertical) * 0.5f); ++i)
            {
                GameObject pieceGo = Instantiate(piecePrefab, Vector2.zero, Quaternion.identity);
                Piece firstPiece = pieceGo.GetComponent<Piece>();

                firstPiece.name = StaticVariables.DisabledPieceName;
                firstPiece.transform.SetParent(disabledPiecesParent.transform);

                disabledPieces.Add(firstPiece);
                pieceGo.SetActive(false);
            }
        }

        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (regenerate)
                {
                    if (selectedPiece != null)
                    {
                        selectedPiece.target = null;
                        selectedPiece = null;
                    }

                    Piece regenPiece = GetPiece(row, column);

                    regenPiece.SetPieceValue(Random.Range(0, pieceSprites.Length));
                    regenPiece.transform.position = new Vector2(regenPiece.row, regenPiece.column + fallOffset);
                }

                else
                {
                    Piece initPiece = disabledPieces[0];

                    initPiece.SettingPiece(this, $"[{row} , {column}]", Random.Range(0, pieceSprites.Length), row, column);

                    initPiece.transform.SetParent(transform);
                    initPiece.transform.position = new Vector2(initPiece.row, initPiece.column + fallOffset);

                    initPiece.gameObject.SetActive(true);
                    disabledPieces.RemoveAt(0);
                }
            }
        }

        StartCoroutine(CheckingTheMatchedPiece());
    }

    IEnumerator CheckingTheMatchedPiece()
    {
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                Piece checkPiece = GetPiece(row, column);

                while (IsMatchedPiece(checkPiece))
                    checkPiece.SetPieceValue(Random.Range(0, pieceSprites.Length));

                checkPiece.currState = PieceState.MOVE;
            }
        }

        while (FindMovingPiece())
            yield return null;

        yield return new WaitForSeconds(delayTime);

        currBoardState = BoardState.ORDER;
    }

    private bool IsMatchedPiece(Piece piece)
    {
        if (piece.row < horizontal - 1 && piece.row > 0)
        {
            if (piece.value == GetPiece(piece.row + 1, piece.column).value &&
                piece.value == GetPiece(piece.row - 1, piece.column).value)
                return true;
        }

        if (piece.column < vertical - 1 && piece.column > 0)
        {
            if (piece.value == GetPiece(piece.row, piece.column + 1).value &&
                piece.value == GetPiece(piece.row, piece.column - 1).value)
                return true;
        }

        return false;
    }

    private void Update()
    {
        if (IsStageStopped())
            return;

        switch (currBoardState)
        {
            case BoardState.WORK:
                {
                    if (hintPieces.Count > 0)
                        HintPieceSwitch(false);

                    hintAccumTime = 0;

                    // 블럭이 움직이고 있을 때
                    if (selectedPiece == null || FindMovingPiece())
                        return;

                    if (selectedPiece.isTunning && selectedPiece.target.isTunning)
                    {
                        selectedPiece.TunningNull();
                        return;
                    }

                    if (selectedPiece.crossBomb || selectedPiece.target.crossBomb)
                    {
                        UsedCrossBomb(selectedPiece);
                        UsedCrossBomb(selectedPiece.target);
                        FindMatchedPiece();
                        return;
                    }

                    FindMatchedPiece(true);
                    if (isMatched)
                        FindMatchedPiece();

                    else
                    {
                        selectedPiece.target.MoveToBack();
                        selectedPiece.MoveToBack();
                    }

                    break;
                }

            case BoardState.ORDER:
                {
                    if (hintPieces.Count == 0)
                    {
                        hintAccumTime += Time.deltaTime;

                        if (hintAccumTime > 3f)
                        {
                            if (DeadLockCheck())
                                GeneratePiece(true);

                            else
                                HintPieceSwitch(true);
                        }
                    }

                    break;
                }
        }

        //DebugSystem();
    }

    private void FindMatchedPiece(bool findMatched = false)
    {
        if (isMatched)
            isMatched = false;

        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (row > 0 && row < horizontal - 1)
                {
                    if (pieceArray[row + 1, column] != null && pieceArray[row - 1, column] != null)
                        AddMatchedPiece(GetPiece(row, column), 
										GetPiece(row + 1, column), 
										GetPiece(row - 1, column), 
										findMatched);
                }

                if (column > 0 && column < vertical - 1)
                {
                    if (pieceArray[row, column + 1] != null && pieceArray[row, column - 1] != null)
                        AddMatchedPiece(GetPiece(row, column), 
										GetPiece(row, column + 1), 
										GetPiece(row, column - 1), 
										findMatched);
                }

                if (isMatched)
                    return;
            }
        }

        if (findMatched && !isMatched)
            return;

        if (matchedPieces.Count > 0)
        {
            GenerateItemPiece();
            StartCoroutine(DisabledMatchedPiece());
            return;
        }

        else
        {
            stageCtrl.combo = 0;
            currBoardState = BoardState.ORDER;
        }
    }

    private void AddMatchedPiece(Piece currPiece, Piece sidePiece1, Piece sidePiece2, bool findMatched)
    {
        if (currPiece.value == sidePiece1.value && currPiece.value == sidePiece2.value)
        {
            if (findMatched)
            {
                isMatched = true;
                return;
            }

			else
			{
				if (!matchedPieces.Contains(sidePiece1))
					matchedPieces.Add(sidePiece1);

				if (!matchedPieces.Contains(currPiece))
					matchedPieces.Add(currPiece);

				if (!matchedPieces.Contains(sidePiece2))
					matchedPieces.Add(sidePiece2);

				UsedItem(sidePiece1, currPiece, sidePiece2);
			}
        }
    }

    private void GenerateItemPiece()
    {
        if (matchedPieces.Count < 3)
            return;

        int rows = 0;
        int cols = 0;
        int prevCount = 0;

        // Cross Bomb
        if (matchedPieces.Count > 4)
        {
            for (int i = 0; i < matchedPieces.Count - 1; ++i)
            {
                if (verifyedPieces.Contains(matchedPieces[i]))
                    continue;

                verifyedPieces.Add(matchedPieces[i]);
                FindDirectionMatchedPiece(matchedPieces[i], ref rows, ref cols);

                if (rows >= 2 && cols >= 2)
                {
                    Piece bombPiece = matchedPieces[i];

                    if (selectedPiece != null)
                    {
                        if (selectedPiece.value == matchedPieces[i].value)
                            bombPiece = selectedPiece;

                        if (selectedPiece.target != null && selectedPiece.target.value == matchedPieces[i].value)
                            bombPiece = selectedPiece.target;
                    }

                    else
                        bombPiece = verifyedPieces[Random.Range(prevCount, verifyedPieces.Count)];

                    bombPiece.crossBomb = true;
                    bombPiece.value = pieceSprites.Length + 1;
                    bombPiece.itemSprite.sprite = itemSprites[2];

                    matchedPieces.Remove(bombPiece);

                    prevCount = verifyedPieces.Count - 1;
                }

                else
                {
                    if (verifyedPieces.Count >= 3)
                        verifyedPieces.RemoveRange(prevCount, (verifyedPieces.Count - 1) - prevCount);

                    else
                        verifyedPieces.Clear();
                }

                rows = 0;
                cols = 0;
            }
        }

        // Row Bomb
        for (int i = 0; i < matchedPieces.Count - 1; ++i)
        {
            if (verifyedPieces.Contains(matchedPieces[i]))
                continue;

            for (int r = 1; r < horizontal - 1; ++r)
            {
                if (matchedPieces[i].row + r > horizontal - 1)
                    break;

                Piece check = GetPiece(matchedPieces[i].row + r, matchedPieces[i].column);

                if (matchedPieces.Contains(check) && matchedPieces[i].value == check.value)
                {
                    verifyedPieces.Add(check);
                    rows++;
                }

                else
                    break;
            }

            if (rows >= 3)
            {
                Piece bombPiece = matchedPieces[i];

                if(selectedPiece != null)
                {
                    if (selectedPiece.value == matchedPieces[i].value)
                        bombPiece = selectedPiece;

                    if (selectedPiece.target != null && selectedPiece.target.value == matchedPieces[i].value)
                        bombPiece = selectedPiece.target;
                }
                
                else
                    bombPiece = verifyedPieces[Random.Range(prevCount, verifyedPieces.Count)];

                bombPiece.rowBomb = true;
                bombPiece.itemSprite.sprite = itemSprites[1];

                matchedPieces.Remove(bombPiece);

                prevCount = verifyedPieces.Count - 1;
            }

            else
            {
                if (verifyedPieces.Count >= 3)
                    verifyedPieces.RemoveRange(prevCount, (verifyedPieces.Count - 1) - prevCount);

                else
                    verifyedPieces.Clear();
            }

            rows = 0;
        }

        // Column Bomb
        for (int i = 0; i < matchedPieces.Count - 1; ++i)
        {
            if (verifyedPieces.Contains(matchedPieces[i]))
                continue;

            for (int c = 1; c < vertical - 1; ++c)
            {
                if (matchedPieces[i].column + c > vertical - 1)
                    break;

                Piece check = GetPiece(matchedPieces[i].row, matchedPieces[i].column + c);

                if (matchedPieces.Contains(check) && matchedPieces[i].value == check.value)
                {
                    verifyedPieces.Add(check);
                    cols++;
                }

                else
                    break;
            }

            if (cols >= 3)
            {
                Piece bombPiece = matchedPieces[i];

                if (selectedPiece != null)
                {
                    if (selectedPiece.value == matchedPieces[i].value)
                        bombPiece = selectedPiece;

                    if (selectedPiece.target != null && selectedPiece.target.value == matchedPieces[i].value)
                        bombPiece = selectedPiece.target;
                }

                else
                    bombPiece = verifyedPieces[Random.Range(prevCount, verifyedPieces.Count)];

                bombPiece.columnBomb = true;
                bombPiece.itemSprite.sprite = itemSprites[0];

                matchedPieces.Remove(bombPiece);

                prevCount = verifyedPieces.Count - 1;
            }

            else
            {
                if (verifyedPieces.Count >= 3)
                    verifyedPieces.RemoveRange(prevCount, verifyedPieces.Count  - prevCount);

                else
                    verifyedPieces.Clear();
            }

            cols = 0;
        }
    }

    private void FindDirectionMatchedPiece(Piece piece, ref int rows, ref int cols)
    {
        Vector2[] direction =
        {
            Vector2.up,
            Vector2.right,
            Vector2.down,
            Vector2.left
        };

        foreach (var dir in direction)
        {
            for (int i = 1; ; ++i)
            {
                if (piece.row + ((int)dir.x * i) >= horizontal || piece.column + ((int)dir.y * i) >= vertical ||
                    piece.row + ((int)dir.x * i) <= -1 || piece.column + ((int)dir.y * i) <= 1)
                    break;

                Piece check = GetPiece(piece.row + ((int)dir.x * i), piece.column + ((int)dir.y * i));

                // 매치된 블럭일 때
                if (matchedPieces.Contains(check) && !verifyedPieces.Contains(check) && check.value == piece.value)
                {
                    // 체크가 끝난 블럭은 검사에서 제외
                    verifyedPieces.Add(check);

                    // 검사하는 블럭에 상하좌우에 다른 매치된 블럭이 있을 경우
                    if (FindNeighborPiece(check))
                        FindDirectionMatchedPiece(check, ref rows, ref cols);

                    // 상하인 경우 cols 증가
                    if (dir == direction[0] || dir == direction[2])
                        ++cols;

                    // 좌우인 경우 rows 증가
                    else if (dir == direction[1] || dir == direction[3])
                        ++rows;
                }

                // 블럭이 없으면 즉시 취소
                else
                    break;
            }
        }
    }

    private bool FindNeighborPiece(Piece piece)
    {
        if (piece.row + 1 < horizontal - 1)
        {
            Piece right = GetPiece(piece.row + 1, piece.column);
            if (matchedPieces.Contains(right) && !verifyedPieces.Contains(right) && 
                piece.value == right.value)
                return true;
        }

        if (piece.row - 1 > 0)
        {
            Piece left = GetPiece(piece.row - 1, piece.column);
            if (matchedPieces.Contains(left) && !verifyedPieces.Contains(left) && 
                piece.value == left.value)
                return true;
        }

        if (piece.column + 1 < vertical - 1)
        {
            Piece up = GetPiece(piece.row, piece.column + 1);
            if (matchedPieces.Contains(up) && !verifyedPieces.Contains(up) && 
                piece.value == up.value)
                return true;
        }

        if (piece.column - 1 > 0)
        {
            Piece down = GetPiece(piece.row, piece.column - 1);
            if (matchedPieces.Contains(down) && !verifyedPieces.Contains(down) &&
                piece.value == down.value)
                return true;
        }

        return false;
    }

    private void UsedItem(Piece piece1, Piece piece2, Piece piece3)
    {
        if (piece1.rowBomb)
        {
            piece1.rowBomb = false;
            GetRowPieces(piece1.column);
            piece1.PieceEffectPlay(PieceEffect.ROWBOMB);
        }
        if (piece2.rowBomb)
        {
            piece2.rowBomb = false;
            GetRowPieces(piece2.column);
            piece2.PieceEffectPlay(PieceEffect.ROWBOMB);
        }
        if (piece3.rowBomb)
        {
            piece3.rowBomb = false;
            GetRowPieces(piece3.column);
            piece3.PieceEffectPlay(PieceEffect.ROWBOMB);
        }

        if (piece1.columnBomb)
        {
            piece1.columnBomb = false;
            GetColumnPieces(piece1.row);
            piece1.PieceEffectPlay(PieceEffect.COLUMNBOMB);
        }
        if (piece2.columnBomb)
        {
            piece2.columnBomb = false;
            GetColumnPieces(piece2.row);
            piece2.PieceEffectPlay(PieceEffect.COLUMNBOMB);
        }
        if (piece3.columnBomb)
        {
            piece3.columnBomb = false;
            GetColumnPieces(piece3.row);
            piece3.PieceEffectPlay(PieceEffect.COLUMNBOMB);
        }
    }

    private void UsedCrossBomb(Piece piece)
    {
        if (piece.crossBomb)
        {
            piece.crossBomb = false;
            GetRowPieces(piece.column);
            GetColumnPieces(piece.row);
            piece.PieceEffectPlay(PieceEffect.CROSSBOMB);
        }
    }

    private void GetRowPieces(int column)
    {
        for (int row = 0; row < horizontal; ++row)
        {
            if (pieceArray[row, column] != null)
            {
                Piece rowPiece = GetPiece(row, column);

                if(rowPiece.columnBomb)
                {
                    GetColumnPieces(rowPiece.row);
                    rowPiece.columnBomb = false;
                }

                if(rowPiece.crossBomb)
                {
                    UsedCrossBomb(rowPiece);
                    rowPiece.crossBomb = false;
                }

                if (!matchedPieces.Contains(rowPiece))
                    matchedPieces.Add(rowPiece);

                if (!verifyedPieces.Contains(rowPiece))
                    verifyedPieces.Add(rowPiece);
            }
        }
    }

    private void GetColumnPieces(int row)
    {
        for (int column = 0; column < vertical; ++column)
        {
            if (pieceArray[row, column] != null)
            {
                Piece columnPiece = GetPiece(row, column);

                if (columnPiece.rowBomb)
                {
                    GetRowPieces(columnPiece.column);
                    columnPiece.rowBomb = false;
                }

                if (columnPiece.crossBomb)
                {
                    UsedCrossBomb(columnPiece);
                    columnPiece.crossBomb = false;
                }

                if (!matchedPieces.Contains(columnPiece))
                    matchedPieces.Add(columnPiece);

                if (!verifyedPieces.Contains(columnPiece))
                    verifyedPieces.Add(columnPiece);
            }
        }
    }

    IEnumerator DisabledMatchedPiece()
    {
        if (selectedPiece != null)
        {
            stageCtrl.DecreaseMove(stageCtrl.decreaseMoveValue);
            selectedPiece = null;
        }

        foreach (var piece in matchedPieces)
        {
            piece.PieceEffectPlay(PieceEffect.PIECEEXPLOSION);
            piece.SetDisabledPiece();
        }

        stageCtrl.SoundEffectPlay(SoundEffectList.MATCHED);

        yield return new WaitForSeconds(effectDelayTime);

        ++stageCtrl.combo;
        foreach (var piece in matchedPieces)
        {
            stageCtrl.IncreaseScore(stageCtrl.matchedScore);

            piece.transform.SetParent(disabledPiecesParent.transform);
            piece.gameObject.SetActive(false);

            disabledPieces.Add(piece);
        }

        matchedPieces.Clear();
        verifyedPieces.Clear();

        StartCoroutine(FallingPieces());
    }

    IEnumerator FallingPieces()
    {
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (pieceArray[row, column] == null)
                {
                    for (int i = column + 1; i < vertical; ++i)
                    {
                        if (pieceArray[row, i] != null)
                        {
                            Piece fallPiece = GetPiece(row, i); // 빈 자리의 위에 있는 블럭
                            fallPiece.SettingPiece(this, $"[{fallPiece.row}  , {fallPiece.column}]",fallPiece.value, fallPiece. row, column);

                            pieceArray[row, i] = null;

                            fallPiece.currState = PieceState.MOVE;
                            break;
                        }
                    }
                }
            }
        }

        int fall = 0;

        for (int row = 0; row < horizontal; ++row)
        {
            for (int column = 0; column < vertical; ++column)
            {
                if (pieceArray[row, column] == null)
                {
                    Piece enabledPiece = disabledPieces[0];

                    enabledPiece.SettingPiece(this, $"[{row}  , {column}]", Random.Range(0, pieceSprites.Length), row, column);

                    enabledPiece.transform.SetParent(transform);
                    enabledPiece.transform.position = new Vector2(enabledPiece.row, vertical + fall);

                    enabledPiece.gameObject.SetActive(true);

                    enabledPiece.currState = PieceState.MOVE;
                    disabledPieces.RemoveAt(0);
                    ++fall;
                }
            }

            fall = 0;
        }

        while (FindMovingPiece())
            yield return null;

        yield return new WaitForSeconds(delayTime);

        FindMatchedPiece();
    }

    public Piece GetPiece(int row, int column)
    {
        return pieceArray[row, column].GetComponent<Piece>();
    }

    private bool DeadLockCheck()
    {
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (row < horizontal - 1)
                {
                    SwapBoardIndex(row, column, Vector2.right);

                    FindMatchedPiece(true);
                    if (isMatched)
                    {
                        SwapBoardIndex(row, column, Vector2.right);
                        hintPieces.Add(GetPiece(row, column));
                        hintPieces.Add(GetPiece(row + 1, column));
                        return false;
                    }

                    SwapBoardIndex(row, column, Vector2.right);
                }

                if (column < vertical - 1)
                {
                    SwapBoardIndex(row, column, Vector2.up);

                    FindMatchedPiece(true);
                    if (isMatched)
                    {
                        SwapBoardIndex(row, column, Vector2.up);
                        hintPieces.Add(GetPiece(row, column));
                        hintPieces.Add(GetPiece(row, column + 1));
                        return false;
                    }

                    SwapBoardIndex(row, column, Vector2.up);
                }
            }
        }

        return true;
    }

    private void SwapBoardIndex(int row, int column, Vector2 dir)
    {
        GameObject temp = pieceArray[row + (int)dir.x, column + (int)dir.y];
        pieceArray[row + (int)dir.x, column + (int)dir.y] = pieceArray[row, column];
        pieceArray[row, column] = temp;
    }

    private bool FindMovingPiece()
    {
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (pieceArray[row, column] != null)
                {
                    Piece movePiece = GetPiece(row, column);

                    if (movePiece.currState == PieceState.MOVE)
                        return true;
                }
            }
        }
        return false;
    }

    private void HintPieceSwitch(bool hintSwitch = false)
    {
        if (hintSwitch)
        {
            hintPieces[0].PieceEffectPlay(PieceEffect.HINTEFFECT);
            hintPieces[1].PieceEffectPlay(PieceEffect.HINTEFFECT);
        }

        else
        {
            hintPieces[0].PieceEffectStop(PieceEffect.HINTEFFECT);
            hintPieces[1].PieceEffectStop(PieceEffect.HINTEFFECT);

            hintPieces.Clear();
        }
    }

	public bool IsStageStopped()
	{
		return stageCtrl.currStageState == StageState.CLEAR || stageCtrl.currStageState == StageState.FAIL;
	}

	private void DebugSystem()
    {
        // debug Array Check
        if (Input.GetKeyDown(KeyCode.A))
            DebugBoardChecking(false);

        // debug Piece Moving Check
        if (Input.GetKeyDown(KeyCode.B))
            DebugBoardChecking(true);

        // debug Board Current State Check
        if (Input.GetKeyDown(KeyCode.D))
            Debug.Log(currBoardState);

        // debug Deadlock Check
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (DeadLockCheck())
                Debug.Log("is DeadLock !!");
        }

        // debug Item Check
        if (Input.GetKeyDown(KeyCode.G))
            DebugBoardItemChecking();

        // debug Board Reset
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            selectedPiece = null;
            GeneratePiece(true);
        }

        // debug Generate Item
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selectedPiece != null)
            {
                selectedPiece.itemSprite.sprite = itemSprites[0];
                selectedPiece.rowBomb = true;
            }
        }

        // debug Generate Item
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (selectedPiece != null)
            {
                selectedPiece.itemSprite.sprite = itemSprites[1];
                selectedPiece.columnBomb = true;
            }
        }

        // debug Generate Item
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (selectedPiece != null)
            {
                selectedPiece.itemSprite.sprite = itemSprites[2];
                selectedPiece.crossBomb = true;
            }
        }
    }

    private void DebugBoardChecking(bool moving = false)
    {
        StringBuilder sb = new StringBuilder(1000);

        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (pieceArray[row, column] != null)
                {
                    Piece piece = GetPiece(row, column);
                    /*if (boardIndex[row, column].GetComponent<Block>().value == GetPiece(row, column).value)
                        Debug.Log("[" + row + " , " + column + "] = " + GetPiece(row, column).value);*/

                    if (!moving)
                    {
                        sb.Append($"[{row} , {column}] = {piece.value} || PosX : {piece.transform.position.x} , PosY :  {piece.transform.position.y} \n");
                        //Debug.Log("[" + row + " , " + column + "] = " + piece.value + ", PosX : " + piece.transform.position.x + ", PosY : " + piece.transform.position.y);
                    }

                    else
                    {
                        if (piece.currState == PieceState.MOVE)
                            Debug.Log($"[{row} , {column}]] is Moving");
                    }
                }
            }
        }

        Debug.Log(sb.ToString());
        //sb.Clear();
    }

    private void DebugBoardItemChecking()
    {
        for (int column = 0; column < vertical; ++column)
        {
            for (int row = 0; row < horizontal; ++row)
            {
                if (pieceArray[row, column] != null)
                {
                    Piece piece = GetPiece(row, column);

                    if (piece.rowBomb || piece.columnBomb || piece.crossBomb)
                        Debug.Log("is itemPiece");
                }
            }
        }
    }

    /*public void PieceListSort(List<Piece> pieces, bool column = false, bool descending = false)
    {
        if (column)
        {
            if (descending)
            {
                pieces.Sort(delegate (Piece a, Piece b)
                {
                    if (a.column < b.column)
                        return 1;

                    else if (a.column > b.column)
                        return -1;

                    else
                        return 0;
                });
            }

            else
            {
                pieces.Sort(delegate (Piece a, Piece b)
                {
                    if (a.column > b.column)
                        return 1;

                    else if (a.column < b.column)
                        return -1;

                    else
                        return 0;
                });
            }
        }

        else
        {
            if (descending)
            {
                pieces.Sort(delegate (Piece a, Piece b)
                {
                    if (a.row < b.row)
                        return 1;

                    else if (a.row > b.row)
                        return -1;

                    else
                        return 0;
                });
            }

            else
            {
                pieces.Sort(delegate (Piece a, Piece b)
                {
                    if (a.row > b.row)
                        return 1;

                    else if (a.row < b.row)
                        return -1;

                    else
                        return 0;
                });
            }
        }
    }*/
}