using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Board
{
    //Singleton
    public static Board instance;

    public string debugMessage;
    /// <summary>
    /// This is a dictionary linking vector2Int positions to pieces. values null if not accepted
    /// </summary>
    public Dictionary<Vector2Int, Piece> boardPieces { get; private set; }
    public int size { get; private set; }

    //Event for when pieces are removed from board in case we want some effect to play in another script
    public event EventHandler<EventArgsPieceRemoved> OnPieceRemovedFromBoard;
    public class EventArgsPieceRemoved : EventArgs {
        public Piece removedPiece;
    }

    //Event for when a piece is damaged in case we want some effect to play in another script.
    public event EventHandler<EventArgsPieceDamaged> OnPieceDamaged;
    public class EventArgsPieceDamaged : EventArgs
    {
        public Piece damagedPiece;
    }
    //Add Event for when piece is moved
    public event EventHandler<EventArgsPieceMoved> OnPieceMoved;
    public class EventArgsPieceMoved : EventArgs
    {
        public Vector2Int pieceStartBoardCordintaes;
        public Vector2Int pieceEndBoardCordintaes;
    }

    ///public List<Piece_Data> pieceList { get; private set; }
    public List<Piece> GetPieces()
    {
        List<Piece> pieceList= new List<Piece>();
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                if (boardPieces[new Vector2Int(x,y)] != null) {
                    pieceList.Add(boardPieces[new Vector2Int(x, y)]);
                }
            }
        }

        return pieceList;
    }
    //public void ResetPieceList()
    //{
    //    pieceList = new List<Piece_Data>();
    //}

    public int MOVING_TO_EMPTY = 0;
    public int MOVING_TO_ENEMYPIECE = 1;
    public int MOVING_TO_ILLIGAL_SPACE = 2;
    public List<Vector2Int> boardPositions;

    /// <summary>
    /// This Function will Move a piece and damage it's target. Can be overriden during chance time.
    /// </summary>
    public BoardFunction_MoveAndDamage boardFunction_moveAndDamage;
    public BoardFunction_MoveAndTake boardFunction_moveAndTake;

    public Board()
    {
        debugMessage = "";
        size = 8;
        //boardPieces = new Piece_Data[size, size];
        boardPositions = new List<Vector2Int>();
        boardPieces = new Dictionary<Vector2Int, Piece> ();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2Int position = new Vector2Int (x, y);
                boardPositions.Add (position);

                if (y <= 1) {
                    //boardPieces[x, y] = new Piece_Data(Piece_Data.Color.white, new Vector2Int(x, y));
                    boardPieces.Add(position, new Piece(Piece.Color.white, position));
                }
                else if (y >= size - 2) {
                    boardPieces.Add(position, new Piece(Piece.Color.black, position));
                    //boardPieces[x, y] = new Piece_Data(Piece_Data.Color.black, new Vector2Int(x, y));
                    //pieceList.Add(boardPieces[x, y]);
                }
                else { //boardPieces[x,y] = null;
                    boardPieces.Add(position, null);
                }
            }
        }

        InitializeBoardFunctions();
        SetRandomVIPs();
    }

    void InitializeBoardFunctions()
    {
        boardFunction_moveAndDamage = new BoardFunction_MoveAndDamage();
        boardFunction_moveAndTake = new BoardFunction_MoveAndTake();
    }

    const string NEXTPIECE = "_";
    const string NEXTLINE = "/";
    const string BLANK = "BLANK";
    public string GetBoardState()
    {
        //Encoded based on states. maybe name each piece for smaller encoding
        //UPDATE TO LOOP THROUGH BOARD Positions
        string boardState = "";
        for (int y = 0; y < size; y++) {
            boardState += NEXTLINE;
            for (int x = 0; x < size; x++) {
                boardState += NEXTPIECE;
                if (boardPieces[new Vector2Int(x,y)] == null)
                    boardState += BLANK;
                else
                    boardState += boardPieces[new Vector2Int(x, y)].GetPieceString();
            }
        }
        return boardState;
    }

    public void SetBoardFromStringState(string stringState)
    {
        String[] Lines = stringState.Split(NEXTLINE);
        int y = 0;
        foreach (string line in Lines) {
            int x = 0;
            string[] stringPieces = line.Split(NEXTPIECE);
            foreach (string piece in stringPieces) {
                if (piece != BLANK) {
                    boardPieces[new Vector2Int(x,y)] = new Piece(piece, new Vector2Int(x, y));
                }
                else {
                    boardPieces[new Vector2Int(x, y)] = null;
                }
                x++;
            }
            y++;
        }
    }

    //TODO: Sync for Multiplayer
    void SetRandomVIPs()
    {
        int whiteVIP = UnityEngine.Random.Range(0, 16);
        int blackVIP = UnityEngine.Random.Range(0, 16);

        int x = whiteVIP % 8;//row
        int y = whiteVIP / 8;//col
        boardPieces[new Vector2Int(x,y)].IsVIP = true;

        x = blackVIP % 8;
        y = (size - 1) - blackVIP / 8;
        boardPieces[new Vector2Int(x, y)].IsVIP = true;
    }

    public List<Vector2Int> getAllLegalMoves(Piece piece, Chess_Move_SO chessMoves)
    {
        piece.type = chessMoves.pieceType;
        List<Vector2Int> legalMoves = new List<Vector2Int>();

        if (chessMoves.pieceType == Piece.Type.pawn) {
            List<Vector2Int> extraPawnMoves = GetExtraPawnMoves(piece);

            if(extraPawnMoves.Count > 0) {
                foreach (Vector2Int move in extraPawnMoves) {
                    legalMoves.Add(move);
                }
            }
        }

        //Handle Preset Moves
        foreach (Move move in chessMoves.moves) {
            //int newYPos = piece.positionOnBoard.y + move.changeInY;
            //int newXPos = piece.positionOnBoard.x + move.changeInX;
            Vector2Int changeInPosition = new Vector2Int(move.changeInX, move.changeInY);
            Vector2Int newPosition = piece.positionOnBoard + changeInPosition;

            int moveValidateResult = ValidMove(piece.GetColor(), newPosition);


            //Pawn Logic! They can only move in one direction so skip move if its
            //The wrong direction. Can't move backwards
            if (chessMoves.pieceType == Piece.Type.pawn) {
                if (piece.GetColor() == Piece.Color.white && move.changeInY < 0) {
                    continue;
                }
                else if (piece.GetColor() == Piece.Color.black && move.changeInY > 0) {
                    continue;
                }
                //Only Pawns need this extra check but some checkers rules need this too???
                if (move.requiresTargetPiece == false) {
                    if (moveValidateResult == MOVING_TO_EMPTY) {
                        legalMoves.Add(newPosition);
                    }
                }
                else {
                    if (moveValidateResult == MOVING_TO_ENEMYPIECE) {
                        legalMoves.Add(newPosition);
                    }
                }
            }
            else {
                //Not a pawn
                if (moveValidateResult != MOVING_TO_ILLIGAL_SPACE) {
                    legalMoves.Add(newPosition);
                }
            }

        }

        //Sliding Move
        foreach(SlidingMove move in chessMoves.slidingMoves) {
            int moveValidateResult;
            int startX = piece.positionOnBoard.x;
            int endX = piece.positionOnBoard.x + move.changeInX;
            int startY = piece.positionOnBoard.y;
            int endY = piece.positionOnBoard.x + move.changeInY;
            while (startX != endX || startY != endY) {
                //Normalize move to -1 or 1
                //Could probably just use -1 and 1 in the slidingmove rules
                //However what if some effect causes half movement???
                if(Mathf.Abs(move.changeInX) > 0)
                    startX += move.changeInX/Mathf.Abs(move.changeInX); //adding 1/-1
                if(Mathf.Abs(move.changeInY) > 0)
                    startY += move.changeInY/Mathf.Abs(move.changeInY); //adding 1/-1

                Vector2Int position = new Vector2Int(startX, startY);
                //Validate position
                moveValidateResult = ValidMove(piece.GetColor(), position);

                //If move is not illegal
                if (moveValidateResult == MOVING_TO_EMPTY) {
                    legalMoves.Add(position);
                }
                else if(moveValidateResult == MOVING_TO_ENEMYPIECE) {
                    //Movement Stops if you reach an enemy but move is still added
                    legalMoves.Add(position);
                    break;
                }
                else if(moveValidateResult == MOVING_TO_ILLIGAL_SPACE){
                    //One illegal move stops the rest of the slide.
                    break;
                }
            }

         }
        return legalMoves;
    }

    List<Vector2Int> GetExtraPawnMoves(Piece piece)
    {
        List<Vector2Int> ExtraMoves = new List<Vector2Int>();
        if(piece.hasMoved == false) {
            int newXpos = piece.positionOnBoard.x;
            int newYpos = piece.positionOnBoard.y;

            Vector2Int position = piece.positionOnBoard;

            int moveValidateResult1;
            int moveValidateResult2;
            if (piece.GetColor() == Piece.Color.white) {
                position.y++;
                moveValidateResult1 = ValidMove(piece.GetColor(), position);
                position.y++;
                moveValidateResult2 = ValidMove(piece.GetColor(), position);
            }
            else {
                position.y--;
                moveValidateResult1 = ValidMove(piece.GetColor(), position);
                position.y--;
                moveValidateResult2 = ValidMove(piece.GetColor(), position);
            }

            if (moveValidateResult1 == MOVING_TO_EMPTY && moveValidateResult2 == MOVING_TO_EMPTY) {
                ExtraMoves.Add(position);
            }
        }

        //TODO: Detect en pessant
        return ExtraMoves;
    }
    public void MoveAndTake(Piece piece, Vector2Int newPosition)
    {
        boardFunction_moveAndTake.MoveAndTake(piece, newPosition);
    }

    public void MoveAndDamage(Piece piece, Vector2Int newPosition)
    {
        boardFunction_moveAndDamage.MoveAndDamage(piece, newPosition);
    }

    public void DamagePiece(Vector2Int positionToDamage)
    {
        boardPieces[positionToDamage].health--;
        if(boardPieces[positionToDamage].health == 0) {
            RemovePiece(positionToDamage);
        }

        EventArgsPieceDamaged e = new EventArgsPieceDamaged();
        e.damagedPiece = boardPieces[positionToDamage];
        OnPieceDamaged(this, e);
    }

    public void MovePiece(Piece piece, Vector2Int newPosition)
    {
        EventArgsPieceMoved e = new EventArgsPieceMoved();
        e.pieceStartBoardCordintaes = piece.positionOnBoard;
        e.pieceEndBoardCordintaes = newPosition;
        OnPieceMoved?.Invoke(this, e);

    
        boardPieces[piece.positionOnBoard] = null;
        boardPieces[newPosition] = piece;
        piece.positionOnBoard = newPosition;
    }

    public void ResetPieceType(Piece piece)
    {
        piece.type = Piece.Type.none;
    }

    public void MovePieceNetworkCall(Vector2Int oldPosition, Vector2Int newPosition)
    {
        if(boardPieces[oldPosition] == null) { return; }

        MovePiece(boardPieces[oldPosition], newPosition);
    }

    public void SwapPiecesExternal(Piece piece1, Piece piece2)
    {
        boardPieces[piece1.positionOnBoard] = piece2;
        boardPieces[piece2.positionOnBoard] = piece1;

        Vector2Int piece2Position = piece2.positionOnBoard;
        piece2.positionOnBoard = piece1.positionOnBoard;
        piece1.positionOnBoard = piece2Position;
    }
    public void RemovePiece(Vector2Int removePosition)
    {
        //Fire Remove Piece Event
        EventArgsPieceRemoved e = new EventArgsPieceRemoved();
        e.removedPiece = boardPieces[removePosition];
        OnPieceRemovedFromBoard?.Invoke(this, e);

        boardPieces[removePosition] = null;
    }

    public int ValidMove(Piece.Color color, Vector2Int newPosition)
    {
        if(boardPieces.ContainsKey(newPosition) == false) {
            return MOVING_TO_ILLIGAL_SPACE;
        }

        //if (newXPos > size - 1 || newXPos < 0) return MOVING_TO_ILLIGAL_SPACE;
        //if (newYPos > size - 1 || newYPos < 0) return MOVING_TO_ILLIGAL_SPACE;

        if (boardPieces[newPosition] == null) {
            return MOVING_TO_EMPTY;
        }
        else if(boardPieces[newPosition].GetColor() != color) {
            return MOVING_TO_ENEMYPIECE;
        }
        else {
            return MOVING_TO_ILLIGAL_SPACE;
        }
    }

}
