using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Collections;

/// <summary>
/// 체스 입력 컨트롤러: 클릭 및 드래그/드롭 이동, 이동 옵션 하이라이트, 이전 이동 색강조,
/// 이동 후 체크메이트 판정 및 결과 텍스트 표시
/// </summary>
public class ChessInputController : MonoBehaviour
{
    [Header("References")]
    public BoardManager boardManager;
    public Tilemap chessTilemap;
    public GameObject highlightPrefab;

    public GameObject resultPanel;
    public Text resultText;  // UI 텍스트 컴포넌트 연결

    [Header("Highlight Colors")]
    public Color moveHighlightColor = new Color(0f, 1f, 0f, 0.5f);
    public Color lastMoveColor = new Color(1f, 1f, 0f, 0.5f);

    private PieceColor _currentTurn = PieceColor.White;

    // Selection & move
    private Piece _selectedPiece;
    private Vector2Int _startBoardPos = new Vector2Int(-1, -1);
    private Vector3 _startWorldPos;
    private List<Vector2Int> _legalMoves = new List<Vector2Int>();
    private List<GameObject> _moveHighlights = new List<GameObject>();

    // Selection tile color restore
    private Vector3Int _selectionCell = new Vector3Int(-1, -1, 0);
    private Color _selectionOrig;

    // Last move highlight cells & colors
    private Vector3Int _prevFrom = new Vector3Int(-1, -1, 0);
    private Vector3Int _prevTo = new Vector3Int(-1, -1, 0);
    private Color _prevFromColor;
    private Color _prevToColor;

    // Drag detection
    private bool _mouseDown;
    private Vector2 _mouseDownPos;
    private bool _isDragging;
    private const float DragThreshold = 10f;

    void Start()
    {
        resultPanel.SetActive(false);
    }

    private IEnumerator ShowResult(string message)
    {
        resultText.text = message;
        resultPanel.SetActive(true);
        resultText.canvasRenderer.SetAlpha(0f);
        resultText.CrossFadeAlpha(1f, 0.5f, false);
        yield return null;
    }


    void Update()
    {
        // Mouse Down
        if (Input.GetMouseButtonDown(0))
        {
            Vector2Int pos = GetBoardPositionUnderMouse();
            var piece = boardManager.GetPieceAt(pos);
            if (piece != null && piece.color == _currentTurn)
            {
                SelectPiece(piece, pos);
                _mouseDown = true;
                _mouseDownPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                _isDragging = false;
            }
            else if (_selectedPiece != null && !_isDragging)
            {
                // Click Move
                if (_legalMoves.Contains(pos) && boardManager.MovePiece(_startBoardPos, pos))
                    HandlePostMove(pos);
                else
                    ClearSelection();
                _mouseDown = false;
            }
        }

        // Drag start detect
        if (_mouseDown && !_isDragging && Input.GetMouseButton(0))
        {
            Vector2 cur = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            if ((cur - _mouseDownPos).magnitude > DragThreshold)
                _isDragging = true;
        }

        // Dragging
        if (_isDragging && _selectedPiece != null)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0;
            _selectedPiece.transform.position = world;
            if (Input.GetMouseButtonUp(0))
                FinishDrag();
            return;
        }

        // Mouse up without drag
        if (_mouseDown && !_isDragging && Input.GetMouseButtonUp(0))
            _mouseDown = false;
    }

    private void SelectPiece(Piece piece, Vector2Int pos)
    {
        ClearSelection();
        _selectedPiece = piece;
        _startBoardPos = pos;
        _startWorldPos = chessTilemap.GetCellCenterWorld((Vector3Int)pos);
        _legalMoves = MoveGenerator.GetLegalMoves(boardManager.Board, pos);

        // Highlight selection tile
        _selectionCell = new Vector3Int(pos.x, pos.y, 0);
        chessTilemap.SetTileFlags(_selectionCell, TileFlags.None);
        _selectionOrig = chessTilemap.GetColor(_selectionCell);
        chessTilemap.SetColor(_selectionCell, lastMoveColor);

        // Highlight legal moves
        foreach (var m in _legalMoves)
        {
            Vector3 center = chessTilemap.GetCellCenterWorld((Vector3Int)m);
            var go = Instantiate(highlightPrefab, center, Quaternion.identity, chessTilemap.transform);
            go.GetComponent<SpriteRenderer>().color = moveHighlightColor;
            _moveHighlights.Add(go);
        }
    }

    private void FinishDrag()
    {
        _mouseDown = false;
        _isDragging = false;
        Vector2Int drop = GetBoardPositionUnderMouse();
        if (_legalMoves.Contains(drop) && boardManager.MovePiece(_startBoardPos, drop))
            HandlePostMove(drop);
        else
        {
            _selectedPiece.transform.position = _startWorldPos;
            ClearSelection();
        }
    }

    private void HandlePostMove(Vector2Int to)
    {
        // Clear highlights & selection restore
        ClearSelection();

        // Last move tile highlight
        if (_prevFrom.x >= 0)
        {
            chessTilemap.SetTileFlags(_prevFrom, TileFlags.None);
            chessTilemap.SetColor(_prevFrom, _prevFromColor);
        }
        if (_prevTo.x >= 0)
        {
            chessTilemap.SetTileFlags(_prevTo, TileFlags.None);
            chessTilemap.SetColor(_prevTo, _prevToColor);
        }
        var fromCell = new Vector3Int(_startBoardPos.x, _startBoardPos.y, 0);
        var toCell = new Vector3Int(to.x, to.y, 0);
        _prevFromColor = chessTilemap.GetColor(fromCell);
        _prevToColor = chessTilemap.GetColor(toCell);
        chessTilemap.SetTileFlags(fromCell, TileFlags.None);
        chessTilemap.SetColor(fromCell, lastMoveColor);
        chessTilemap.SetTileFlags(toCell, TileFlags.None);
        chessTilemap.SetColor(toCell, lastMoveColor);
        _prevFrom = fromCell;
        _prevTo = toCell;


        PieceColor nextTurn = _currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

        // Check for checkmate
        if (MoveGenerator.IsCheckmate(boardManager.Board, nextTurn))
        {
            var winner = _currentTurn.ToString();
            StartCoroutine(ShowResult($"Checkmate!\n{winner} wins."));
        }
        else if (MoveGenerator.IsStalemate(boardManager.Board, nextTurn))
            StartCoroutine(ShowResult("Stalemate!\nDraw."));
        else
            _currentTurn = nextTurn;
        }

    private void ClearSelection()
    {
        // restore selection tile color
        if (_selectionCell.x >= 0)
        {
            chessTilemap.SetTileFlags(_selectionCell, TileFlags.None);
            chessTilemap.SetColor(_selectionCell, _selectionOrig);
            _selectionCell = new Vector3Int(-1, -1, 0);
        }
        foreach (var go in _moveHighlights)
            Destroy(go);
        _moveHighlights.Clear();
        _selectedPiece = null;
        _legalMoves.Clear();
    }

    private Vector2Int GetBoardPositionUnderMouse()
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = chessTilemap.WorldToCell(world);
        return new Vector2Int(cell.x, cell.y);
    }
}
