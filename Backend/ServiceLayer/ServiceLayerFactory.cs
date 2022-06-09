﻿
namespace IntroSE.Kanban.Backend.ServiceLayer
{


    /// <summary>
    /// A factory for the service layer classes<br/>
    /// This factory implements the singleton pattern<br/>
    /// use GetInstance() to get an instance of this factory.<br/><br/>
    /// <b>Note:</b> this factory instantiates each class exactly once and does not produce duplicates
    /// <code>Inventory:</code>
    /// <list type="bullet">
    /// <item>BoardControllerService</item>
    /// <item>BoardService</item>
    /// <item>TaskService</item>
    /// <item>UserService</item>
    /// </list>
    /// </summary>
    public class ServiceLayerFactory
    {
        private static ServiceLayerFactory instance = null;

        private BoardControllerService boardControllerService;
        private BoardService boardService;
        private TaskService taskService;
        private UserService userService;

        private ServiceLayerFactory() 
        {
            BusinessLayer.BusinessLayerFactory BLFactory = BusinessLayer.BusinessLayerFactory.GetInstance();
            boardControllerService = new(BLFactory.BoardController);
            boardService = new(BLFactory.BoardController);
            taskService = new(BLFactory.BoardController);
            userService = new(BLFactory.UserController);
        }

        /// <summary>
        /// Get the BoardControllerService instance
        /// </summary>
        public BoardControllerService BoardControllerService => boardControllerService;

        /// <summary>
        /// Get the BoardService instance
        /// </summary>
        public BoardService BoardService => boardService;

        /// <summary>
        /// Get the TaskService instance
        /// </summary>
        public TaskService TaskService => taskService;

        /// <summary>
        /// Get the TaskService instance
        /// </summary>
        public UserService UserService => userService;


        /// <summary>
        /// Get the factory instance
        /// </summary>
        public static ServiceLayerFactory GetInstance()
        {
            if (instance == null) instance = new();
            return instance;
        }
    }
}
