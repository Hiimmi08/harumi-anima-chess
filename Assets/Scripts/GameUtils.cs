using Pieces;
using System;

public static class GameUtils
{
    public static string GetPieceName(this Rank rank)
    {
        return Enum.GetName(typeof(Rank), rank);
    }

    public static bool IsDefeatable(this Piece piece, Piece otherPiece)
    {
        var rank = piece.CurrentRank;
        var other = otherPiece.CurrentRank;

        if (rank == Rank.Rat && other == Rank.Elephant)
        {
            return true;
        }
        
        if (rank == Rank.Elephant && other == Rank.Rat)
        {
            return false;
        }

        return rank > other;
    }
    
    public static bool IsSwimmable(this Rank rank) => rank is Rank.Rat or Rank.Dog;
    public static bool IsCrossable(this Rank rank) => rank is Rank.Lion or Rank.Tiger;
}