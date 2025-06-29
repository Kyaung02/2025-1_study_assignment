using UnityEngine;

public enum PieceType { King, Queen, Rook, Bishop, Knight, Pawn }
public enum PieceColor { White, Black }

public class Piece : MonoBehaviour
{
    // 말의 종류와 색
    public PieceType type;
    public PieceColor color;

    // 내부 좌표: (0~7, 0~7)
    public Vector2Int boardPos;

    // 보드에 배치될 때 초기화
    public void Init(PieceType type, PieceColor color, Vector2Int pos)
    {
        this.type = type;
        this.color = color;
        this.boardPos = pos;
        name = $"{color}_{type}_{pos.x}{pos.y}";
    }

}
