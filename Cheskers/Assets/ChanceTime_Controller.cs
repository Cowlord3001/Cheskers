using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChanceTime_Controller : MonoBehaviour
{

    /// <summary>
    /// Sudden Death: All pieces are cleared off the board, except the VIPs.
    /// </summary>
    public void ChanceTime1_SuddentDeath()
    {
        for(int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                if (Board_Data.instance.boardPieces[x, y] != null && Board_Data.instance.boardPieces[x, y].IsVIP == false) {
                    Board_Data.instance.RemovePiece(x, y);
                }
            }
        }
    }
    /// <summary>
    /// 2. Teleportation Accident: A player’s VIP randomly swaps places with another piece at the start of their turn.
    /// This is not limited to the current player’s pieces.
    /// </summary>
    public void ChanceTime2_TeleportationAccident()
    {

    }
    /// <summary>
    /// Conformity: The player whose VIP was damaged rolls a d6. This decides the behavior of all pieces for the remainder of the game.
    /// </summary>
    public void ChanceTime3_Comformity()
    {

    }
    /// <summary>
    /// Foreign Exchange: The VIPs immediately switch places.
    /// </summary>
    public void ChanceTime4_ForeignExchange()
    {

    }
    /// <summary>
    /// Thorns II: Whenever a piece is attacked, the attacker is damaged.
    /// </summary>
    public void ChanceTime5_ThornsII()
    {

    }
    /// <summary>
    /// Duel: Coin flips are now changed to games of Rock, Paper, Scissors between players.
    /// </summary>
    public void ChanceTime6_Duel()
    {

    }
    /// <summary>
    /// Chesker Revolution: All pieces on the board are evenly-distributed between players and placed back on the board, starting from the back-left row.
    /// All pieces in players’ hands are evenly-distributed as well.If a piece cannot be evenly-distributed, it is destroyed.
    /// </summary>
    public void ChanceTime7_CheskerRevolution()
    {

    }
    /// <summary>
    /// Structural Failure: All damaged pieces (besides the VIP) are destroyed, and all undamaged pieces are considered damaged.
    /// </summary>
    public void ChanceTime8_StructuralFailure()
    {

    }
    /// <summary>
    /// Necromancy: All pieces in the players’ hands are converted to their color and placed back on the board, starting from the back-left row.
    /// </summary>
    public void ChanceTime9_Necromancy()
    {

    }
    /// <summary>
    /// Defectors: At the start of each turn, one of the player’s pieces randomly changes to the opposing player’s color (this cannot affect a VIP).
    /// </summary>
    public void ChanceTime10_Defectors()
    {

    }
}
