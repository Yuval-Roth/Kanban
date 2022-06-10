﻿using System;
using System.Collections.Generic;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    public enum TaskStates
    {
        backlog,
        inprogress,
        done
    }

    /// <summary>
    ///This class controls the actions users' board.<br/>
    ///<br/>
    ///<code>Supported operations:</code>
    ///<br/>
    /// <list type="bullet">AddTask()</list>
    /// <list type="bullet">RemoveTask()</list>
    /// <list type="bullet">AdvanceTask()</list>
    /// <list type="bullet">SearchTask()</list>
    /// <list type="bullet">GetColumnLimit()</list>
    /// <list type="bullet">GetColumnName()</list>
    /// <list type="bullet">GetColumn()</list>
    /// <list type="bullet">LimitColumn()</list>
    /// <br/><br/>
    /// ===================
    /// <br/>
    /// <c>Ⓒ Kfir Nissim</c>
    /// <br/>
    /// ===================
    /// </summary>
    public class Board
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Backend\\BusinessLayer\\Board.cs");

        private readonly int id;
        private readonly string title;
        private string owner;
        private readonly LinkedList<string> joined;
        private readonly LinkedList<Task>[] columns;
        private readonly int[] columnLimit;
        private readonly Dictionary<int, TaskStates> taskStateTracker;
        private int taskIDCounter;


        public Board(string title, int id, string owner)
        {
            this.id = id;
            this.title = title;
            this.owner = owner;
            joined = new();
            columnLimit = new int[3];
            columns = new LinkedList<Task>[3];
            columnLimit[(int)TaskStates.backlog] = -1;
            columnLimit[(int)TaskStates.inprogress] = -1;
            columnLimit[(int)TaskStates.done] = -1;
            columns[(int)TaskStates.backlog] = new();
            columns[(int)TaskStates.inprogress] = new();
            columns[(int)TaskStates.done] = new();
            taskStateTracker = new();
        }

        public Board(DataAccessLayer.BoardDTO boardDTO)
        {
            id = boardDTO.Id;
            title = boardDTO.Title;
            owner = boardDTO.Owner;
            joined = boardDTO.Joined;
            columnLimit = new int[3];
            columns = new LinkedList<Task>[3];
            columns[(int)TaskStates.backlog] = new();
            columns[(int)TaskStates.inprogress] = new();
            columns[(int)TaskStates.done] = new();
            columnLimit[(int)TaskStates.backlog] = boardDTO.BackLogLimit;
            columnLimit[(int)TaskStates.inprogress] = boardDTO.InProgressLimit;
            columnLimit[(int)TaskStates.done] = boardDTO.DoneLimit;
            taskStateTracker = new();

            foreach (DataAccessLayer.TaskDTO taskDTO in boardDTO.BackLog)
            {
                Task task = new(taskDTO);
                taskStateTracker.Add(task.Id,task.State);
                columns[(int)TaskStates.backlog].AddLast(task);
            }
            foreach (DataAccessLayer.TaskDTO taskDTO in boardDTO.InProgress)
            {
                Task task = new(taskDTO);
                taskStateTracker.Add(task.Id, task.State);
                columns[(int)TaskStates.inprogress].AddLast(task);
            }
            foreach (DataAccessLayer.TaskDTO taskDTO in boardDTO.Done)
            {
                Task task = new(taskDTO);
                taskStateTracker.Add(task.Id, task.State);;
                columns[(int)TaskStates.done].AddLast(task);
            }
            
        }


        //====================================
        //         getters/initializers
        //====================================

        public int Id { get { return id; } init { id = value; } }
        public string Title { get { return title; } init { title = value; } }
        public string Owner { get { return owner; } init { owner = value; } }
        public LinkedList<string> Joined { get { return joined; } init { joined = value; } }
        public LinkedList<Task>[] Columns { /*get { return columns; }*/ init { columns = value; } }
        public int[] ColumnLimit { /*get { return columnLimit; }*/ init { columnLimit = value; } }
        public Dictionary<int, TaskStates> TaskStateTracker { /*get { return taskStateTracker; }*/ init { taskStateTracker = value; } }
        public int TaskIDCounter { /*get { return taskIDCounter; }*/ init { taskIDCounter = value; } }

        //====================================
        //            Functionality
        //====================================


        /// <summary>
        /// Add new <c>Task</c> to <c>Board</c> board <br/> <br/>
        /// <b>Throws</b> <c>ArgumentException</c> if the backlog column reached the limit
        /// </summary>
        /// <param name="title"></param>
        /// <param name="duedate"></param>
        /// <param name="description"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddTask(string title, DateTime duedate, string description)
        {
            log.Debug("AddTask() for: " + title + ", " + description + ", " + duedate);
            if (columns[(int)TaskStates.backlog].Count != columnLimit[(int)TaskStates.backlog])
            {
                try
                {
                    columns[(int)TaskStates.backlog].AddLast(new Task(taskIDCounter, title, duedate, description));
                    taskStateTracker.Add(taskIDCounter, TaskStates.backlog);
                    taskIDCounter++;
                    log.Debug("AddTask() success");
                }
                catch (ArgumentException e)
                {
                    log.Error("AddTask() failed: '" + e.Message);
                    throw new ArgumentException(e.Message);
                }
                catch (NoSuchElementException e)
                {
                    log.Error("AddTask() failed: '" + e.Message);
                    throw new NoSuchElementException(e.Message);
                }
            }
            else 
            {
                log.Error("AddTask() failed: Backlog in board '" + this.title + "' has reached its limit and can't contain more tasks");
                throw new ArgumentException("Backlog in board '" + this.title + "' has reached its limit and can't contain more tasks");
            }
        }

        /// <summary>
        /// Remove <c>Task</c> from <c>Board</c> board <br/> <br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the task doesn't exist
        /// </summary>
        /// <param name="taskID"></param>
        /// <exception cref="NoSuchElementException"></exception>
        public void RemoveTask(int taskId)
        {
            log.Debug("RemoveTask() taskId: " + taskId);
            if (taskStateTracker.ContainsKey(taskId))
            {
                LinkedList<Task> taskList = columns[(int)taskStateTracker[taskId]];
                foreach (Task task in taskList)
                {
                    if (task.Id == taskId)
                    {
                        taskList.Remove(task);
                        taskStateTracker.Remove(taskId);
                        log.Debug("RemoveTask() success");
                        break;
                    }
                }       
            }
            else
            {
                log.Error("RemoveTask() failed: task numbered '" + taskId + "' doesn't exist");
                throw new NoSuchElementException("A Task with the taskId '" +
                    taskId + "' doesn't exist in the Board");
            }
        }


        /// <summary>
        /// Advance <c>Task</c> from <c>Board</c> board <br/> <br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the task doesn't exist at all or is not in the specified column<br/>
        /// <b>Throws</b> <c>ArgumentException</c> if the task can't be advanced<br/>
        /// <b>Throws</b> <c>IndexOutOfRangeException</c> if the column is not a valid column number
        /// <b>Throws</b> <c>AccessViolationException</c> if the user isn't task assignee
        /// </summary>
        /// <param name="email"></param>
        /// <param name="taskId"></param>
        /// <param name="columnOrdinal"></param>
        /// <exception cref="NoSuchElementException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="AccessViolationException"></exception>
        public void AdvanceTask(string email, int columnOrdinal, int taskId)
        {
            log.Debug("AdvanceTask() for column and taskId: " + columnOrdinal + ", " + taskId);
            ValidateColumnOrdinal(columnOrdinal);
            Task task = SearchTask(taskId);
            if (task.Assignee != email)
            {
                log.Error("AdvanceTask() failed: User is not the task's assignee");
                throw new AccessViolationException("User is not the task's assignee");
            }

            if (taskStateTracker.ContainsKey(taskId))
            {
                TaskStates state = taskStateTracker[taskId];
                if ((int)state == columnOrdinal)
                {
                    if (state != TaskStates.done)
                    {
                        if (columns[(int)state + 1].Count < columnLimit[(int)state + 1] | columnLimit[(int)state + 1] == -1)
                        {
                            Task toAdvance = SearchTask(taskId);
                            columns[(int)state].Remove(toAdvance);
                            columns[(int)state + 1].AddLast(toAdvance);
                            taskStateTracker[taskId] = state + 1;
                            log.Debug("AdvanceTask() success");
                        }
                        else
                        {
                            log.Error("AdvanceTask() failed: task numbered '" + taskId + "' can't be advanced because the next column is full");
                            throw new ArgumentException("task numbered '" + taskId + "' can't be advanced because the next column is full");
                        }
                        
                    }
                    else
                    {
                        log.Error("AdvanceTask() failed: task numbered '" + taskId + "' is done and can't be advanced");
                        throw new ArgumentException("task numbered '" + taskId + "' is done and can't be advanced");
                    }
                }
                else
                {
                    log.Error("AdvanceTask() failed: task numbered '" + taskId + "' isn't in the column " + (TaskStates)columnOrdinal);
                    throw new NoSuchElementException("the task '" +
                        taskId + "' isn't in the column " + (TaskStates)columnOrdinal);
                }
            }
            else
            {
                log.Error("AdvanceTask() failed: task numbered '" + taskId + "' doesn't exist");
                throw new NoSuchElementException("task numbered '" + taskId + "' doesn't exist");
            }
            
        }


        /// <summary>
        /// Search <c>Task</c> from <c>Board</c> board <br/> <br/>
        /// <b>Throws</b> <c>NoSuchElementException</c> if the task doesn't exist in the specified column <br/>
        /// <b>Throws</b> <c>IndexOutOfRangeException</c> if the column is not a valid column number
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="columnOrdinal"></param>
        /// <exception cref="NoSuchElementException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Task SearchTask(int taskId, int columnOrdinal)
        {
            log.Debug("SearchTask() taskId, columnOrdinal: " + taskId + ", " + columnOrdinal);
            ValidateColumnOrdinal(columnOrdinal);

            LinkedList<Task> taskList = columns[columnOrdinal];
            foreach (Task task in taskList)
            {
                if (task.Id == taskId)
                {
                    log.Debug("SearchTask() success");
                    return task;
                }
            }
            log.Error("SearchTask() failed: A task numbered '" + taskId +
                "' doesn't exist in column '" + columnOrdinal + "'");
            throw new NoSuchElementException("A Task with the taskId '" +
                taskId + "' doesn't exist in column '" + columnOrdinal + "'");
        }

        /// <summary>
        /// Search <c>Task</c> from <c>Board</c> board <br/> <br/>
        /// <b>Throws</b> <c>UserDoesNotExistException</c> if the task doesn't exist
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns>Task, unless an error occurs</returns>
        /// <exception cref="UserDoesNotExistException"></exception>
        public Task SearchTask(int taskId) 
        {
            log.Debug("SearchTask() taskId: " + taskId);
            if (taskStateTracker.ContainsKey(taskId))
            {
                LinkedList<Task> taskList = columns[(int)taskStateTracker[taskId]];
                foreach (Task task in taskList)
                {
                    if (task.Id == taskId)
                    {
                        log.Debug("SearchTask() success");
                        return task;
                    }
                }
                //======================================================================================================
                // this part of the code should generally never run. if it does, there is a serious problem somewhere.
                //======================================================================================================
                log.Fatal("FATAL ERROR: task numbered" + taskId + "exists in the taskStateTracker and not in the column '" +
                    taskStateTracker[taskId] + "' where it's supposed to be");
                throw new OperationCanceledException("FATAL ERROR: task numbered" + taskId +
                    "exists in the taskStateTracker and not in the column '" +
                    taskStateTracker[taskId] + "' where it's supposed to be");
                //======================================================================================================
            }
            else
            {
                log.Error("SearchTask() failed: A task numbered '" + taskId + "' doesn't exist");
                throw new UserDoesNotExistException("A Task with the taskId '" +
                    taskId + "' doesn't exist in the Board");
            }           
        }

        /// <summary>
        /// Get<c>column limit</c> from <c>Board</c> board <br/> <br/>
        /// <b>Throws</b> <c>IndexOutOfRangeException</c> if the column is not a valid column number
        /// </summary>
        /// <param name="columnOrdinal"></param>
        /// <returns>int column limit, unless an error occurs</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int GetColumnLimit(int columnOrdinal)
        {
            log.Debug("GetColumnLimit() columnOrdinal: " + columnOrdinal);
            ValidateColumnOrdinal(columnOrdinal);

            log.Debug("GetColumnLimit() success");
            return columnLimit[columnOrdinal];
        }


        /// <summary>
        /// Get<c>column name</c> from <c>Board</c> board <br/> <br/>
        /// <b>Throws</b> <c>IndexOutOfRangeException</c> if the column is not a valid column number
        /// </summary>
        /// <param name="columnOrdinal"></param>
        /// <returns>column name, unless an error occurs</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public string GetColumnName(int columnOrdinal)
        {
            log.Debug("GetColumnName() columnOrdinal: " + columnOrdinal);
            ValidateColumnOrdinal(columnOrdinal);
            
            log.Debug("GetColumnName() success");
            switch (columnOrdinal)
            {
                case 1: return "in progress";

                default: return ((TaskStates)columnOrdinal).ToString();
            }
            
        }

        /// <summary>
        /// Get<c>column</c> from <c>Board</c> board <br/> <br/>
        /// <b>Throws</b> <c>IndexOutOfRangeException</c> if the column is not a valid column number
        /// </summary>
        /// <param name="columnOrdinal"></param>
        /// <returns>LinkedList of task, unless an error occurs</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public LinkedList<Task> GetColumn(int columnOrdinal)
        {
            log.Debug("GetColumn() columnOrdinal: " + columnOrdinal);
            ValidateColumnOrdinal(columnOrdinal);

            log.Debug("GetColumn() success");
            return columns[columnOrdinal];
        }

        /// <summary>
        /// Limit<c>column</c> from <c>Board</c> board <br/> <br/>
        /// <b>Throws</b> <c>ArgumentException</c> if column size is over the specified limit or the limit is invalid<br/>
        /// <b>Throws</b> <c>IndexOutOfRangeException</c> if the column is not a valid column number
        /// </summary>
        /// <param name="columnOrdinal"></param>
        /// <param name="limit"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void LimitColumn(int columnOrdinal, int limit)
        {
            log.Debug("LimitColumn() for column and limit: " + columnOrdinal + ", " + limit);
            ValidateColumnOrdinal(columnOrdinal);

            if (limit >= -1)
            {
                if (limit != -1 & columns[columnOrdinal].Count > limit)
                {
                    log.Error("LimitColumn() failed: '" + (TaskStates)columnOrdinal + "' size is bigger than the limit " + limit);
                    throw new ArgumentException("A column '" +
                        (TaskStates)columnOrdinal + "' size is bigger than th limit " + limit);
                }
                columnLimit[columnOrdinal] = limit;
                log.Debug("LimitColumn() success");
            }
            else 
            {
                log.Error("LimitColumn() failed: '" + limit + "' the limit is not valid");
                throw new ArgumentException("A limit '" +
                    limit + "' is not valid");
            } 
        }


        /// <summary>
        /// Change <c>Board's Owner</c> to <c>Board</c> <br/> <br/>
        /// <b>Throws</b> <c>ArgumentException</c> if the newOwnerEmail doesn't joined to the board<br/>
        /// </summary>
        /// <param name="currentOwnerEmail"></param>
        /// <param name="newOwnerEmail"></param>
        /// <param name="boardName"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ChangeOwner(string currentOwnerEmail, string newOwnerEmail, string boardName)
        {
            log.Debug("ChangeOwner() for board: " + boardName + "from: " + currentOwnerEmail + "to: " + newOwnerEmail);
            if (!this.joined.Contains(newOwnerEmail))
            {
                log.Error("ChangeOwner() failed: '" + newOwnerEmail + "' isn't joined to the board");
                throw new ArgumentException("the user " + newOwnerEmail + " isn't joined to the board");
            }
            this.owner = newOwnerEmail;
            this.joined.AddLast(currentOwnerEmail);
            this.joined.Remove(newOwnerEmail);
            log.Debug("ChangeOwner() success");
        }

        /// <summary>
        /// add <c>User</c> to <c>Board</c> joined boards <br/><br/>
        /// <b>Throws</b> <c>ArgumentException</c> if the user is the board's owner or if the user alredy joined to the board<br/>
        /// </summary>
        /// <param name="email"></param>
        /// <param name="boardId"></param>
        /// <exception cref="ArgumentException"></exception>
        public void JoinBoard(string email, int boardId)
        {
            log.Debug("JoinBoard() for user: " + email + "for board " + boardId);
            if(this.owner == email)
            {
                log.Error("JoinBoard() failed: user with email '" + email + "' is the board's owner");
                throw new AccessViolationException("the user " + email + " is the board's owner");
            }
            if (this.joined.Contains(email)){
                log.Error("JoinBoard() failed: user with email '" + email + "' already joined to the board");
                throw new ArgumentException("the user " + email + " already joined to the board");
            }
            this.joined.AddLast(email);
            log.Debug("JoinBoard() success");
        }
        /// <summary>
        /// remove <c>User</c> from <c>Board</c> joined boards and move all it's assigned task to unassigned <br/><br/>
        /// <b>Throws</b> <c>ArgumentException</c> if the user isn't joined to the board<br/>
        /// </summary>
        /// <param name="email"></param>
        /// <param name="boardId"></param>
        /// <exception cref="ArgumentException"></exception>
        public void LeaveBoard(string email, int boardId)
        {
            log.Debug("LeaveBoard() for user: " + email + "for board " + boardId);
            if (!this.joined.Contains(email))
            {
                log.Error("LeaveBoard() failed: user with email '" + email + "' is not joined to the board");
                throw new ArgumentException("user with email '" + email + "' is not joined to the board");
            }
            this.joined.Remove(email);
            foreach(Task task in this.columns[(int)TaskStates.backlog])
            {
                if (task.Assignee == email)
                {
                    task.Assignee = "unAssigned";
                }
            }
            foreach (Task task in this.columns[(int)TaskStates.inprogress])
            {
                if (task.Assignee == email)
                {
                    task.Assignee = "unAssigned";
                }
            }
            log.Debug("LeaveBoard() success");
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



        //====================================================
        //                  Json related
        //====================================================

        public Serializable.Board_Serializable GetSerializableInstance() 
        {
            LinkedList<Serializable.Task_Serializable>[] serializableColumns = new LinkedList<Serializable.Task_Serializable>[3];

            for (int column = (int)TaskStates.backlog ; column <= (int)TaskStates.done; column++)
            {
                serializableColumns[column] = new LinkedList<Serializable.Task_Serializable>();
                foreach (Task task in columns[column])
                {
                    serializableColumns[column].AddLast(task.GetSerializableInstance());
                }
            }
            return new Serializable.Board_Serializable()
            {
                Title = title,
                Columns = columns,
                ColumnLimit = columnLimit,
                TaskStateTracker = taskStateTracker
            };
        }
    }
}
