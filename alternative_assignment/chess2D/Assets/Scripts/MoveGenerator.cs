using System.Collections.Generic;
using UnityEngine;

public static class MoveGenerator
{
    private const int BOARD_SIZE = 8;

    public static List<Vector2Int> GetLegalMoves(Piece[,] board, Vector2Int from)
    {
        var piece = board[from.x, from.y];
        var possible = new List<Vector2Int>();
        if (piece == null || !InBounds(from))
            return possible;

        switch (piece.type)
        {
            case PieceType.Pawn:
                possible.AddRange(GetPawnMoves(board, from, piece.color));
                break;
            case PieceType.Knight:
                possible.AddRange(GetKnightMoves(board, from, piece.color));
                break;
            case PieceType.Bishop:
                possible.AddRange(GetSlidingMoves(board, from, piece.color, diagonal: true, straight: false));
                break;
            case PieceType.Rook:
                possible.AddRange(GetSlidingMoves(board, from, piece.color, diagonal: false, straight: true));
                break;
            case PieceType.Queen:
                possible.AddRange(GetSlidingMoves(board, from, piece.color, diagonal: true, straight: true));
                break;
            case PieceType.King:
                possible.AddRange(GetKingMoves(board, from, piece.color));
                break;
        }

        // 체크 상황 고려: 시뮬레이션하여 왕이 체크되지 않는 수만 남김
        var legal = new List<Vector2Int>();
        foreach (var to in possible)
        {
            var sim = CloneBoard(board);
            var moving = sim[from.x, from.y];
            sim[from.x, from.y] = null;
            sim[to.x, to.y] = moving;
            if (!IsKingInCheck(sim, piece.color))
                legal.Add(to);
        }
        return legal;
    }

    public static bool IsCheckmate(Piece[,] board, PieceColor color)
    {
        if (!IsKingInCheck(board, color))
            return false;

        for (int x = 0; x < BOARD_SIZE; x++)
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                var p = board[x, y];
                if (p != null && p.color == color)
                {
                    var moves = GetLegalMoves(board, new Vector2Int(x, y));
                    if (moves.Count > 0)
                        return false;
                }
            }
        return true;
    }

    /// <summary>
    /// 스테일메이트 판정: 체크가 아니면서 합법 수가 하나도 없으면 true
    /// </summary>
    public static bool IsStalemate(Piece[,] board, PieceColor color)
    {
        if (IsKingInCheck(board, color))
            return false;
        for (int x = 0; x < BOARD_SIZE; x++)
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                var p = board[x, y];
                if (p != null && p.color == color)
                {
                    if (GetLegalMoves(board, new Vector2Int(x, y)).Count > 0)
                        return false;
                }
            }
        return true;
    }

    // ── 체크 감지 ──────────────────────────────────────

    private static bool IsKingInCheck(Piece[,] board, PieceColor color)
    {
        // 왕 위치 찾기
        Vector2Int kingPos = new Vector2Int(-1, -1);
        for (int x = 0; x < BOARD_SIZE; x++)
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                var p = board[x, y];
                if (p != null && p.type == PieceType.King && p.color == color)
                {
                    kingPos = new Vector2Int(x, y);
                    break;
                }
            }
        if (!InBounds(kingPos)) return false;

        // 상대 기물의 공격 경로에 왕이 있는지
        for (int x = 0; x < BOARD_SIZE; x++)
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                var p = board[x, y];
                if (p != null && p.color != color)
                {
                    List<Vector2Int> attacks = p.type switch
                    {
                        PieceType.Pawn => GetPawnAttackSquares(board, new Vector2Int(x, y), p.color),
                        PieceType.Knight => GetKnightMoves(board, new Vector2Int(x, y), p.color),
                        PieceType.Bishop => GetSlidingMoves(board, new Vector2Int(x, y), p.color, true, false),
                        PieceType.Rook => GetSlidingMoves(board, new Vector2Int(x, y), p.color, false, true),
                        PieceType.Queen => GetSlidingMoves(board, new Vector2Int(x, y), p.color, true, true),
                        PieceType.King => GetKingMoves(board, new Vector2Int(x, y), p.color),
                        _ => new List<Vector2Int>()
                    };
                    if (attacks.Contains(kingPos))
                        return true;
                }
            }

        return false;
    }

    private static Piece[,] CloneBoard(Piece[,] board)
    {
        var copy = new Piece[BOARD_SIZE, BOARD_SIZE];
        for (int x = 0; x < BOARD_SIZE; x++)
            for (int y = 0; y < BOARD_SIZE; y++)
                copy[x, y] = board[x, y];
        return copy;
    }

    // ── 기물별 이동 생성 ─────────────────────────────────

    private static List<Vector2Int> GetPawnMoves(Piece[,] board, Vector2Int pos, PieceColor color)
    {
        var moves = new List<Vector2Int>();
        int dir = (color == PieceColor.White) ? 1 : -1;
        var one = new Vector2Int(pos.x, pos.y + dir);
        if (InBounds(one) && board[one.x, one.y] == null)
            moves.Add(one);

        bool startRank = (color == PieceColor.White && pos.y == 1) || (color == PieceColor.Black && pos.y == 6);
        var two = new Vector2Int(pos.x, pos.y + 2 * dir);
        if (startRank && InBounds(two) && board[one.x, one.y] == null && board[two.x, two.y] == null)
            moves.Add(two);

        foreach (int dx in new[] { -1, 1 })
        {
            var diag = new Vector2Int(pos.x + dx, pos.y + dir);
            if (InBounds(diag) && board[diag.x, diag.y] != null && board[diag.x, diag.y].color != color)
                moves.Add(diag);
        }

        return moves;
    }

    private static List<Vector2Int> GetPawnAttackSquares(Piece[,] board, Vector2Int pos, PieceColor color)
    {
        var attacks = new List<Vector2Int>();
        int dir = (color == PieceColor.White) ? 1 : -1;
        foreach (int dx in new[] { -1, 1 })
        {
            var d = new Vector2Int(pos.x + dx, pos.y + dir);
            if (InBounds(d))
                attacks.Add(d);
        }
        return attacks;
    }

    private static List<Vector2Int> GetKnightMoves(Piece[,] board, Vector2Int pos, PieceColor color)
    {
        var moves = new List<Vector2Int>();
        Vector2Int[] deltas = {
            new Vector2Int(1,2), new Vector2Int(2,1), new Vector2Int(2,-1), new Vector2Int(1,-2),
            new Vector2Int(-1,-2), new Vector2Int(-2,-1), new Vector2Int(-2,1), new Vector2Int(-1,2)
        };
        foreach (var d in deltas)
        {
            var t = pos + d;
            if (!InBounds(t)) continue;
            var occ = board[t.x, t.y];
            if (occ == null || occ.color != color)
                moves.Add(t);
        }
        return moves;
    }

    private static List<Vector2Int> GetSlidingMoves(Piece[,] board, Vector2Int pos, PieceColor color, bool diagonal, bool straight)
    {
        var moves = new List<Vector2Int>();
        var dirs = new List<Vector2Int>();
        if (straight)
        {
            dirs.Add(new Vector2Int(1, 0)); dirs.Add(new Vector2Int(-1, 0));
            dirs.Add(new Vector2Int(0, 1)); dirs.Add(new Vector2Int(0, -1));
        }
        if (diagonal)
        {
            dirs.Add(new Vector2Int(1, 1)); dirs.Add(new Vector2Int(1, -1));
            dirs.Add(new Vector2Int(-1, 1)); dirs.Add(new Vector2Int(-1, -1));
        }
        foreach (var dir in dirs)
        {
            var cur = pos;
            while (true)
            {
                cur += dir;
                if (!InBounds(cur)) break;
                var occ = board[cur.x, cur.y];
                if (occ == null)
                {
                    moves.Add(cur);
                    continue;
                }
                if (occ.color != color)
                    moves.Add(cur);
                break;
            }
        }
        return moves;
    }

    private static List<Vector2Int> GetKingMoves(Piece[,] board, Vector2Int pos, PieceColor color)
    {
        var moves = new List<Vector2Int>();
        Vector2Int[] deltas = {
            new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(0,1), new Vector2Int(-1,1),
            new Vector2Int(-1,0), new Vector2Int(-1,-1), new Vector2Int(0,-1), new Vector2Int(1,-1)
        };
        foreach (var d in deltas)
        {
            var t = pos + d;
            if (!InBounds(t)) continue;
            var occ = board[t.x, t.y];
            if (occ == null || occ.color != color)
                moves.Add(t);
        }
        return moves;
    }

    private static bool InBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < BOARD_SIZE && pos.y >= 0 && pos.y < BOARD_SIZE;
    }
}
