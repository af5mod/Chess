using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;
using Figure = ChessLogic.Figure;

namespace Chess
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TextBlock[,] figureImages = new TextBlock[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private GameState gameState;
        private readonly Dictionary<Position, Move> availableMoves = new Dictionary<Position, Move>();
        private readonly Dictionary<Position, Move> checkedWhileCastlingMoves = new Dictionary<Position, Move>();
        private Position selectedPos = null;
        private Position checkedPos = null;
        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
        }

        private void InitializeBoard()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 50
                    };
                    figureImages[i, j] = textBlock;
                    FigureGrid.Children.Add(textBlock);
                    Grid.SetRow(textBlock, i);
                    Grid.SetColumn(textBlock, j);
                    Rectangle highlight = new Rectangle();
                    highlights[i, j] = highlight;
                    HighlightGrid.Children.Add(highlight);
                    Grid.SetRow(highlight, i);
                    Grid.SetColumn(highlight, j);
                }
            }
        }

        public static class TextBlocks
        {
            private static Dictionary<FigureType, String> whiteFigures = InitializeWhiteFigures();

            private static Dictionary<FigureType, string> InitializeWhiteFigures()
            {
                return new Dictionary<FigureType, string>
                {
                    { FigureType.Pawn, "♙" },
                    { FigureType.Knight, "♘" },
                    { FigureType.Bishop, "♗" },
                    { FigureType.Rook, "♖" },
                    { FigureType.Queen, "♕" },
                    { FigureType.King, "♔" }
                };
            }

            private static Dictionary<FigureType, String> blackFigures = InitializeBlackFigures();

            private static Dictionary<FigureType, string> InitializeBlackFigures()
            {
                return new Dictionary<FigureType, string>
                {
                    { FigureType.Pawn, "♟" },
                    { FigureType.Knight, "♞" },
                    { FigureType.Bishop, "♝" },
                    { FigureType.Rook, "♜" },
                    { FigureType.Queen, "♛" },
                    { FigureType.King, "♚" }
                };
            }

            public static String GetImage(Player color, FigureType type)
            {
                switch (color)
                {
                    case Player.White:
                        return whiteFigures[type];
                    case Player.Black:
                        return blackFigures[type];
                    default:
                        return null;
                }
            }

            public static String GetImage(Figure figure)
            {
                if (figure == null)
                {
                    return null;
                }
                return GetImage(figure.Color, figure.Type);
            }
        }

        private void DrawBoard(Board board)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Figure figure = board[i, j];
                    figureImages[i, j].Text = TextBlocks.GetImage(figure);
                }
            }
        }
        private void NewGameClick(object sender, RoutedEventArgs e)
        {
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetName(Player.White);
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowHighlightsMoves()
        {
            Color color = Color.FromArgb(150, 125, 255, 125);

            foreach (Position to in availableMoves.Keys)
            {
                highlights[to.Row, to.Col].Fill = new SolidColorBrush(color);
            }
        }
        private void ShowHighlightCheck()
        {
            if (checkedPos != null)
            {
                highlights[checkedPos.Row, checkedPos.Col].Fill = Brushes.Transparent;
                checkedPos = null;
            }

            if (gameState.Board.IsInCheck(gameState.CurrentPlayer))
            {
                Position kingPos = gameState.Board.FindKingPosition(gameState.CurrentPlayer);
                if (kingPos != null)
                {
                    checkedPos = kingPos;
                    Color color = Color.FromArgb(150, 255, 0, 0);
                    highlights[kingPos.Row, kingPos.Col].Fill = new SolidColorBrush(color);
                }
            }
        }
        private void ShowHighlightInvalidCastle()
        {
            foreach (Position move in checkedWhileCastlingMoves.Keys)
            {
                if (move != null)
                {
                    Color color = Color.FromArgb(150, 255, 255, 0);
                    highlights[move.Row, move.Col].Fill = new SolidColorBrush(color);
                }
            }
        }
        private void HideHighlights()
        {
            foreach (Position to in availableMoves.Keys)
            {
                highlights[to.Row, to.Col].Fill = Brushes.Transparent;
            }
            foreach (Position to in checkedWhileCastlingMoves.Keys)
            {
                highlights[to.Row, to.Col].Fill = Brushes.Transparent;
            }
        }

        private void MouseDownFigureGrid(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(FigureGrid);
            Point localPoint = FigureGrid.TranslatePoint(point, FigureGrid);
            Position pos = ToRectanglePosition(localPoint);
            Figure selectedFigure = gameState.Board[pos];

            if (selectedPos == null)
            {
                FromPositionSelected(pos);
            }
            else
            {
                ToPositionSelected(pos);
            }
        }

        private Position ToRectanglePosition(Point point)
        {
            double rectangleSizeWidth = FigureGrid.ActualWidth / 8;
            int row = (int)(point.Y / rectangleSizeWidth);
            int col = (int)(point.X / rectangleSizeWidth);
            return new Position(row, col);
        }

        private void FromPositionSelected(Position pos)
        {
            IEnumerable<Move> moves = gameState.LegalMoves(pos);
            MovesCheckedWhileCastling(gameState);
            if (moves.Any())
            {
                selectedPos = pos;
                MovesAvaliable(moves);
                ShowHighlightsMoves();
            }
            if (checkedWhileCastlingMoves.Any())
            {
                selectedPos = pos;
                ShowHighlightInvalidCastle();
            }
        }

        private void ToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHighlights();
            if (availableMoves.TryGetValue(pos, out Move move))
            {
                if (move.Type == MoveType.PawnPromotion)
                {
                    MakePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    MakeMove(move);
                }
            }
        }

        private void MakePromotion(Position from, Position to)
        {
            figureImages[to.Row, to.Col].Text = TextBlocks.GetImage(gameState.CurrentPlayer, FigureType.Pawn);
            figureImages[from.Row, from.Col].Text = null;
            PromotionWindow promWindow = new PromotionWindow();
            promWindow.ShowDialog();
            if (promWindow.FigureSelected != FigureType.Pawn)
            {
                figureImages[to.Row, to.Col].Text = TextBlocks.GetImage(gameState.CurrentPlayer, promWindow.FigureSelected);
                Move promMove = new PawnPromotion(from, to, promWindow.FigureSelected);
                MakeMove(promMove);
            }
        }

        private void MakeMove(Move move)
        {
            gameState.MakeMove(move);
            DrawBoard(gameState.Board);
            ShowHighlightCheck();
            SetName(gameState.CurrentPlayer);
        }
        private void MovesAvaliable(IEnumerable<Move> moves)
        {
            availableMoves.Clear();
            foreach (Move move in moves)
            {
                availableMoves[move.ToPos] = move;
            }
        }

        private void MovesCheckedWhileCastling(GameState gameState)
        {
            checkedWhileCastlingMoves.Clear();
            foreach (Move move in gameState.invalidCastlingPositions)
            {
                checkedWhileCastlingMoves[move.ToPos] = move;
            }
        }

        private void SetName(Player player)
        {
            if (player == Player.White)
            {
                CurrentPlayerText.Text = "Ход игрока: Белые";
            }
            else if (player == Player.Black)
            {
                CurrentPlayerText.Text = "Ход игрока: Черные";
            }
        }
    }
}
