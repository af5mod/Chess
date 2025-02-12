namespace ChessLogic
{
    public enum Player
    {
        White,
        Black,
        None
    }

    public enum FigureType
    {
        Pawn,
        Bishop,
        Rook,
        Queen,
        King,
        Knight
    }

    public enum MoveType
    {
        Normal,
        CastleKS,
        CastleQS,
        PawnPromotion,
        EnPassant,
        DoublePawn
    }

    public enum EndReason
    {
        Checkmate,
        StaleMate,
        None
    }
}
