using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DeepCopy  {

	public static object Copy(object obj)
	{
		if (obj == null)
			return null;
		Type type = obj.GetType();

		if (type.IsValueType || type == typeof(string))
		{
			return obj;
		}
		else if (type.IsArray)
		{
			Type elementType = Type.GetType(
				type.FullName.Replace("[]", string.Empty));
			var array = obj as Array;
			Array copied = Array.CreateInstance(elementType, array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				copied.SetValue(Copy(array.GetValue(i)), i);
			}
			return Convert.ChangeType(copied, obj.GetType());
		}
		else if (type.IsClass)
		{

			object toret = Activator.CreateInstance(obj.GetType());
			FieldInfo[] fields = type.GetFields(BindingFlags.Public |
			                                    BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (FieldInfo field in fields)
			{
				object fieldValue = field.GetValue(obj);
				if (fieldValue == null)
					continue;
				field.SetValue(toret, Copy(fieldValue));
			}
			return toret;
		}
		else
			throw new ArgumentException("Unknown type");
	}
}
