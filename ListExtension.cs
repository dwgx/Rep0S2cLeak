using System.Collections.Generic;
using UnityEngine;

public static class ListExtension
{
	public static void Shuffle<T>(this IList<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			list.Swap(i, Random.Range(0, list.Count));
		}
	}

	public static void Swap<T>(this IList<T> list, int i, int j)
	{
		T val = list[j];
		T val2 = list[i];
		T val3 = (list[i] = val);
		val3 = (list[j] = val2);
	}
}
