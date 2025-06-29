using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap chessTilemap;

    [Header("Generic Piece Prefab")]
    // 기존에 각 기물 Prefab 대신 이 하나만 사용
    public GameObject genericPiecePrefab;

    [Header("Piece Sprites")]
    // White
    public Sprite whiteKingSprite;
    public Sprite whiteQueenSprite;
    public Sprite whiteRookSprite;
    public Sprite whiteBishopSprite;
    public Sprite whiteKnightSprite;
    public Sprite whitePawnSprite;
    // Black
    public Sprite blackKingSprite;
    public Sprite blackQueenSprite;
    public Sprite blackRookSprite;
    public Sprite blackBishopSprite;
    public Sprite blackKnightSprite;
    public Sprite blackPawnSprite;

    // 내부 좌표 시스템: [x, y] → Piece
    private Piece[,] board = new Piece[8, 8];

    public Piece[,] Board => board;

    // (추가) 타입·색상 → 스프라이트 매핑
    private Dictionary<(PieceType, PieceColor), Sprite> _spriteMap;

    void Awake()
    {
        // 매핑 초기화
        _spriteMap = new Dictionary<(PieceType, PieceColor), Sprite>()
        {
            { (PieceType.King,   PieceColor.White), whiteKingSprite   },
            { (PieceType.Queen,  PieceColor.White), whiteQueenSprite  },
            { (PieceType.Rook,   PieceColor.White), whiteRookSprite   },
            { (PieceType.Bishop, PieceColor.White), whiteBishopSprite },
            { (PieceType.Knight, PieceColor.White), whiteKnightSprite },
            { (PieceType.Pawn,   PieceColor.White), whitePawnSprite   },

            { (PieceType.King,   PieceColor.Black), blackKingSprite   },
            { (PieceType.Queen,  PieceColor.Black), blackQueenSprite  },
            { (PieceType.Rook,   PieceColor.Black), blackRookSprite   },
            { (PieceType.Bishop, PieceColor.Black), blackBishopSprite },
            { (PieceType.Knight, PieceColor.Black), blackKnightSprite },
            { (PieceType.Pawn,   PieceColor.Black), blackPawnSprite   },
        };
    }

    void Start()
    {
        // 예: Pawn만 배치해보기
        for (int x = 0; x < 8; x++)
        {
            SpawnPiece(PieceType.Pawn, PieceColor.White, new Vector2Int(x, 1));
            SpawnPiece(PieceType.Pawn, PieceColor.Black, new Vector2Int(x, 6));
        }
        SpawnPiece(PieceType.Rook, PieceColor.White, new Vector2Int(0, 0));
        SpawnPiece(PieceType.Rook, PieceColor.White, new Vector2Int(7, 0));
        SpawnPiece(PieceType.Knight, PieceColor.White, new Vector2Int(1, 0));
        SpawnPiece(PieceType.Knight, PieceColor.White, new Vector2Int(6, 0));
        SpawnPiece(PieceType.Bishop, PieceColor.White, new Vector2Int(2, 0));
        SpawnPiece(PieceType.Bishop, PieceColor.White, new Vector2Int(5, 0));
        SpawnPiece(PieceType.Queen, PieceColor.White, new Vector2Int(3, 0));
        SpawnPiece(PieceType.King, PieceColor.White, new Vector2Int(4, 0));

        SpawnPiece(PieceType.Rook, PieceColor.Black, new Vector2Int(0, 7));
        SpawnPiece(PieceType.Rook, PieceColor.Black, new Vector2Int(7, 7));
        SpawnPiece(PieceType.Knight, PieceColor.Black, new Vector2Int(1, 7));
        SpawnPiece(PieceType.Knight, PieceColor.Black, new Vector2Int(6, 7));
        SpawnPiece(PieceType.Bishop, PieceColor.Black, new Vector2Int(2, 7));
        SpawnPiece(PieceType.Bishop, PieceColor.Black, new Vector2Int(5, 7));
        SpawnPiece(PieceType.Queen, PieceColor.Black, new Vector2Int(3, 7));
        SpawnPiece(PieceType.King, PieceColor.Black, new Vector2Int(4, 7));
    }

    // 수정된 SpawnPiece: prefab 대신 Generic + sprite 교체
    void SpawnPiece(PieceType type, PieceColor color, Vector2Int pos)
    {
        Vector3 worldPos = chessTilemap.GetCellCenterWorld((Vector3Int)pos);
        var go = Instantiate(genericPiecePrefab, worldPos, Quaternion.identity, transform);

        // 스프라이트 설정
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = _spriteMap[(type, color)];

        // Piece 컴포넌트 초기화
        var pc = go.GetComponent<Piece>();
        pc.Init(type, color, pos);

        board[pos.x, pos.y] = pc;
    }

    // 말 이동 함수
    public bool MovePiece(Vector2Int from, Vector2Int to)
    {
        // 유효 범위 검사
        if (!IsInBounds(from) || !IsInBounds(to))
            return false;

        var moving = board[from.x, from.y];
        if (moving == null)
            return false;

        // 캡처 처리: 목표 위치에 상대 기물이 있으면 제거
        var target = board[to.x, to.y];
        if (target != null && target.color != moving.color)
        {
            Destroy(target.gameObject);
            board[to.x, to.y] = null;
        }

        // 배열 업데이트
        board[from.x, from.y] = null;
        board[to.x, to.y] = moving;
        moving.boardPos = to;

        // 월드 포지션 갱신
        moving.transform.position = chessTilemap.GetCellCenterWorld((Vector3Int)to);
        return true;
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }



    // (옵션) 해당 셀의 기물 반환
    public Piece GetPieceAt(Vector2Int pos)
        => board[pos.x, pos.y];


}
