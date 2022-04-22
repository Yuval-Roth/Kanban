﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer.Serializable
{
    [Serializable]
    public sealed class Board_Serializable
    {
        public string Title { get; set; }
        public LinkedList<Task_Serializable> Backlog { get; set; }
        public LinkedList<Task_Serializable> InProgress { get; set; }
        public LinkedList<Task_Serializable> Done { get; set; }
    }
}
