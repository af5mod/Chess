using System.Windows;
using System.Windows.Input;
using ChessLogic;

namespace Chess
{
    /// <summary>
    /// Логика взаимодействия для PromotionWindow.xaml
    /// </summary>
    public partial class PromotionWindow : Window
    {
        public FigureType FigureSelected { get; set; } = FigureType.Pawn;
        public PromotionWindow()
        {
            InitializeComponent();

        }

        private void MouseDownChooseQueen(object sender, MouseButtonEventArgs e)
        {
            FigureSelected = FigureType.Queen;
            Close();
        }

        private void MouseDownChooseKnight(object sender, MouseButtonEventArgs e)
        {
            FigureSelected = FigureType.Knight;
            Close();
        }

        private void MouseDownChooseBishop(object sender, MouseButtonEventArgs e)
        {
            FigureSelected = FigureType.Bishop;
            Close();
        }

        private void MouseDownChooseRook(object sender, MouseButtonEventArgs e)
        {
            FigureSelected = FigureType.Rook;
            Close();
        }
    }
}
