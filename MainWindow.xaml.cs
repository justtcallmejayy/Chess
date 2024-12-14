using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChessLogic;


namespace ChessUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Image[,] pieceImage = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();
        private GameState gameState;
        private Position selectedPos = null;
        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();
            gameState = new GameState(Player.white, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }
        private void InitializeBoard()
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    {
                        Image image = new Image();
                        pieceImage[r, c] = image;
                        PieceGrid.Children.Add(image);

                        Rectangle highlight = new Rectangle();
                        highlights[r, c] = highlight;
                        HighlightGrid.Children.Add(highlight);

                    }
                }
        }
        private void DrawBoard(Board board)
        {
            // Get the position of the king in check, if any
            Position kingInCheck = gameState.GetKingPositionInCheck();
            Color checkHighlightColor = Color.FromArgb(150, 255, 0, 0); // Red color for check

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Piece piece = board[i, j];
                    pieceImage[i, j].Source = Images.GetImage(piece);

                    // Highlight the king's square if it is in check
                    if (kingInCheck != null && kingInCheck.Row == i && kingInCheck.Column == j)
                    {
                        highlights[i, j].Fill = new SolidColorBrush(checkHighlightColor);
                    }
                    else
                    {
                        highlights[i, j].Fill = Brushes.Transparent; // Clear other highlights
                    }
                }
            }
        }


        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen())
            {
                return; 
            }
            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSqarePosition(point);
            if (selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }

        }
        private Position ToSqarePosition(Point point)
        {
            double sqareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / sqareSize);
            int col = (int)(point.X / sqareSize);
            return new Position(row, col);
        }
        private void OnFromPositionSelected(Position pos)
        {
            IEnumerable<Move> moves = gameState.LEgalMovesForPiece(pos);

            if (moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighlight();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHighlight();
            if (moveCache.TryGetValue(pos, out Move move))
            {
                HandleMove(move);
            }
        }
        private void HandleMove(Move move)
        {
            gameState.MakeMove(move);
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            if (gameState.IsGameOver()) { 
                ShowGameOver();
            }
        }
        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();
            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }
        private void ShowHighlight()
        {
            Color color = Color.FromArgb(150, 125, 155, 125);
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
            }
            
        }
        private void HideHighlight()
        {

            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }
        private void SetCursor(Player player)
        {
            if (player == Player.white)
            {
                Cursor=ChessCursors.WhiteCursor;
            }
            else
            {
                Cursor=ChessCursors.BlackCursor;
            }
        }
        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content!=null;
        }
        private void ShowGameOver()
        {
            GameOverMenu gameOverMenu= new GameOverMenu(gameState);
            MenuContainer.Content=gameOverMenu;

            gameOverMenu.OptionSelected += option =>
            {
                if (option == Option.Restart)
                {
                    MenuContainer.Content = null;
                    RestartGame();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            };
        }
        private void RestartGame()
        {
            HideHighlight();
            moveCache.Clear();
            gameState = new GameState(Player.white, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }

    }
} 