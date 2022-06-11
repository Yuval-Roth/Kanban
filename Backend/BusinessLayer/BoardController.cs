﻿using System;
using System.Collections.Generic;
using IntroSE.Kanban.Backend.Exceptions;
using IntroSE.Kanban.Backend.Utilities;

namespace IntroSE.Kanban.Backend.BusinessLayer
{

    /// <summary>
    ///This class controls the actions users' boards.<br/>
    ///<br/>
    ///<code>Supported operations:</code>
    ///<br/>
    /// <list type="bullet">AddBoard()</list>
    /// <list type="bullet">RemoveBoard()</list>
    /// <list type="bullet">GetAllTasksByState()</list>
    /// <list type="bullet">GetBoards()</list>
    /// <list type="bullet">SearchBoard()</list>
    /// <br/><br/>
    /// ===================
    /// <br/>
    /// <c>Ⓒ Kfir Nissim</c>
    /// <br/>
    /// ===================
    /// </summary>

    public class BoardController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Backend\\BusinessLayer\\BoardController.cs");
        BoardDataOperations boardData;
        /// <summary>
        /// Initialize a new BoardController <br/><br/>
        /// </summary>
        /// <param name="userData"></param>
        public BoardController(BoardDataOperations boardData)
        {
            this.boardData = boardData;
        }

        /// <summary>
        /// Add new <c>Board</c> to <c>UserData</c> userData <br/> <br/>
        /// <b>Throws</b> <c>ArgumentException</c> if a<c> Board</c> with that title already exists<br/>
        /// for the <c>User</c><br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the user doesn't exist<br/>
        /// <b>Throws</b> <c>AccessViolationException</c> if the user isn't logged in<br/>
        /// in the system
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NoSuchElementException"></exception>
        /// <exception cref="AccessViolationException"></exception>
        public void AddBoard(CIString email, CIString name)
        {

            log.Debug("AddBoard() for: " + email + "Board's name" + name);


            try
            {
                ValidateUser(email);

                if (name.Length == 0)
                {
                    log.Error("AddBoard() failed: board name is empty");
                    throw new ArgumentException("board name is empty");
                }
                boardData.AddNewBoard(email, name);
                log.Debug("AddBoard() success");
            }
            catch (ElementAlreadyExistsException)
            {
                log.Error("AddBoard() failed: board '" + name + "' already exists for " + email);
                throw;
            }
            catch (DataMisalignedException)
            {
                log.Fatal("AddBoard() failed: BoardIDCounter is out of sync");
                throw;
            }
            catch (UserDoesNotExistException e)
            {
                log.Error("AddBoard() failed: " + e.Message);
                throw;
            }
            catch (UserNotLoggedInException e)
            {
                log.Error("AddBoard() failed: " + e.Message);
                throw;
            }

        }


        /// <summary>
        /// Remove <c>Board</c> from <c>UserData</c> userData <br/> <br/>
        /// <b>Throws</b> <c>ArgumentException</c> if a <c>Board</c> with that title <br/>
        /// doesn't exist for the user<br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the user doesn't exist<br/>
        /// <b>Throws</b> <c>AccessViolationException</c> if the user isn't logged in or user isn't the owner<br/>
        /// in the system
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NoSuchElementException"></exception>
        /// <exception cref="AccessViolationException"></exception>
        public void RemoveBoard(CIString email, CIString name)
        {

            log.Debug("RemoveBoard() for: " + email + "Board's name" + name);
            try
            {
                ValidateUser(email);
                Board board = SearchBoard(email, name);
                if (board.Owner.Equals(email)==false)
                {
                    log.Error("RemoveBoard() failed: user has not permission to do RemoveBoard");
                    throw new AccessViolationException("user has not permission to do RemoveBoard");
                }
                boardData.NukeBoard(email, name);
                log.Debug("RemoveBoard() success");
            }
            catch (NoSuchElementException)
            {
                log.Error("RemoveBoard() failed: '" + email + "' doesn't exist");
                throw;
            }
            catch (OperationCanceledException)
            {
                log.Error("RemoveBoard() failed: board '" + name + "' doesn't exist for " + email);
                throw;
            }
            catch (UserDoesNotExistException e)
            {
                log.Error("RemoveBoard() failed: " + e.Message);
                throw;
            }
            catch (UserNotLoggedInException e)
            {
                log.Error("RemoveBoard() failed: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Returns <c>tasks' list</c> from <c>UserData</c> userData <br/> <br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the user doesn't exist<br/>
        /// <b>Throws</b> <c>AccessViolationException</c> if the user isn't logged in<br/>
        /// <b>Throws</b> <c>IndexOutOfRangeException</c> if the column is not a valid column number
        /// </summary>
        /// <param name="email"></param>
        /// <param name="columnOrdinal"></param>
        /// <returns>A list of tasks by specific state, unless an error occurs</returns>
        /// <exception cref="NoSuchElementException"></exception>
        /// <exception cref="AccessViolationException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public LinkedList<Task> GetAllTasksByState(CIString email, int columnOrdinal)
            {
            log.Debug("GetAllTasksByState() for: " + "Board's name" + columnOrdinal);
            try
            {
                ValidateUser(email);
                ValidateColumnOrdinal(columnOrdinal);

                LinkedList<Task> tasks = new LinkedList<Task>();
                LinkedList<Board> boards = GetBoards(email);
                foreach (Board board in boards)
                {
                    foreach (Task task in board.GetColumn(columnOrdinal))
                    {
                        tasks.AddLast(task);
                    }
                }
                log.Debug("GetAllTasksByState() success");
                return tasks;
            }
            catch (UserDoesNotExistException e)
            {
                log.Error("GetAllTasksByState() failed: " + e.Message);
                throw;
            }
            catch (UserNotLoggedInException e)
            {
                log.Error("GetAllTasksByState() failed: " + e.Message);
                throw;
            }
            catch (IndexOutOfRangeException e)
            {
                log.Error("GetAllTasksByState() failed: " + e.Message);
                throw;
            }
            
        }


        /// <summary>
        /// Returns <c>boards' list</c> from <c>UserData</c> userData <br/> <br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the user doesn't exist<br/>
        /// <b>Throws</b> <c>AccessViolationException</c> if the user isn't logged in
        /// </summary>
        /// <param name="email"></param>
        /// <returns>A list of Boards, unless an error occurs</returns>
        /// <exception cref="NoSuchElementException"></exception>
        /// <exception cref="AccessViolationException"></exception>
        public LinkedList<Board> GetBoards (CIString email) {

            log.Debug("GetBoards() for: " + email);
            try
            {
                ValidateUser(email);
                LinkedList<Board> myBoards = boardData.GetBoardsDataUnit(email).MyBoards;
                LinkedList<Board> joinedBoards = boardData.GetBoardsDataUnit(email).JoinedBoards;
                LinkedList<Board> output = new();
                foreach(Board board in myBoards)
                {
                    output.AddLast(board);
                }
                foreach (Board board in joinedBoards)
                {
                    output.AddLast(board);
                }
                log.Debug("GetBoards() success");
                return output;
            }
            catch (NoSuchElementException)
            {
                log.Error("GetBoards() failed: '" + email + "' doesn't exist");
                throw;
            }
            catch (UserDoesNotExistException e)
            {
                log.Error("GetBoards() failed: " + e.Message);
                throw;
            }
            catch (UserNotLoggedInException e)
            {
                log.Error("GetBoards() failed: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Returns <c>List</c> of boardId of user <br/> <br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the user doesn't exist<br/>
        /// <b>Throws</b> <c>AccessViolationException</c> if the user isn't logged in
        /// </summary>
        /// <param name="email"></param>
        /// <returns>A list of int of board Id, unless an error occurs</returns>
        /// <exception cref="NoSuchElementException"></exception>
        /// <exception cref="AccessViolationException"></exception>
        public LinkedList<int> GetBoardsId(CIString email)
        {

            log.Debug("GetBoards() for: " + email);
            try
            {
                ValidateUser(email);
                LinkedList<Board> myBoards = GetBoards(email);
                LinkedList<int> output = new();
                foreach (Board board in myBoards)
                {
                    output.AddLast(board.Id);
                    log.Debug("GetBoards() success");
                }
                return output;
            }
            catch (NoSuchElementException)
            {
                log.Error("GetBoards() failed: '" + email + "' doesn't exist");
                throw;
            }
            catch (UserDoesNotExistException e)
            {
                log.Error("GetBoards() failed: " + e.Message);
                throw;
            }
            catch (UserNotLoggedInException e)
            {
                log.Error("GetBoards() failed: " + e.Message);
                throw;
            }
        }


        /// <summary>
        /// Returns <c>board</c> from <c>UserData</c> userData <br/> <br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the user or the board doesn't exist<br/>
        /// <b>Throws</b> <c>AccessViolationException</c> if the user isn't logged in
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Board, unless an error occurs</returns>
        /// <exception cref="NoSuchElementException"></exception>
        public Board SearchBoard(CIString email , CIString name)
        {
            log.Debug("SearchBoard() for: " + email + " Board's name " + name);

            try
            {
                ValidateUser(email);

                LinkedList<Board> boardList = boardData.GetBoardsDataUnit(email).MyBoards;
                foreach (Board board in boardList)
                {
                    if (board.Title.Equals(name))
                    {
                        log.Debug("SearchBoard() success");
                        return board;
                    }
                }
                LinkedList<Board> boardList1 = boardData.GetBoardsDataUnit(email).JoinedBoards;
                foreach (Board board in boardList1)
                {
                    if (board.Title.Equals(name))
                    {
                        log.Debug("SearchBoard() success");
                        return board;
                    }
                }
                log.Error("SearchBoard() failed: '" + name + "' doesn't exist");
                throw new NoSuchElementException("A board titled '" +
                                name + "' doesn't exists for the user with the email " + email);
            }
            catch (UserDoesNotExistException e)
            {
                log.Error("SearchBoard() failed: " + e.Message);
                throw;
            }
            catch (UserNotLoggedInException e)
            {
                log.Error("SearchBoard() failed: " + e.Message);
                throw;
            }
            
        }

        /// <summary>
        /// Returns <c>board</c> from <c>UserData</c> userData <br/> <br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the board doesn't exist<br/>
        /// <b>Throws</b> <c>AccessViolationException</c> if the user isn't logged in <br/>
        /// <b>Throws</b> <c>UserDoesNotExistException</c> if the user doesn't exist<br/> 
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Board, unless an error occurs</returns>
        /// <exception cref="NoSuchElementException"></exception>
        /// <exception cref="AccessViolationException"></exception>
        /// <exception cref="UserDoesNotExistException"></exception>
        public Board SearchBoard(CIString email, int boardId)
        {
            log.Debug("SearchBoard() for: Board's Id " + boardId);
            try
            {
                ValidateUser(email);
                Board board = boardData.SearchBoardById(boardId);
                log.Debug("SearchBoard() success");
                return board;
            }
            catch (NoSuchElementException)
            {
                log.Error("SearchBoard() failed: '" + boardId + "' doesn't exist");
                throw new NoSuchElementException("A board with Id '" +
                                boardId + "' doesn't exists");
            }
            catch (UserDoesNotExistException e)
            {
                log.Error("SearchBoard() failed: " + e.Message);
                throw;
            }
            catch (UserNotLoggedInException e)
            {
                log.Error("SearchBoard() failed: " + e.Message);
                throw;
            }
        }
        /// <summary>
        /// add <c>User</c> to <c>Board</c> joined boards <br/><br/>
        /// <b>Throws</b> <c>ElementAlreadyExistsException</c> if the user already joined to the board<br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the board doesn't exist <br/>
        /// <b>Throws</b> <c>UserDoesNotExistException</c> if the user doesn't exist<br/> 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="boardId"></param>
        /// <exception cref="ElementAlreadyExistsException"></exception>
        /// <exception cref="NoSuchElementException"></exception>
        /// <exception cref="UserDoesNotExistException"></exception>

        public void JoinBoard(CIString email, int boardId)
        {
            log.Debug("JoinBoard() for: user " + email + " Board's Id " + boardId);
            try
            {
                ValidateUser(email);
                boardData.AddPointerToJoinedBoard(email, boardId);
                Board board = SearchBoard(email, boardId);
                board.JoinBoard(email, boardId);
                log.Debug("JoinBoard() success");
            }
            catch (ElementAlreadyExistsException)
            {
                log.Error("JoinBoard() failed: the user '" + email + "' is already joined to the board");
                throw new ElementAlreadyExistsException("the user '" + email + "' is already joined to the board");
            }
            catch (NoSuchElementException)
            {
                log.Error($"JoinBoard() failed: A board with id '{boardId}' doesn't exist in the system");
                throw new NoSuchElementException($"A board with id '{boardId}' doesn't exist in the system");
            }
            catch (UserDoesNotExistException e)
            {
                log.Error("JoinBoard() failed: " + e.Message);
                throw;
            }
            catch (UserNotLoggedInException e)
            {
                log.Error("JoinBoard() failed: " + e.Message);
                throw;
            }
            catch (AccessViolationException e)
            {
                log.Error("JoinBoard() failed: " + e.Message);
                throw;
            }
            catch (ArgumentException e)
            {
                log.Error("JoinBoard() failed: " + e.Message);
                throw;
            }

        }
        /// <summary>
        /// remove <c>User</c> from <c>Board</c> joined boards <br/><br/>
        /// <b>Throws</b> <c>UserDoesNotExistException</c> if the user doesn't exist<br/>
        /// <b>Throws</b> <c>ArgumentException</c> if the user isn't joined to a board with that Id<br/> 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="boardId"></param>
        /// <exception cref="UserDoesNotExistException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void LeaveBoard(CIString email, int boardId)
        {
            log.Debug("LeaveBoard() for user: " + email + "for board " + boardId);
            try
            {
                ValidateUser(email);
                Board board = SearchBoard(email, boardId);
                if (board.Owner == email)
                {
                    log.Error($"LeaveBoard() failed: user {email} is the owner of the board {boardId}");
                    throw new ElementAlreadyExistsException($"user '{email}' is the board's owner");
                }
                boardData.RemovePointerToJoinedBoard(email, boardId);
                board.LeaveBoard(email, boardId);
                log.Debug("LeaveBoard() success");
            }
            catch (UserDoesNotExistException)
            {
                log.Error("LeaveBoard() failed: the user " + email + " doesn't exist");
                throw new UserDoesNotExistException("the user " + email + " doesn't exist");
            }
            catch (UserNotLoggedInException e)
            {
                log.Error("JoinBoard() failed: " + e.Message);
                throw;
            }
            catch (AccessViolationException)
            {
                log.Error("LeaveBoard() failed: the user " + email + " is not joined to a board with Id '" + boardId + " '");
                throw new UserDoesNotExistException("the user " + email + " is not joined to a board with Id '" + boardId + " '");
            }
            catch (ArgumentException e)
            {
                log.Error("JoinBoard() failed: " + e.Message);
                throw;
            }
        }


        /// <summary>
        /// <b>Throws</b> <c>UserDoesNotExistException</c> if the user doesn't exist<br/>
        /// <b>Throws</b> <c>UserNotLoggedInException</c> if the user isn't logged in
        /// </summary>
        /// <param name="email"></param>
        /// <exception cref="UserDoesNotExistException"></exception>
        /// <exception cref="UserNotLoggedInException"></exception>
        private void ValidateUser(CIString email)
        {
            if (!boardData.UserExists(email))
            {
                log.Error("ValidateUser() failed: a user with the email '" +
                    email + "' doesn't exist in the system");
                throw new UserDoesNotExistException("A user with the email '" +
                    email + "' doesn't exist in the system");
            }
            if (!boardData.UserLoggedInStatus(email))
            {
                log.Error("ValidateUser() failed: user '" + email + "' isn't logged in");
                throw new UserNotLoggedInException("user '" + email + "' isn't logged in");
            }
        }


        /// <summary>
        /// Change <c>Board's Owner</c> to <c>BoardData</c> boardData <br/> <br/>
        /// <b>Throws</b> <c>ElementAlreadyExistsException</c> if a<c> Board</c> with that title already exists<br/>
        /// for the <c>User</c><br/>
        /// <b>Throws</b> <c>UserDoesNotExistException</c> if the user doesn't exist<br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if Board doesn't exist for the user<br/>
        /// <b>Throws</b> <c>AccessViolationException</c> if user isn't the owner<br/>
        /// </summary>
        /// <param name="currentOwnerEmail"></param>
        /// <param name="newOwnerEmail"></param>
        /// <param name="boardName"></param>
        /// <exception cref="ElementAlreadyExistsException"></exception>
        /// <exception cref="NoSuchElementException"></exception>
        /// <exception cref="UserDoesNotExistException"></exception>
        /// <exception cref="AccessViolationException"></exception>
        public void ChangeOwner(CIString currentOwnerEmail, CIString newOwnerEmail, CIString boardName)
        {
            log.Debug("ChangeOwner() for board: " + boardName + "from: " + currentOwnerEmail + "to: " + newOwnerEmail);
            try
            {
                Board board = SearchBoard(currentOwnerEmail, boardName);
                if (board.Owner == newOwnerEmail)
                {
                    log.Error($"ChangeOwner() failed: user {newOwnerEmail} is already the owner of the board {boardName}");
                    throw new ElementAlreadyExistsException($"user '{newOwnerEmail}' is already the board's owner");
                }
                if (board.Owner.Equals(currentOwnerEmail)==false)
                {
                    log.Error("ChangeOwner() failed: user isn't the board's owner");
                    throw new AccessViolationException("user isn't the board's owner");
                }
                boardData.ChangeOwnerPointer(currentOwnerEmail, boardName, newOwnerEmail);
                board.ChangeOwner(currentOwnerEmail, newOwnerEmail, boardName);
                log.Debug("ChangeOwner() success");
            }
            catch (ElementAlreadyExistsException e)
            {
                log.Error("ChangeOwner() failed: "+e.Message);
                throw;
            }
            catch (NoSuchElementException e)
            {
                log.Error("ChangeOwner() failed: " + e.Message);
                throw;
            }
            catch (UserDoesNotExistException e)
            {
                log.Error("ChangeOwner() failed: " + e.Message);
                throw;
            }
            catch (ArgumentException e)
            {
                log.Error("ChangeOwner() failed: " + e.Message);
                throw;
            }
        }

        public void AddTask(CIString email, CIString boardName, CIString title, CIString description, DateTime dueDate)
        {
            log.Debug("AddTask() for: " + title + ", " + description + ", " + dueDate);
            try
            {
                Board board = SearchBoard(email, boardName);
                board.AddTask(title, dueDate, description);
                log.Debug("AddTask() success");
            }
            catch (NoSuchElementException ex)
            {
                log.Error("AddTask() failed: " + ex.Message);
                throw;
            }
            catch (ArgumentException ex)
            {
                log.Error("AddTask() failed: " + ex.Message);
                throw;
            }
            catch (AccessViolationException ex)
            {
                log.Error("AddTask() failed: " + ex.Message);
                throw;
            }
            catch (UserDoesNotExistException ex)
            {
                log.Error("AddTask() failed: " + ex.Message);
                throw;
            }
            catch (UserNotLoggedInException ex)
            {
                log.Error("AddTask() failed: " + ex.Message);
                throw;
            }
        }



        public void RemoveTask(CIString email, CIString boardTitle, int taskId)
        {
            log.Debug("RemoveTask() taskId: " + taskId);
            try
            {
                Board board = SearchBoard(email, boardTitle);
                board.RemoveTask(taskId);
                log.Debug("RemoveTask() success");
            }
            catch (NoSuchElementException ex)
            {
                log.Error("RemoveTask() failed: " + ex.Message);
                throw;
            }
            catch (ArgumentException ex)
            {
                log.Error("RemoveTask() failed: " + ex.Message);
                throw;
            }
            catch (AccessViolationException ex)
            {
                log.Error("RemoveTask() failed: " + ex.Message);
                throw;
            }
            catch (UserDoesNotExistException ex)
            {
                log.Error("RemoveTask() failed: " + ex.Message);
                throw;
            }
            catch (UserNotLoggedInException ex)
            {
                log.Error("RemoveTask() failed: " + ex.Message);
                throw;
            }
        }


        public void AdvanceTask(CIString email, CIString boardName, int columnOrdinal, int taskId)
        {
            log.Debug("AdvanceTask() taskId: " + taskId);
            try
            {
                Board board = SearchBoard(email, boardName);
                Task task = board.SearchTask(taskId);
                board.AdvanceTask(email, columnOrdinal, taskId);
                task.AdvanceTask(email);
                log.Debug("AdvanceTask() success");
            }
            catch (NoSuchElementException ex)
            {
                log.Error("AdvanceTask() failed: " + ex.Message);
                throw;
            }
            catch (ArgumentException ex)
            {
                log.Error("AdvanceTask() failed: " + ex.Message);
                throw;
            }
            catch (IndexOutOfRangeException ex)
            {
                log.Error("AdvanceTask() failed: " + ex.Message);
                throw;
            }
            catch (AccessViolationException ex)
            {
                log.Error("AdvanceTask() failed: " + ex.Message);
                throw;
            }
            catch (UserDoesNotExistException ex)
            {
                log.Error("AdvanceTask() failed: " + ex.Message);
                throw;
            }
            catch (UserNotLoggedInException ex)
            {
                log.Error("AdvanceTask() failed: " + ex.Message);
                throw;
            }
        }



        public void LimitColumn(CIString email, CIString boardName, int columnOrdinal, int limit)
        {
            log.Debug("LimitColumn() for column and limit: " + columnOrdinal + ", " + limit);
            try
            {
                Board board = SearchBoard(email, boardName);
                board.LimitColumn(columnOrdinal, limit);
                log.Debug("LimitColumn() success");
            }
            catch (NoSuchElementException ex)
            {
                log.Error("LimitColumn() failed: " + ex.Message);
                throw;
            }
            catch (ArgumentException ex)
            {
                log.Error("LimitColumn() failed: " + ex.Message);
                throw;
            }
            catch (IndexOutOfRangeException ex)
            {
                log.Error("LimitColumn() failed: " + ex.Message);
                throw;
            }
            catch (AccessViolationException ex)
            {
                log.Error("LimitColumn() failed: " + ex.Message);
                throw;
            }
            catch (UserDoesNotExistException ex)
            {
                log.Error("LimitColumn() failed: " + ex.Message);
                throw;
            }
            catch (UserNotLoggedInException ex)
            {
                log.Error("LimitColumn() failed: " + ex.Message);
                throw;
            }
        }


        public int GetColumnLimit(CIString email, CIString boardName, int columnOrdinal)
        {
            log.Debug("GetColumnLimit() columnOrdinal: " + columnOrdinal);
            try
            {
                Board board = SearchBoard(email, boardName);
                int columnlimit = board.GetColumnLimit(columnOrdinal);
                log.Debug("GetColumnLimit() success");
                return columnlimit;
            }
            catch (NoSuchElementException ex)
            {
                log.Error("GetColumnLimit() failed: " + ex.Message);
                throw;
            }
            catch (ArgumentException ex)
            {
                log.Error("GetColumnLimit() failed: " + ex.Message);
                throw;
            }
            catch (IndexOutOfRangeException ex)
            {
                log.Error("GetColumnLimit() failed: " + ex.Message);
                throw;
            }
            catch (AccessViolationException ex)
            {
                log.Error("GetColumnLimit() failed: " + ex.Message);
                throw;
            }
            catch (UserDoesNotExistException ex)
            {
                log.Error("GetColumnLimit() failed: " + ex.Message);
                throw;
            }
            catch (UserNotLoggedInException ex)
            {
                log.Error("GetColumnLimit() failed: " + ex.Message);
                throw;
            }
        }

        public string GetColumnName(CIString email, CIString boardName, int columnOrdinal)
        {
            log.Debug("GetColumnName() columnOrdinal: " + columnOrdinal);
            try
            {
                Board board = SearchBoard(email, boardName);
                string columnname = board.GetColumnName(columnOrdinal);
                log.Debug("GetColumnName() success");
                return columnname;
            }
            catch (NoSuchElementException ex)
            {
                log.Error("GetColumnName() failed: " + ex.Message);
                throw;
            }
            catch (ArgumentException ex)
            {
                log.Error("GetColumnName() failed: " + ex.Message);
                throw;
            }
            catch (IndexOutOfRangeException ex)
            {
                log.Error("GetColumnName() failed: " + ex.Message);
                throw;
            }
            catch (AccessViolationException ex)
            {
                log.Error("GetColumnName() failed: " + ex.Message);
                throw;
            }
            catch (UserDoesNotExistException ex)
            {
                log.Error("GetColumnName() failed: " + ex.Message);
                throw;
            }
            catch (UserNotLoggedInException ex)
            {
                log.Error("GetColumnName() failed: " + ex.Message);
                throw;
            }
        }



        public LinkedList<Task> GetColumn(CIString email, CIString boardName, int columnOrdinal)
        {
            log.Debug("GetColumn() columnOrdinal: " + columnOrdinal);
            try
            {
                Board board = SearchBoard(email, boardName);
                LinkedList<Task> column = board.GetColumn(columnOrdinal);
                log.Debug("GetColumn() success");
                return column;
            }
            catch (NoSuchElementException ex)
            {
                log.Error("GetColumn() failed: " + ex.Message);
                throw;
            }
            catch (ArgumentException ex)
            {
                log.Error("GetColumn() failed: " + ex.Message);
                throw;
            }
            catch (IndexOutOfRangeException ex)
            {
                log.Error("GetColumn() failed: " + ex.Message);
                throw;
            }
            catch (AccessViolationException ex)
            {
                log.Error("GetColumn() failed: " + ex.Message);
                throw;
            }
            catch (UserDoesNotExistException ex)
            {
                log.Error("GetColumn() failed: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// <b>Throws</b> <c>IndexOutOfRangeException</c> if the column is not a valid column number
        /// </summary>
        /// <param name="columnOrdinal"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        private void ValidateColumnOrdinal(int columnOrdinal)
        {
            if (columnOrdinal < (int)TaskStates.backlog | columnOrdinal > (int)TaskStates.done)
            {
                log.Error("ValidateColumnOrdinal() failed: '" + columnOrdinal + "' is not a valid column number");
                throw new IndexOutOfRangeException("The column '" + columnOrdinal + "' is not a valid column number");
            }
        }

    }
}
