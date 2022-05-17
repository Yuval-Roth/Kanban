﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    public class Board
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Backend\\BusinessLayer\\Board.cs");

        private int counterID;
        private string title;
        private Dictionary<int, LinkedList<Task>> columns;
        private int [] columnLimit;

        //====================================
        //            getters/setters
        //====================================

        public Board(string title)
        {
            this.title = title;
            counterID = 0;
            columnLimit = new int[3];
            columns = new();
            columnLimit[(int)TaskStates.backlog] = -1;
            columnLimit[(int)TaskStates.inprogress] = -1;
            columnLimit[(int)TaskStates.done] = -1;
            columns.Add((int)TaskStates.backlog, new LinkedList<Task>());
            columns.Add((int)TaskStates.inprogress, new LinkedList<Task>());
            columns.Add((int)TaskStates.done, new LinkedList<Task>());
        }
        public string Title
        { 
            get { return title; }
            set { title = value; }
        }


        //====================================
        //            Functionality
        //====================================


        public void AddTask(string title, DateTime duedate, string description)
        {
            log.Debug("AddTask() for taskId: " + title + ", " + description + ", " + duedate);
            if (columnLimit[0]!=-1 && columns[0]!=null && columns[0].Count() == columnLimit[0])
            {
                log.Error("AddTask() failed: board '" + this.title + "' has a limit and can't contains more task");
                throw new ArgumentException("A board titled " +
                        this.title + " has a limit and can't contains more task");
            }
            LinkedList<Task> list = columns[0];
            list.AddLast(new Task(counterID, title, duedate, description));
            counterID++;
            columns[0]= list;
            log.Debug("AddTask() success");
        }

        public void RemoveTask(int taskId)
        {
            log.Debug("RemoveTask() taskId: " + taskId);
            bool found = false;
            for (int i = 0; i < columns.Count; i++)
            {
                LinkedList<Task> list = columns[i];
                foreach (Task task in list)
                {
                    if (task.Id == taskId) { found = true; list.Remove(task); break; }
                }
                if (found) { break; }
            }
            if (!found)
            {
                log.Error("RemoveTask() failed: '" + taskId + "' doesn't exist");
                throw new NoSuchElementException("A Task with the taskId '" +
                    taskId + "' doesn't exist in the Board");
            }
            else { log.Debug("RemoveTask() success"); }
        }

        public void AdvanceTask(int columnOrdinal, int taskId)
        {
            log.Debug("AdvanceTask() for column and taskId: " + columnOrdinal + ", " + taskId);
            if (columnOrdinal<0 || columnOrdinal > 2) 
            {
                log.Error("AdvanceTask() failed: '" + columnOrdinal + "' doesn't exist");
                throw new NoSuchElementException("A column '" +
                    columnOrdinal + "' doesn't exist in the Board");
            }
            if (columnOrdinal == 2)
            {
                log.Error("AdvanceTask() failed: '" + columnOrdinal + "'value is done");
                throw new ArgumentException("the task '" +
                    taskId + "' is already done");
            }
            bool found = false;
            foreach(Task task in columns[columnOrdinal])
            {
                if (task.Id == taskId) { found=true; break; }
            }
            if (!found)
            {
                log.Error("AdvanceTask() failed: '" + taskId + "'doesn't found in the column " +columnOrdinal);
                throw new NoSuchElementException("the task '" +
                    taskId + "'doesn't found in the column " + columnOrdinal);
            }
            int nextcolumnordinal = columnOrdinal + 1;
            if(columns[nextcolumnordinal].Count() == columnLimit[nextcolumnordinal])
            {
                log.Error("AdvanceTask() failed: '" + taskId + "'the next column " + nextcolumnordinal + "'is over the limit");
                throw new ArgumentException("'the next column " + nextcolumnordinal + "'is over the limit");
            }
            Task toAdvance = SearchTask(taskId);
            columns[columnOrdinal].Remove(toAdvance);
            columns[nextcolumnordinal].AddLast(toAdvance);
            log.Debug("AdvanceTask() success");
        }

        public Task SearchTask(int taskId, int columnOrdinal)
        {
            log.Debug("SearchTask() taskId: " + taskId);
            LinkedList<Task> list = columns[columnOrdinal];
            foreach (Task task in list)
            {
                if (task.Id == taskId) { log.Debug("SearchTask() success"); return task; }
            }
            log.Error("SearchTask() failed: '" + taskId + "' doesn't exist");
            throw new NoSuchElementException("A Task with the taskId '" +
                taskId + "' doesn't exist in the Board");
        }

        public Task SearchTask(int taskId) 
        {
            log.Debug("SearchTask() taskId: " + taskId);
            for (int i = 0; i < columns.Count; i++)
            {
                LinkedList<Task> list = columns[i];
                foreach (Task task in list)
                {
                    if (task.Id == taskId) {log.Debug("SearchTask() success"); return task; }
                }
            }   
                log.Error("SearchTask() failed: '" + taskId + "' doesn't exist");
                throw new NoSuchElementException("A Task with the taskId '" +
                    taskId + "' doesn't exist in the Board");
        }


        public int GetColumnLimit(int columnOrdinal)
        {
            log.Debug("GetColumnLimit() columnOrdinal: " + columnOrdinal);
            if (columnOrdinal < 0 || columnOrdinal > 2)
            {
                log.Error("GetColumnLimit() failed: '" + columnOrdinal + "' doesn't exist");
                throw new NoSuchElementException("A column '" +
                    columnOrdinal + "' doesn't exist in the Board");
            }
            if (columnLimit[columnOrdinal] == -1)
            {
                log.Error("GetColumnLimit() failed: '" + columnOrdinal + "' has no limit");
                throw new ArgumentException("A column '" +
                    columnOrdinal + "' has no limit");
            }
            log.Debug("GetColumnLimit() success");
            return columnLimit[columnOrdinal];
        }


        public string GetColumnName(int columnOrdinal)
        {
            log.Debug("GetColumnName() columnOrdinal: " + columnOrdinal);
            if (columnOrdinal < 0 || columnOrdinal > 2)
            {
                log.Error("GetColumnName() failed: '" + columnOrdinal + "' doesn't exist");
                throw new NoSuchElementException("A column '" +
                    columnOrdinal + "' doesn't exist in the Board");
            }
            
            log.Debug("GetColumnName() success");
            return ((TaskStates) columnOrdinal).ToString();
        }


        public LinkedList<Task> GetColumn(int columnOrdinal)
        {
            log.Debug("GetColumn() columnOrdinal: " + columnOrdinal);
            if (columnOrdinal < 0 || columnOrdinal > 2)
            {
                log.Error("GetColumn() failed: '" + columnOrdinal + "' doesn't exist");
                throw new NoSuchElementException("A column '" +
                    columnOrdinal + "' doesn't exist in the Board");
            }

            log.Debug("GetColumn() success");
            return columns[columnOrdinal];
        }


        public void LimitColumn(int columnOrdinal, int limit)
        {
            log.Debug("LimitColumn() for column and limit: " + columnOrdinal + ", " + limit);
            if (limit <= 0)
            {
                log.Error("LimitColumn() failed: '" + limit + "' the limit is negative");
                throw new NoSuchElementException("A limit '" +
                    limit + "' is negative");
            }
            if (columnOrdinal < 0 || columnOrdinal > 2)
            {
                log.Error("LimitColumn() failed: '" + columnOrdinal + "' doesn't exist");
                throw new NoSuchElementException("A column '" +
                    columnOrdinal + "' doesn't exist in the Board");
            }
            if (columns[columnOrdinal].Count > limit)
            {
                log.Error("LimitColumn() failed: '" + columnOrdinal + "' size is bigger than th limit " +limit);
                throw new NoSuchElementException("A column '" +
                    columnOrdinal + "' size is bigger than th limit " + limit);
            }
            log.Debug("LimitColumn() success");
            columnLimit[columnOrdinal] = limit;
        }

        //====================================================
        //                  Json related
        //====================================================

        public Serializable.Board_Serializable GetSerializableInstance() 
        {
            LinkedList<Serializable.Task_Serializable> serializableBackLog = new();
            foreach (Task task in columns[0])
            {
                serializableBackLog.AddLast(task.GetSerializableInstance());
            }

            LinkedList<Serializable.Task_Serializable> serializableInProgress = new();
            foreach (Task task in columns[1])
            {
                serializableInProgress.AddLast(task.GetSerializableInstance());
            }

            LinkedList<Serializable.Task_Serializable> serializableDone = new();
            foreach (Task task in columns[2])
            {
                serializableDone.AddLast(task.GetSerializableInstance());
            }
            return new Serializable.Board_Serializable()
            {
                Title = title,
                Backlog = serializableBackLog,
                InProgress = serializableInProgress,
                Done = serializableDone,
            };
        }
    }
}
