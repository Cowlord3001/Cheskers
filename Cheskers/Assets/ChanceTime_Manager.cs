using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChanceTime_Manager : MonoBehaviour
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
                if (Board.instance.boardPieces[position] != null && Board.instance.boardPieces[position].IsVIP == false) {
                    Board.instance.boardPieces[position] = null;
                }
            }
        }

        //Input_Controller.instance.ClearTokens();
        PieceDisplay_Manager.instance.UpdatePieces();
    }
    /// <summary>
    /// 2. Teleportation Accident: A player’s VIP randomly swaps places with another piece at the start of their turn.
    /// This is not limited to the current player’s pieces.
    /// </summary>
    public void ChanceTime2_TeleportationAccident()
    {
        //Solution overwrite end of turn state to swap pieces.
        //TODO: Event that calls this for other person... Maybe this controller can have clientRPC's and server runs chancetime.
        PlayerTurnState newState = new State_EndOfTurn_TeleportingAccident(Turn_Manager.instance);

        Turn_Manager.instance.SwapState(Turn_Manager.PhaseInTurn.END_OF_TURN ,newState);
    }
    /// <summary>
    /// Conformity: The player whose VIP was damaged rolls a d6. This decides the behavior of all pieces for the remainder of the game.
    /// </summary>
    public void ChanceTime3_Comformity()
    {
        Piece.Type[] types = {Piece.Type.pawn, Piece.Type.knight, Piece.Type.queen,
                                   Piece.Type.king, Piece.Type.rook, Piece.Type.bishop};

        int typeIndex = Random.Range(0, types.Length);

        PlayerTurnState newState = new State_PieceConfirmation_Comformity (Turn_Manager.instance, types[typeIndex]);

        Turn_Manager.instance.SwapState(Turn_Manager.PhaseInTurn.PIECE_CONFIRMATION , newState);

        //TODO: Override Pieces to all look like the right type.
    }
    /// <summary>
    /// Foreign Exchange: The VIPs immediately switch places.
    /// </summary>
    public void ChanceTime4_ForeignExchange()
    {
        List<Piece> VIPlist = new List<Piece>();

        foreach (var piece in Board.instance.GetPieces()) {
            if(piece.IsVIP)
                VIPlist.Add (piece);
        }

        Board.instance.SwapPiecesExternal(VIPlist[0], VIPlist[1]);
        PieceDisplay_Manager.instance.UpdatePieces();
    }
    /// <summary>
    /// Thorns II: Whenever a piece is attacked, the attacker is damaged.
    /// </summary>
    public void ChanceTime5_ThornsII()
    {
        //Override Functions in Board controller
        Board.instance.boardFunction_moveAndDamage = new BoardFunction_MoveAndDamage_ThornsII();
        Board.instance.boardFunction_moveAndTake = new BoardFunction_MoveAndTake_ThornsII();
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
        int totalTokens = Token_Manager.instance.GetBlackTokenNumber() + Token_Manager.instance.GetWhiteTokenNumber();
        int totalBoardPieces = Board.instance.GetPieces().Count;

        Token_Manager.instance.ClearTokens();

        for (int i = 0; i < totalTokens/2; i++) {
            Token_Manager.instance.GiveToken(Piece.Color.white);
            Token_Manager.instance.GiveToken(Piece.Color.black);
        }

        //Hands are redistributed seperately and evenly
        foreach (var pos in Board.instance.boardPositions) {
            Board.instance.boardPieces[pos] = null;
        }

        int blackPiecesToPlace = totalBoardPieces / 2;
        int whitePiecesToPlace = totalBoardPieces / 2;

        //Instead of creating new pieces we need to detect who has more and give the other play some of their pieces. That are not he vip.
        for (int i = 0; i < whitePiecesToPlace; i++) {
            int x = i % 8;
            int y = i / 8;
            Vector2Int position = new Vector2Int(x, y);
            Board.instance.boardPieces[position] = new Piece(Piece.Color.white, position);
        }
        for (int i = 0; i < blackPiecesToPlace; i++) {
            int x = i % 8;
            int y = 7 - (i / 8);
            Vector2Int position = new Vector2Int(x, y);
            Board.instance.boardPieces[position] = new Piece(Piece.Color.black, position);
        }
        PieceDisplay_Manager.instance.UpdatePieces();
    }
    /// <summary>
    /// Structural Failure: All damaged pieces (besides the VIP) are destroyed, and all undamaged pieces are considered damaged.
    /// </summary>
    Piece.Color colorToAddTokensFor;
    public void ChanceTime8_StructuralFailure()
    {
        Board.instance.OnPieceRemovedFromBoard += AddToken;
        if(Multiplayer_Manager.instance.isMultiplayerGame) {
            if(Multiplayer_Manager.instance.turnColor.Value == Piece.Color.white) {
                colorToAddTokensFor = Piece.Color.black;
            }
            else {
                colorToAddTokensFor = Piece.Color.white;
            }
        }
        else {
            if(Turn_Manager.instance.GetColor() == Piece.Color.white) {
                colorToAddTokensFor = Piece.Color.black;
            }
            else {
                colorToAddTokensFor = Piece.Color.white;
            }
        }

        //New Function no real change in rules
        foreach (Piece piece in Board.instance.GetPieces()) {
            if (piece.IsVIP == false) {
                Board.instance.DamagePiece(piece.positionOnBoard);
            }
        }
        PieceDisplay_Manager.instance.UpdatePieces();

        Board.instance.OnPieceRemovedFromBoard -= AddToken;
    }

    void AddToken(object sender, Board.EventArgsPieceRemoved e)
    {
        if (e.removedPiece.GetColor() == colorToAddTokensFor) {
            Token_Manager.instance.GiveToken(colorToAddTokensFor);
        }
    }

    /// <summary>
    /// Necromancy: All pieces in the players’ hands are converted to their color and placed back on the board, starting from the back-left row.
    /// </summary>
    public void ChanceTime9_Necromancy()
    {
        //New Function no real change in rules
        int blackTokenCount = Token_Manager.instance.GetBlackTokenNumber();
        int whiteTokenCount = Token_Manager.instance.GetWhiteTokenNumber();

        Token_Manager.instance.ClearTokens();

        int i = 0;
        while(blackTokenCount > 0) {
            int x = i % 8;
            int y = 7 - (i / 8);
            Vector2Int position = new Vector2Int(x, y);
            if (Board.instance.boardPieces[position] == null) {
                Board.instance.boardPieces[position] = new Piece(Piece.Color.black, position);
                blackTokenCount--;
            }
            i++;
        }
        
        i = 0;
        while (whiteTokenCount >0) {
            int x = i % 8;
            int y = i / 8;
            Vector2Int position = new Vector2Int(x, y);
            if (Board.instance.boardPieces[position] == null) {
                Board.instance.boardPieces[position] = new Piece(Piece.Color.white, position);
                whiteTokenCount--;
            }
            i++;
        }
        
        PieceDisplay_Manager.instance.UpdatePieces();
    }
    /// <summary>
    /// Defectors: At the start of each turn, one of the player’s pieces randomly changes to the opposing player’s color (this cannot affect a VIP).
    /// </summary>
    public void ChanceTime10_Defectors()
    {
        //Another Override to End of Turn state
    }
}
