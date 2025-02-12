using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ChessLogic
{
    public static class PlayerExtensions
    {
        public static Player Opponent(this Player player)
        {
            switch (player)
            {
                case Player.White:
                    return Player.Black;
                case Player.Black:
                    return Player.White;
                default:
                    return Player.None;
            }

        }
    }

    public class Position
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public Position(int row, int col)
        {
            Row = row; Col = col;
        }

        public Player SquareColor()
        {
            if ((Row + Col) % 2 == 0)
            {
                return Player.White;
            }
            else
            {
                return Player.Black;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Position position &&
                   Row == position.Row &&
                   Col == position.Col;
        }

        public override int GetHashCode()
        {
            int hashCode = 1084646500;
            hashCode = hashCode * -1521134295 + Row.GetHashCode();
            hashCode = hashCode * -1521134295 + Col.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Position left, Position right)
        {
            return EqualityComparer<Position>.Default.Equals(left, right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }

        public static Position operator +(Position pos, Direction dir)
        {
            return new Position(pos.Row + dir.RowDirection, pos.Col + dir.ColDirection);
        }
    }

    public class Direction
    {
        public readonly static Direction North = new Direction(-1, 0);
        public readonly static Direction South = new Direction(1, 0);
        public readonly static Direction East = new Direction(0, 1);
        public readonly static Direction West = new Direction(0, -1);
        public readonly static Direction NorthEast = North + East;
        public readonly static Direction NorthWest = North + West;
        public readonly static Direction SouthEast = South + East;
        public readonly static Direction SouthWest = South + West;
        public int RowDirection { get; set; }
        public int ColDirection { get; set; }
        public Direction(int rowDirection, int colDirection)
        {
            RowDirection = rowDirection;
            ColDirection = colDirection;
        }
        public static Direction operator +(Direction left, Direction right)
        {
            return new Direction(left.RowDirection + right.RowDirection, left.ColDirection + right.ColDirection);
        }

        public static Direction operator *(Direction dir, int scalar)
        {
            return new Direction(dir.RowDirection * scalar, dir.ColDirection * scalar);
        }
    }

    public abstract class Move
    {
        public abstract MoveType Type { get; }
        public abstract Position FromPos { get; }
        public abstract Position ToPos { get; }
        public abstract void Complete(Board board);

        public virtual bool IsLegalMove(Board board)
        {
            Player player = board[FromPos].Color;
            Board boardCopy = board.Copy();
            Complete(boardCopy);
            return !boardCopy.IsInCheck(player);
        }
    }

    public class NormalMove : Move
    {
        public override MoveType Type => MoveType.Normal;
        public override Position FromPos { get; }
        public override Position ToPos { get; }
        public NormalMove(Position fromPos, Position toPos)
        {
            FromPos = fromPos;
            ToPos = toPos;
        }
        public override void Complete(Board board)
        {
            Figure figure = board[FromPos];
            board[ToPos] = figure;
            board[FromPos] = null;
            figure.HasMoved = true;
        }
    }
    public class PawnPromotion : Move
    {
        public override MoveType Type => MoveType.PawnPromotion;
        public override Position FromPos { get; }
        public override Position ToPos { get; }
        private readonly FigureType newType;
        public PawnPromotion(Position fromPos, Position toPos, FigureType newType)
        {
            FromPos = fromPos;
            ToPos = toPos;
            this.newType = newType;
        }

        public Figure CreatePromotionFigure(Player color)
        {
            if (newType.Equals(FigureType.Queen))
            {
                return new Queen(color);
            }
            else if (newType.Equals(FigureType.Rook))
            {
                return new Rook(color);
            }
            else if (newType.Equals(FigureType.Knight))
            {
                return new Knight(color);
            }
            else
            {
                return new Bishop(color);
            }
        }
        public override void Complete(Board board)
        {
            Figure pawn = board[FromPos];
            board[FromPos] = null;

            Figure promotionFigure = CreatePromotionFigure(pawn.Color);
            board[ToPos] = promotionFigure;
            promotionFigure.HasMoved = true;
        }
    }
    public class Castle : Move
    {
        public override MoveType Type { get; }
        public override Position FromPos { get; }
        public override Position ToPos { get; }
        private readonly Direction kingMoveDir;
        private readonly Position rookToPos;
        private readonly Position rookFromPos;
        public Castle(MoveType type, Position kingPos)
        {
            Type = type;
            FromPos = kingPos;
            if (type == MoveType.CastleKS)
            {
                kingMoveDir = Direction.East;
                ToPos = new Position(kingPos.Row, 6);
                rookFromPos = new Position(kingPos.Row, 7);
                rookToPos = new Position(kingPos.Row, 5);
            }
            else if (type == MoveType.CastleQS)
            {
                kingMoveDir = Direction.West;
                ToPos = new Position(kingPos.Row, 2);
                rookFromPos = new Position(kingPos.Row, 0);
                rookToPos = new Position(kingPos.Row, 3);
            }
        }

        public override void Complete(Board board)
        {
            new NormalMove(FromPos, ToPos).Complete(board);
            new NormalMove(rookFromPos, rookToPos).Complete(board);
        }
        public override bool IsLegalMove(Board board)
        {
            Player player = board[FromPos].Color;
            if (board.IsInCheck(player))
            {
                return false;
            }
            Board copy = board.Copy();
            Position kingPosInCopy = FromPos;
            for (int i = 0; i < 2; i++)
            {
                new NormalMove(kingPosInCopy, kingPosInCopy + kingMoveDir).Complete(copy);
                kingPosInCopy += kingMoveDir;

                if (copy.IsInCheck(player))
                {
                    return false;
                }
            }
            return true;
        }
    }
    public class DoublePawn : Move
    {
        public override MoveType Type => MoveType.DoublePawn;
        public override Position FromPos { get; }
        public override Position ToPos { get; }
        private readonly Position skippedPos;
        public DoublePawn(Position from, Position to)
        {
            FromPos = from;
            ToPos = to;
            skippedPos = new Position((from.Row + to.Row) / 2, from.Col);
        }
        public override void Complete(Board board)
        {
            Player player = board[FromPos].Color;
            board.SetPawnSkipPosition(player, skippedPos);
            new NormalMove(FromPos, ToPos).Complete(board);
        }
    }
    public class EnPassant : Move
    {
        public override MoveType Type => MoveType.EnPassant;
        public override Position FromPos { get; }
        public override Position ToPos { get; }
        private readonly Position attackPos;
        public EnPassant(Position from, Position to)
        {
            FromPos = from;
            ToPos = to;
            attackPos = new Position(from.Row, to.Col);
        }
        public override void Complete(Board board)
        {
            new NormalMove(FromPos, ToPos).Complete(board);
            board[attackPos] = null;
        }
    }

    public abstract class Figure
    {
        public abstract FigureType Type { get; }
        public abstract Player Color { get; }
        public bool HasMoved { get; set; } = false;

        public abstract Figure Copy();

        public abstract IEnumerable<Move> GetMoves(Position from, Board board);
        protected IEnumerable<Position> MovePositionsInDirection(Position from, Board board, Direction dir)
        {
            for (Position pos = from + dir; Board.IsInside(pos); pos += dir)
            {
                if (board.IsEmpty(pos))
                {
                    yield return pos;
                    continue;
                }
                Figure figure = board[pos];
                if (figure.Color != Color)
                {
                    yield return pos;
                }
                yield break;
            }
        }
        protected IEnumerable<Position> MovePositionsInDirections(Position from, Board board, Direction[] dirs)
        {
            return dirs.SelectMany(dir => MovePositionsInDirection(from, board, dir));
        }

        public virtual bool CanCaptureOpponentKing(Position from, Board board)
        {
            return GetMoves(from, board).Any(move =>
            {
                Figure figure = board[move.ToPos];
                return figure != null && figure.Type == FigureType.King;
            });
        }
    }
    public class Pawn : Figure
    {
        public override FigureType Type => FigureType.Pawn;

        public override Player Color { get; }
        private readonly Direction forward;
        public Pawn(Player color)
        {
            Color = color;
            if (Color == Player.White)
            {
                forward = Direction.North;
            }
            else if (Color == Player.Black)
            {
                forward = Direction.South;
            }
        }
        public override Figure Copy()
        {
            Pawn copy = new Pawn(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        private static bool CanMoveTo(Position pos, Board board)
        {
            return Board.IsInside(pos) && (board.IsEmpty(pos));
        }

        private bool CanAttack(Position pos, Board board)
        {
            if (!Board.IsInside(pos) || board.IsEmpty(pos))
            {
                return false;
            }
            return board[pos].Color != Color;
        }

        private static IEnumerable<Move> PromotionMoves(Position from, Position to)
        {
            yield return new PawnPromotion(from, to, FigureType.Knight);
            yield return new PawnPromotion(from, to, FigureType.Queen);
            yield return new PawnPromotion(from, to, FigureType.Bishop);
            yield return new PawnPromotion(from, to, FigureType.Rook);
        }

        private IEnumerable<Move> ForwardMoves(Position from, Board board)
        {
            Position oneMovePos = from + forward;
            if (CanMoveTo(oneMovePos, board))
            {
                if (oneMovePos.Row == 0 || oneMovePos.Col == 0)
                {
                    foreach (Move promMove in PromotionMoves(from, oneMovePos))
                    {
                        yield return promMove;
                    }
                }
                else
                {
                    yield return new NormalMove(from, oneMovePos);
                }
                Position twoMovesPos = oneMovePos + forward;
                if (!HasMoved && CanMoveTo(twoMovesPos, board))
                {
                    yield return new DoublePawn(from, twoMovesPos);
                }
            }
        }

        private IEnumerable<Move> DiagonalMoves(Position from, Board board)
        {
            foreach (Direction dir in new Direction[] { Direction.East, Direction.West })
            {
                Position to = from + dir + forward;

                if (to == board.GetPawnSkipPosition(Color.Opponent()))
                {
                    yield return new EnPassant(from, to);
                }

                else if (CanAttack(to, board))
                {
                    if (to.Row == 0 || to.Col == 0)
                    {
                        foreach (Move promMove in PromotionMoves(from, to))
                        {
                            yield return promMove;
                        }
                    }
                    else
                    {
                        yield return new NormalMove(from, to);
                    }
                }
            }
        }

        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            return ForwardMoves(from, board).Concat(DiagonalMoves(from, board));
        }

        public override bool CanCaptureOpponentKing(Position from, Board board)
        {
            return DiagonalMoves(from, board).Any(move =>
            {
                Figure figure = board[move.ToPos];
                return figure != null && figure.Type == FigureType.King;
            });
        }
    }

    public class Bishop : Figure
    {
        public override FigureType Type => FigureType.Bishop;

        public override Player Color { get; }

        private static readonly Direction[] dirs = new Direction[]
        {
                Direction.NorthEast,
                Direction.SouthWest,
                Direction.NorthWest,
                Direction.SouthEast
        };
        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            return MovePositionsInDirections(from, board, dirs).Select(to => new NormalMove(from, to));
        }
        public Bishop(Player color)
        {
            Color = color;
        }
        public override Figure Copy()
        {
            Bishop copy = new Bishop(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }
    }

    public class Knight : Figure
    {
        public override FigureType Type => FigureType.Knight;

        public override Player Color { get; }
        public Knight(Player color)
        {
            Color = color;
        }
        public override Figure Copy()
        {
            Knight copy = new Knight(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }
        private static IEnumerable<Position> PotenialToPos(Position from)
        {
            foreach (Direction vDir in new Direction[] { Direction.North, Direction.South })
            {
                foreach (Direction hDir in new Direction[] { Direction.East, Direction.West })
                {
                    yield return from + vDir * 2 + hDir;
                    yield return from + vDir + hDir * 2;
                }
            }
        }
        private IEnumerable<Position> MovePositions(Position from, Board board)
        {
            return PotenialToPos(from).Where(pos => Board.IsInside(pos) && (board.IsEmpty(pos) || board[pos].Color != Color));
        }
        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            return MovePositions(from, board).Select(to => new NormalMove(from, to));
        }

    }

    public class Rook : Figure
    {
        public override FigureType Type => FigureType.Rook;

        public override Player Color { get; }
        private static readonly Direction[] dirs = new Direction[]
        {
                Direction.North,
                Direction.South,
                Direction.West,
                Direction.East
        };
        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            return MovePositionsInDirections(from, board, dirs).Select(to => new NormalMove(from, to));
        }
        public Rook(Player color)
        {
            Color = color;
        }
        public override Figure Copy()
        {
            Rook copy = new Rook(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }
    }

    public class Queen : Figure
    {
        public override FigureType Type => FigureType.Queen;

        public override Player Color { get; }
        private static readonly Direction[] dirs = new Direction[]
        {
                Direction.North,
                Direction.South,
                Direction.West,
                Direction.East,
                Direction.NorthEast,
                Direction.SouthEast,
                Direction.NorthWest,
                Direction.SouthWest
        };
        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            return MovePositionsInDirections(from, board, dirs).Select(to => new NormalMove(from, to));
        }
        public Queen(Player color)
        {
            Color = color;
        }
        public override Figure Copy()
        {
            Queen copy = new Queen(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }
    }
    public class King : Figure
    {
        public override FigureType Type => FigureType.King;
        public override Player Color { get; }
        private static readonly Direction[] dirs = new Direction[]
        {
                Direction.North,
                Direction.South,
                Direction.West,
                Direction.East,
                Direction.NorthEast,
                Direction.SouthEast,
                Direction.NorthWest,
                Direction.SouthWest
        };
        public King(Player color)
        {
            Color = color;
        }
        public override Figure Copy()
        {
            King copy = new King(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }
        private IEnumerable<Position> MovePositions(Position from, Board board)
        {
            foreach (Direction dir in dirs)
            {
                Position to = from + dir;
                if (!Board.IsInside(to))
                {
                    continue;
                }
                if (board.IsEmpty(to) || board[to].Color != Color)
                {
                    yield return to;
                }
            }
        }

        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            foreach (Position to in MovePositions(from, board))
            {
                yield return new NormalMove(from, to);
            }

            if (CanCastleKS(from, board))
            {
                yield return new Castle(MoveType.CastleKS, from);
            }


            if (CanCastleQS(from, board))
            {
                yield return new Castle(MoveType.CastleQS, from);
            }
        }


        public override bool CanCaptureOpponentKing(Position from, Board board)
        {
            return MovePositions(from, board).Any(to =>
            {
                Figure figure = board[to];
                return figure != null && figure.Type == FigureType.King;
            });
        }

        private static bool IsUnmovedRook(Position pos, Board board)
        {
            if (board.IsEmpty(pos))
            {
                return false;
            }

            Figure figure = board[pos];
            return figure.Type == FigureType.Rook && !figure.HasMoved;
        }
        private static bool AllEmpty(IEnumerable<Position> positions, Board board)
        {
            return positions.All(pos => board.IsEmpty(pos));
        }
        public bool CanCastleKS(Position from, Board board)
        {
            if (HasMoved)
            {
                return false;
            }
            Position rookPos = new Position(from.Row, 7);
            Position[] betweenPositions = new Position[] {
                new Position (from.Row, 5),
                new Position (from.Row, 6)
            };

            return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
        }

        public bool CanCastleQS(Position from, Board board)
        {
            if (HasMoved)
            {
                return false;
            }
            Position rookPos = new Position(from.Row, 0);
            Position[] betweenPositions = new Position[] {
                new Position (from.Row, 1),
                new Position (from.Row, 2),
                new Position (from.Row, 3)
            };

            return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
        }
    }

    public class Board
    {
        private readonly Figure[,] board = new Figure[8, 8];
        private readonly Dictionary<Player, Position> pawnSkipPositions = new Dictionary<Player, Position>
        {
            { Player.White, null },
            { Player.Black, null },
        };
        public Figure this[int col, int row]
        {
            get { return board[col, row]; }
            set { board[col, row] = value; }
        }

        public Figure this[Position pos]
        {
            get { return this[pos.Row, pos.Col]; }
            set { this[pos.Row, pos.Col] = value; }
        }

        public Position GetPawnSkipPosition(Player player)
        {
            return pawnSkipPositions[player];
        }

        public void SetPawnSkipPosition(Player player, Position pos)
        {
            pawnSkipPositions[player] = pos;
        }

        public static Board Initial()
        {

            Board board = new Board();
            board[0, 0] = new Rook(Player.Black);
            board[0, 1] = new Knight(Player.Black);
            board[0, 2] = new Bishop(Player.Black);
            board[0, 3] = new Queen(Player.Black);
            board[0, 4] = new King(Player.Black);
            board[0, 5] = new Bishop(Player.Black);
            board[0, 6] = new Knight(Player.Black);
            board[0, 7] = new Rook(Player.Black);

            board[7, 0] = new Rook(Player.White);
            board[7, 1] = new Knight(Player.White);
            board[7, 2] = new Bishop(Player.White);
            board[7, 3] = new Queen(Player.White);
            board[7, 4] = new King(Player.White);
            board[7, 5] = new Bishop(Player.White);
            board[7, 6] = new Knight(Player.White);
            board[7, 7] = new Rook(Player.White);
            
            for (int i = 0; i < 8; i++)
            {
                board[1, i] = new Pawn(Player.Black);
                board[6, i] = new Pawn(Player.White);
            }
            
            /*
            Board board = new Board();
            board[1, 0] = new Pawn(Player.White);
            board[1, 1] = new Pawn(Player.White);
            board[0, 4] = new King(Player.Black);
            board[2, 5] = new Queen(Player.White);
            board[5, 5] = new Bishop(Player.White);
            board[7, 4] = new King(Player.White);
            */
            return board;
        }

        public static bool IsInside(Position pos)
        {
            return pos.Row >= 0 && pos.Col >= 0 && pos.Row < 8 && pos.Col < 8;
        }

        public bool IsEmpty(Position pos)
        {
            return this[pos] == null;
        }

        public Position FindKingPosition(Player player)
        {
            foreach (Position pos in FigurePositionsForColor(player))
            {
                if (this[pos].Type == FigureType.King)
                {
                    return pos;
                }
            }
            return null;
        }

        public IEnumerable<Position> FigurePositions()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Position pos = new Position(i, j);
                    if (!IsEmpty(pos))
                    {
                        yield return pos;
                    }
                }
            }
        }

        public IEnumerable<Position> FigurePositionsForColor(Player player)
        {
            return FigurePositions().Where(pos => this[pos].Color == player);
        }

        public bool IsInCheck(Player player)
        {
            return FigurePositionsForColor(player.Opponent()).Any(pos =>
            {
                Figure figure = this[pos];
                return figure.CanCaptureOpponentKing(pos, this);
            });
        }

        public Board Copy()
        {
            Board copy = new Board();
            foreach (Position pos in FigurePositions())
            {
                copy[pos] = this[pos].Copy();
            }
            return copy;
        }
    }

    public class Result
    {
        public Player Winner { get; }

        public EndReason Reason { get; }
        public Result(Player winner, EndReason reason)
        {
            Winner = winner;
            Reason = reason;
        }
        public static Result Win(Player winner)
        {
            return new Result(winner, EndReason.Checkmate);
        }

        public static Result Draw(EndReason reason)
        {
            return new Result(Player.None, reason);
        }
    }

    public class GameState
    {
        public Board Board { get; set; }
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;
        public HashSet<Move> invalidCastlingPositions { get; private set; } = new HashSet<Move>();
        public GameState(Player player, Board board)
        {
            CurrentPlayer = player;
            Board = board;
        }
        public IEnumerable<Move> LegalMoves(Position pos)
        {
            invalidCastlingPositions.Clear();
            if (Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer)
            {
                return Enumerable.Empty<Move>();
            }
            Figure figure = Board[pos];
            IEnumerable<Move> moveCandidates = figure.GetMoves(pos, Board);
            if (figure.Type == FigureType.King)
            {
                foreach (Move move in moveCandidates)
                {
                    if ((move.Type == MoveType.CastleKS || move.Type == MoveType.CastleQS) && !move.IsLegalMove(Board))
                    {
                        invalidCastlingPositions.Add(move);
                    }
                }
            }
            return moveCandidates.Where(move => move.IsLegalMove(Board));
        }
        public void MakeMove(Move move)
        {
            Board.SetPawnSkipPosition(CurrentPlayer, null);
            move.Complete(Board);
            CurrentPlayer = CurrentPlayer.Opponent();
            CheckForGameOver();
        }
        public IEnumerable<Move> AllLegalMoves(Player player)
        {
            IEnumerable<Move> moveCandidates = Board.FigurePositionsForColor(player).SelectMany(pos =>
            {
                Figure figure = Board[pos];
                return figure.GetMoves(pos, Board);
            });

            return moveCandidates.Where(move => move.IsLegalMove(Board));
        }

        private void CheckForGameOver()
        {
            if (!AllLegalMoves(CurrentPlayer).Any())
            {
                if (Board.IsInCheck(CurrentPlayer))
                {
                    Result = Result.Win(CurrentPlayer.Opponent());
                    if (CurrentPlayer.Opponent() == Player.White)
                    {
                        MessageBox.Show("Победили белые фигуры!");
                    }
                    else if (CurrentPlayer.Opponent() == Player.Black)
                    {
                        MessageBox.Show("Победили черные фигуры!");
                    }
                }
                else
                {
                    Result = Result.Draw(EndReason.StaleMate);
                    MessageBox.Show("Ничья!");
                }
            }
        }

        public bool IsGameOver()
        {
            return Result != null;
        }
    }
}
