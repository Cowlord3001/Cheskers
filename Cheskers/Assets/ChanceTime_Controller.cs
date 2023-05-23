using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChanceTime_Controller : MonoBehaviour
{
    public List<System.Action> chanceTimeFunctionsList;

    private void Start()
    {
        chanceTimeFunctionsList = new List<System.Action>();
        chanceTimeFunctionsList.Add(ChanceTime1_SuddentDeath);
        chanceTimeFunctionsList.Add(ChanceTime2_TeleportationAccident);
        chanceTimeFunctionsList.Add(ChanceTime3_Comformity);
        chanceTimeFunctionsList.Add(ChanceTime4_ForeignExchange);
        chanceTimeFunctionsList.Add(ChanceTime5_ThornsII);
        chanceTimeFunctionsList.Add(ChanceTime7_CheskerRevolution);
        chanceTimeFunctionsList.Add(ChanceTime8_StructuralFailure);
        chanceTimeFunctionsList.Add(ChanceTime9_Necromancy);
    }


    /// <summary>
    /// Sudden Death: All pieces are cleared off the board, except the VIPs.
    /// </summary>
    public void ChanceTime1_SuddentDeath()
    {
        for(int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                Vector2Int position = new Vector2Int(x, y);
                if (Board_Data.instance.boardPieces[position] != null && Board_Data.instance.boardPieces[position].IsVIP == false) {
                    Board_Data.instance.boardPieces[position] = null;
                }
            }
        }

        //Input_Controller.instance.ClearTokens();
        Piece_Display.instance.UpdatePieces();
    }
    /// <summary>
    /// 2. Teleportation Accident: A player’s VIP randomly swaps places with another piece at the start of their turn.
    /// This is not limited to the current player’s pieces.
    /// </summary>
    public void ChanceTime2_TeleportationAccident()
    {
        //Solution overwrite end of turn state to swap pieces.
        //TODO: Event that calls this for other person... Maybe this controller can have clientRPC's and server runs chancetime.
        PlayerTurnState newState = new State_EndOfTurn_TeleportingAccident(Piece_Controller.instance);

        Piece_Controller.instance.SwapState(Piece_Controller.PhaseInTurn.END_OF_TURN ,newState);
    }
    /// <summary>
    /// Conformity: The player whose VIP was damaged rolls a d6. This decides the behavior of all pieces for the remainder of the game.
    /// </summary>
    public void ChanceTime3_Comformity()
    {
        Piece_Data.Type[] types = {Piece_Data.Type.pawn, Piece_Data.Type.knight, Piece_Data.Type.queen,
                                   Piece_Data.Type.king, Piece_Data.Type.rook, Piece_Data.Type.bishop};

        int typeIndex = Random.Range(0, types.Length);

        PlayerTurnState newState = new State_PieceConfirmation_Comformity (Piece_Controller.instance, types[typeIndex]);

        Piece_Controller.instance.SwapState(Piece_Controller.PhaseInTurn.PIECE_CONFIRMATION , newState);

        //TODO: Override Pieces to all look like the right type.
    }
    /// <summary>
    /// Foreign Exchange: The VIPs immediately switch places.
    /// </summary>
    public void ChanceTime4_ForeignExchange()
    {
        List<Piece_Data> VIPlist = new List<Piece_Data>();

        foreach (var piece in Board_Data.instance.GetPieces()) {
            if(piece.IsVIP)
                VIPlist.Add (piece);
        }

        Board_Data.instance.SwapPiecesExternal(VIPlist[0], VIPlist[1]);
        Piece_Display.instance.UpdatePieces();
    }
    /// <summary>
    /// Thorns II: Whenever a piece is attacked, the attacker is damaged.
    /// </summary>
    public void ChanceTime5_ThornsII()
    {
        //Override Functions in Board controller
        Board_Data.instance.boardFunction_moveAndDamage = new BoardFunction_MoveAndDamage_ThornsII();
        Board_Data.instance.boardFunction_moveAndTake = new BoardFunction_MoveAndTake_ThornsII();
        //Don't want the other person to gain a token.
        //A token is gained when removepiece is used.... 
        //have a way to remove pieces that does not add tokens
        //You can't gain tokens when its not your turn? this seems to work
    }
    /// <summary>
    /// Duel: Coin flips are now changed to games of Rock, Paper, Scissors between players.
    /// </summary>
    public void ChanceTime6_Duel()
    {
        //Need to implement coin flip class that can be turned into rockpaper siscors.
        //How does winner decide what happens in game? 
        //Hard chance time need new behaviors
    }
    /// <summary>
    /// Chesker Revolution: All pieces on the board are evenly-distributed between players and placed back on the board, starting from the back-left row.
    /// All pieces in players’ hands are evenly-distributed as well. If a piece cannot be evenly-distributed, it is destroyed.
    /// </summary>
    public void ChanceTime7_CheskerRevolution()
    {
        //TODO: Determine where extra pieces go. Extra pieces deleted for now.
        //New Function no real change in rules
        int totalTokens = Input_Controller.instance.GetBlackTokenNumber() +  Input_Controller.instance.GetWhiteTokenNumber();
        int totalBoardPieces = Board_Data.instance.GetPieces().Count;

        Input_Controller.instance.ClearTokens();
        for (int i = 0; i < totalTokens/2; i++) {
            Input_Controller.instance.GiveToken(Piece_Data.Color.white);
            Input_Controller.instance.GiveToken(Piece_Data.Color.black);
        }


        //Hands are redistributed seperately and evenly
        foreach (var pos in Board_Data.instance.boardPositions) {
            Board_Data.instance.boardPieces[pos] = null;
        }

        int blackPiecesToPlace = totalBoardPieces / 2;
        int whitePiecesToPlace = totalBoardPieces / 2;

        //Instead of creating new pieces we need to detect who has more and give the other play some of their pieces. That are not he vip.
        for (int i = 0; i < whitePiecesToPlace; i++) {
            int x = i % 8;
            int y = i / 8;
            Vector2Int position = new Vector2Int(x, y);
            Board_Data.instance.boardPieces[position] = new Piece_Data(Piece_Data.Color.white, position);
        }
        for (int i = 0; i < blackPiecesToPlace; i++) {
            int x = i % 8;
            int y = 7 - (i / 8);
            Vector2Int position = new Vector2Int(x, y);
            Board_Data.instance.boardPieces[position] = new Piece_Data(Piece_Data.Color.black, position);
        }
        Piece_Display.instance.UpdatePieces();
    }
    /// <summary>
    /// Structural Failure: All damaged pieces (besides the VIP) are destroyed, and all undamaged pieces are considered damaged.
    /// </summary>
    public void ChanceTime8_StructuralFailure()
    {
        //New Function no real change in rules
        foreach (Piece_Data piece in Board_Data.instance.GetPieces()) {
            Board_Data.instance.DamagePiece(piece.positionOnBoard);
        }
        Piece_Display.instance.UpdatePieces();
    }
    /// <summary>
    /// Necromancy: All pieces in the players’ hands are converted to their color and placed back on the board, starting from the back-left row.
    /// </summary>
    public void ChanceTime9_Necromancy()
    {
        //New Function no real change in rules
        int blackTokenCount = Input_Controller.instance.GetBlackTokenNumber();
        int whiteTokenCount = Input_Controller.instance.GetWhiteTokenNumber();

        Input_Controller.instance.ClearTokens();

        int i = 0;
        while(blackTokenCount > 0) {
            int x = i % 8;
            int y = 7 - (i / 8);
            Vector2Int position = new Vector2Int(x, y);
            if (Board_Data.instance.boardPieces[position] == null) {
                Board_Data.instance.boardPieces[position] = new Piece_Data(Piece_Data.Color.white, position);
                blackTokenCount--;
            }
            i++;
        }

        i = 0;
        while (whiteTokenCount >0) {
            int x = i % 8;
            int y = i / 8;
            Vector2Int position = new Vector2Int(x, y);
            if (Board_Data.instance.boardPieces[position] == null) {
                Board_Data.instance.boardPieces[position] = new Piece_Data(Piece_Data.Color.black, position);
                whiteTokenCount--;
            }
            i++;
        }

        Piece_Display.instance.UpdatePieces();
    }
    /// <summary>
    /// Defectors: At the start of each turn, one of the player’s pieces randomly changes to the opposing player’s color (this cannot affect a VIP).
    /// </summary>
    public void ChanceTime10_Defectors()
    {
        //Another Override to End of Turn state
    }
}
