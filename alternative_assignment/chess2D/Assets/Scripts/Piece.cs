using UnityEngine;

public enum PieceType { King, Queen, Rook, Bishop, Knight, Pawn }
public enum PieceColor { White, Black }

public class Piece : MonoBehaviour
{
    // ���� ������ ��
    public PieceType type;
    public PieceColor color;

    // ���� ��ǥ: (0~7, 0~7)
    public Vector2Int boardPos;

    // ���忡 ��ġ�� �� �ʱ�ȭ
    public void Init(PieceType type, PieceColor color, Vector2Int pos)
    {
        this.type = type;
        this.color = color;
        this.boardPos = pos;
        name = $"{color}_{type}_{pos.x}{pos.y}";
    }

}
