﻿using System;

namespace TFSGeneration.Control.DataBase.Settings
{
	[Serializable]
	public class TFSFieldsException : Exception
	{
		public TFSFieldsException()
		{

		}
		public TFSFieldsException(string message) : base(message)
		{

		}
		public TFSFieldsException(string fromat, params object[] obj_params) : base(string.Format(fromat, obj_params))
		{

		}
	}
}
