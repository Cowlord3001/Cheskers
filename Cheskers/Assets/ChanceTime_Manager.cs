using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

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

    /// <summary>
    /// At the start of each turn, the player must choose one of their pieces to take damage.
    /// The opposing player can expend a piece from their hand to make them choose a different piece(this can only happen once per turn).
    /// </summary>
    public void ChanceTime11_SpntaneousCombustion()
    {
        //First part not too bad, needs a graphic and an override to end of turn state. 
        //Second part needs to prompt the other player to accept or reject the choice.(new ui)
    }

    /// <summary>
    /// Roll a d6 for each of your pieces. This decides their behavior for the remainder of the game.
    /// </summary>
    public void ChanceTime12_ChessRules()
    {
        //Easy if there is no checkmate. Need to override transform piece part of piece confirmation state
    }

    /// <summary>
    /// Flip a coin: heads = black, tails = white. Move each of your pieces to the nearest square of this color (player’s choice).
    /// </summary>
    public void ChanceTime13_CheckersRules()
    {
        //Need to implment checkers rules into the game? look up checkers
    }

    /// <summary>
    /// Roll a d6 for each of your pieces. This decides their behavior for the remainder of the game.
    /// Flip a coin: heads = black, tails = white.If a piece is currently on a square of this color, you can instead move it like a checkers piece.
    /// </summary>
    public void ChaceTime14_EverythingRules()
    {
        //need to implement checkers rules and override some board functions
    }

    /// <summary>
    ///  Some spaces are now pitfalls. Roll 1d4 and consult the table below to decide their arrangement (red = pitfall).
    /// Only a knight can cross a pitfall, and any pieces present on a pitfall when it appears is damaged and moved to the nearest available space(player’s choice).
    /// </summary>
    public void ChanceTime15_Pitfall()
    {
        //Need to override move in some way to check the path a piece travels.
    }

    /// <summary>
    /// The VIPs are enlarged to be 2x2 pieces, “pushing” any pieces in their space (player’s choice on which directions they expand).
    /// Each VIP has 4 health(or 2 if previously damaged), can attack up to 4 pieces in one move, and turn back to 1x1 pieces if they collide with the other VIP.
    /// </summary>
    public void ChanceTime16_SupresizeMe()
    {
        //Not sure how this should be done yet
    }

    /// <summary>
    ///  All the opponent’s pieces must be eliminated to win. The VIP is no longer the target.
    /// </summary>
    public void ChanceTime17_Completionist()
    {
        //Need to implement chance time detection in the code. This function that detects end of game states will be override by completionist
    }

    /// <summary>
    ///  The VIPs now wield guns, allowing them to attack any piece within line of sight 
    ///  (I.e. the first piece in each of the 4 cardinal directions). They do not travel to this piece.
    /// </summary>
    public void ChanceTime18_Gun()
    {
        //Not too bad need to override valid moves to add more moves for the vip
    }
    /// <summary>
    ///  Recursion: The game restarts, with the board reduced to 6x6 and the number of pieces to 12 each.
    /// If this occurs again, the board is reduced to 4x4 and the number of pieces to 8 each.If this occurs a third time, 
    /// God is dead and Cheskers killed him.
    /// </summary>
    public void ChanceTime19_Recursion()
    {
        //Might be ok, dictionary makes this easier in some ways. I think I used 8 in some parts of the code recently.
        //everything else runes off of the variable size in board data so it should be easy. I think I used 8 when looping through
        //pieces in some chance time functions
    }

    /// <summary>
    /// 20. Cataclysm: At the start of each round, a different Chance Time event is rolled. 
    /// This is overridden (if possible) at the start of the next round.
    /// </summary>
    public void ChanceTime20_Cataclysm()
    {
        //Need a reset rules function and then this will be easy to random between the other states. 
        //Make reset rules once we are clear all that needs to be overriden.
    }
}
