using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class GameState
    {
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; }
        public Result Result {  get; private set; }

        public GameState(Player player,Board board)
        {
             CurrentPlayer = player;
            Board = board;  
        }
        public IEnumerable<Move> LEgalMovesForPiece(Position pos)
        {
            if(Board.IsEmpty(pos)|| Board[pos].Color!= CurrentPlayer)
            {
                return Enumerable.Empty<Move>();
            }

            Piece piece = Board[pos];
            IEnumerable<Move> moveCandidates= piece.GetMoves(pos, Board);
            return moveCandidates.Where(move=> move.Islegal(Board));
        }
        public void MakeMove(Move move)
        {
            move.Execute(Board);
            CurrentPlayer = CurrentPlayer.Opponent();
            CheckForGameOver();

        }
        public IEnumerable<Move> AllLegalMovesFor(Player player)
        {
            IEnumerable<Move> moveCandidates = Board.PiecePositionsFor(player).SelectMany(pos =>
            {
                Piece piece = Board[pos];
                return piece.GetMoves(pos, Board);
            });
            return moveCandidates.Where(move=> move.Islegal(Board));
        }
        private void CheckForGameOver()
        {
            if (!AllLegalMovesFor(CurrentPlayer).Any())
            {
                if (Board.IsInCheck(CurrentPlayer))
                {
                    Result= Result.Win(CurrentPlayer.Opponent());
                }
                else
                {
                    Result = Result.Draw(EndReason.Stalemate);
                }
            }
        }
        public bool IsGameOver()
        {
            return Result != null;
        }

        public Position GetKingPositionInCheck()
        {
            if (Board.IsInCheck(CurrentPlayer))
            {
                // Find the king's position for the current player
                return Board.PiecePositionsFor(CurrentPlayer)
                            .FirstOrDefault(pos => Board[pos] is King);
            }
            return null;
        }

    }
}
