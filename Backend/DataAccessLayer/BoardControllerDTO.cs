﻿
namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    public class BoardControllerDTO
    {
        private SQLExecuter executer;

        public BoardControllerDTO(SQLExecuter executer)
        {
            this.executer = executer;
        }

        public bool AddBoard(int Id,string Title ,string Owner)
        {
            return executer.ExecuteWrite ("INSERT into Boards (BoardId, BoardTitle, Owner, BacklogLimit, InprogressLimit, DoneLimit) " +
                $"VALUES({Id},'{Title}','{Owner}',-1,-1,-1)");
        }
        public bool RemoveBoard(int id)
        {
            return executer.ExecuteWrite($"DELETE FROM Boards WHERE Boards.BoardId={id} DELETE FROM UserJoinedBoards WHERE UserJoinedBoards.BoardId={id}");
        }
        public bool JoinBoard(string email, int id)
        {
            return executer.ExecuteWrite("INSERT into UserJoinedBoards (BoardId, Email)" +
                $"VALUES({id},'{email}')");
        }
        public bool LeaveBoard(string email, int id)
        {
            return executer.ExecuteWrite("DELETE FROM UserJoinedBoards" +
                $"WHERE BoardId= '{id}' and Email= '{email}'");
        }
        public bool ChangeOwner(string email, int id)
        {
            return executer.ExecuteWrite("UPDATE Boards"+
            $"SET Owner = '{email}'"+
            $"WHERE TaskId = {id}");
        }
        public bool LimitColumn(int id, BoardColumnNames column, int limit)
        {
            return executer.ExecuteWrite("UPDATE Boards" +
            $"SET {column}Limit = {limit}" +
            $"WHERE BoardId = {id}");
        }
        public bool UpdateBoardIdCounter(int newValue)
        {
            return executer.ExecuteWrite("UPDATE BoardIDCounter" +
                                        $"SET Counter = {newValue}");

        }

    }
}
