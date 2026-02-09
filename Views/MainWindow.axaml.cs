using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GomokuAI.Models;
using GomokuAI.Services;

namespace GomokuAI.Views
{
    public partial class MainWindow : Window
    {
        private readonly GameService _gameService;

        private const int BoardSize = 15;
        private const double CellSize = 40;
        private const double PieceRadius = 16;
        private const double BoardPadding = 20;

        // Color definitions
        private readonly IBrush _boardBrush = new SolidColorBrush(Color.FromRgb(212, 165, 116));
        private readonly IBrush _lineBrush = new SolidColorBrush(Color.FromRgb(139, 105, 20));
        private readonly IBrush _blackBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        private readonly IBrush _whiteBrush = new SolidColorBrush(Color.FromRgb(250, 250, 250));
        private readonly IBrush _hoverBrush = new SolidColorBrush(Color.FromArgb(80, 233, 69, 96));
        private readonly IBrush _lastMoveBrush = new SolidColorBrush(Color.FromRgb(233, 69, 96));

        private Ellipse? _hoverPiece;

        public MainWindow()
        {
            InitializeComponent();

            _gameService = new GameService();

            // Subscribe to events
            _gameService.OnBoardUpdated += () => Dispatcher.UIThread.Post(DrawBoard);
            _gameService.OnGameMessage += (msg) => Dispatcher.UIThread.Post(() => UpdateStatus(msg));
            _gameService.OnGameStateChanged += (state) => Dispatcher.UIThread.Post(() => OnGameStateChanged(state));

            // Initialize board
            DrawEmptyBoard();
        }

        /// <summary>
        /// Draw empty board
        /// </summary>
        private void DrawEmptyBoard()
        {
            BoardCanvas.Children.Clear();

            // Draw board background
            var background = new Rectangle
            {
                Width = CellSize * (BoardSize - 1) + BoardPadding * 2,
                Height = CellSize * (BoardSize - 1) + BoardPadding * 2,
                Fill = _boardBrush,
                RadiusX = 8,
                RadiusY = 8
            };
            BoardCanvas.Children.Add(background);

            // Draw grid lines
            for (int i = 0; i < BoardSize; i++)
            {
                // Horizontal line
                var hLine = new Line
                {
                    StartPoint = new Point(BoardPadding, BoardPadding + i * CellSize),
                    EndPoint = new Point(BoardPadding + (BoardSize - 1) * CellSize, BoardPadding + i * CellSize),
                    Stroke = _lineBrush,
                    StrokeThickness = i == 0 || i == BoardSize - 1 ? 2 : 1
                };
                BoardCanvas.Children.Add(hLine);

                // Vertical line
                var vLine = new Line
                {
                    StartPoint = new Point(BoardPadding + i * CellSize, BoardPadding),
                    EndPoint = new Point(BoardPadding + i * CellSize, BoardPadding + (BoardSize - 1) * CellSize),
                    Stroke = _lineBrush,
                    StrokeThickness = i == 0 || i == BoardSize - 1 ? 2 : 1
                };
                BoardCanvas.Children.Add(vLine);
            }

            // Draw star points
            int[] starPoints = { 3, 7, 11 };
            foreach (int row in starPoints)
            {
                foreach (int col in starPoints)
                {
                    var star = new Ellipse
                    {
                        Width = 8,
                        Height = 8,
                        Fill = _lineBrush
                    };
                    Canvas.SetLeft(star, BoardPadding + col * CellSize - 4);
                    Canvas.SetTop(star, BoardPadding + row * CellSize - 4);
                    BoardCanvas.Children.Add(star);
                }
            }
        }

        /// <summary>
        /// Draw board and pieces
        /// </summary>
        private void DrawBoard()
        {
            DrawEmptyBoard();

            var lastMove = _gameService.GetLastMove();

            // Draw all pieces
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    var cell = _gameService.GetCell(row, col);
                    if (cell != PlayerType.None)
                    {
                        bool isLastMove = lastMove != null && lastMove.Row == row && lastMove.Col == col;
                        DrawPiece(row, col, cell, isLastMove);
                    }
                }
            }

            // Update undo/redo button states
            UpdateUndoRedoButtons();
        }

        /// <summary>
        /// Draw a piece
        /// </summary>
        private void DrawPiece(int row, int col, PlayerType player, bool isLastMove = false)
        {
            var piece = new Ellipse
            {
                Width = PieceRadius * 2,
                Height = PieceRadius * 2,
                Fill = player == PlayerType.Black ? _blackBrush : _whiteBrush,
                Stroke = player == PlayerType.Black ? Brushes.Black : new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                StrokeThickness = 1
            };

            double x = BoardPadding + col * CellSize - PieceRadius;
            double y = BoardPadding + row * CellSize - PieceRadius;
            Canvas.SetLeft(piece, x);
            Canvas.SetTop(piece, y);
            BoardCanvas.Children.Add(piece);

            // Mark last move
            if (isLastMove)
            {
                var marker = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = player == PlayerType.Black ? _whiteBrush : _blackBrush
                };
                Canvas.SetLeft(marker, BoardPadding + col * CellSize - 4);
                Canvas.SetTop(marker, BoardPadding + row * CellSize - 4);
                BoardCanvas.Children.Add(marker);
            }
        }

        /// <summary>
        /// Board click event
        /// </summary>
        private async void BoardCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_gameService.State != GameState.Playing)
                return;

            if (_gameService.CurrentPlayer != _gameService.HumanPlayer.Type)
                return;

            var pos = e.GetPosition(BoardCanvas);
            var (row, col) = GetBoardPosition(pos);

            if (row >= 0 && row < BoardSize && col >= 0 && col < BoardSize)
            {
                await _gameService.PlayerMove(row, col);
            }
        }

        /// <summary>
        /// Mouse move event - Show hover effect
        /// </summary>
        private void BoardCanvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_gameService.State != GameState.Playing)
            {
                RemoveHoverPiece();
                return;
            }

            if (_gameService.CurrentPlayer != _gameService.HumanPlayer.Type)
            {
                RemoveHoverPiece();
                return;
            }

            var pos = e.GetPosition(BoardCanvas);
            var (row, col) = GetBoardPosition(pos);

            if (row >= 0 && row < BoardSize && col >= 0 && col < BoardSize)
            {
                if (_gameService.GetCell(row, col) == PlayerType.None)
                {
                    ShowHoverPiece(row, col);
                    return;
                }
            }

            RemoveHoverPiece();
        }

        /// <summary>
        /// Mouse leave event
        /// </summary>
        private void BoardCanvas_PointerExited(object? sender, PointerEventArgs e)
        {
            RemoveHoverPiece();
        }

        /// <summary>
        /// Show hover piece
        /// </summary>
        private void ShowHoverPiece(int row, int col)
        {
            RemoveHoverPiece();

            _hoverPiece = new Ellipse
            {
                Width = PieceRadius * 2,
                Height = PieceRadius * 2,
                Fill = _hoverBrush,
                Stroke = _lastMoveBrush,
                StrokeThickness = 2,
                IsHitTestVisible = false
            };

            double x = BoardPadding + col * CellSize - PieceRadius;
            double y = BoardPadding + row * CellSize - PieceRadius;
            Canvas.SetLeft(_hoverPiece, x);
            Canvas.SetTop(_hoverPiece, y);
            BoardCanvas.Children.Add(_hoverPiece);
        }

        /// <summary>
        /// Remove hover piece
        /// </summary>
        private void RemoveHoverPiece()
        {
            if (_hoverPiece != null)
            {
                BoardCanvas.Children.Remove(_hoverPiece);
                _hoverPiece = null;
            }
        }

        /// <summary>
        /// Get board position from mouse position
        /// </summary>
        private (int row, int col) GetBoardPosition(Point pos)
        {
            int col = (int)Math.Round((pos.X - BoardPadding) / CellSize);
            int row = (int)Math.Round((pos.Y - BoardPadding) / CellSize);
            return (row, col);
        }

        /// <summary>
        /// Start game button
        /// </summary>
        private void StartButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _gameService.StartNewGame();
            StartButton.Content = "Restart";
            UpdateTurnIndicators();
        }

        /// <summary>
        /// Undo button - Uses Stack-based state management
        /// </summary>
        private void UndoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _gameService.Undo();
            UpdateTurnIndicators();
            UpdateUndoRedoButtons();
        }

        /// <summary>
        /// Redo button - Replays undone moves
        /// </summary>
        private void RedoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _gameService.Redo();
            UpdateTurnIndicators();
            UpdateUndoRedoButtons();
        }

        /// <summary>
        /// Update undo/redo button states
        /// </summary>
        private void UpdateUndoRedoButtons()
        {
            UndoButton.IsEnabled = _gameService.CanUndo;
            RedoButton.IsEnabled = _gameService.CanRedo;
        }

        /// <summary>
        /// Update status text
        /// </summary>
        private void UpdateStatus(string message)
        {
            StatusText.Text = message;
            UpdateTurnIndicators();
            UpdateUndoRedoButtons();
        }

        /// <summary>
        /// Update turn indicators
        /// </summary>
        private void UpdateTurnIndicators()
        {
            if (_gameService.State == GameState.Playing)
            {
                if (_gameService.CurrentPlayer == PlayerType.Black)
                {
                    PlayerIndicator.Text = "●";
                    PlayerIndicator.Foreground = new SolidColorBrush(Color.FromRgb(74, 222, 128));
                    AIIndicator.Text = "○";
                    AIIndicator.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
                }
                else
                {
                    PlayerIndicator.Text = "○";
                    PlayerIndicator.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
                    AIIndicator.Text = "●";
                    AIIndicator.Foreground = new SolidColorBrush(Color.FromRgb(74, 222, 128));
                }
            }
            else
            {
                PlayerIndicator.Text = "○";
                PlayerIndicator.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
                AIIndicator.Text = "○";
                AIIndicator.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
            }
        }

        /// <summary>
        /// Game state changed callback
        /// </summary>
        private void OnGameStateChanged(GameState state)
        {
            UpdateUndoRedoButtons();

            if (state == GameState.BlackWin || state == GameState.WhiteWin || state == GameState.Draw)
            {
                StartButton.Content = "Play Again";
                UndoButton.IsEnabled = false;
                RedoButton.IsEnabled = false;
            }
        }
    }
}
