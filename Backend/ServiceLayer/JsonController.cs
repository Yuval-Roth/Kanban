﻿using System;
using System.Text.Json;
using System.Collections.Generic;

namespace IntroSE.Kanban.Backend.ServiceLayer 
{
	public static class JsonController
	{
		private readonly static JsonSerializerOptions options = new()
		{
			WriteIndented = true,
		};
		public static string Serialize<T>(T obj)
		{
			return JsonSerializer.Serialize<T>(obj, options);
		}
		public static T Deserialize<T>(string json)
		{
			return JsonSerializer.Deserialize<T>(json, options);
		}
		
		
		// these are temporary functions


		public static string ConvertToJson(BusinessLayer.Task task)
		{
			return Serialize(task.GetSerializableInstance());
		}
		public static string ConvertToJson(BusinessLayer.Board board)
		{
			return Serialize(board.GetSerializableInstance());
		}
		public static string ConvertToJson(LinkedList<BusinessLayer.Board> boardList)
		{
			LinkedList<BusinessLayer.Serializable.Board_Serializable> boardList_Serializable = new();
			foreach (BusinessLayer.Board board in boardList) 
			{
				boardList_Serializable.AddLast(board.GetSerializableInstance());
			}
			return Serialize(boardList_Serializable);
		}
		public static string ConvertToJson(BusinessLayer.User user)
		{
			return Serialize(user.GetSerializableInstance());
		}
		public static string ConvertToJson(Response response)
		{
			return Serialize(response);
		}
		public static T BuildFromJson<T>(string json)
		{
			return JsonController.Deserialize<T>(json);
		}
	}
}
