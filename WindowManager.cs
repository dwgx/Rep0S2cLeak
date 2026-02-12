using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
	public static WindowManager instance;

	[DllImport("user32.dll")]
	public static extern bool SetWindowText(IntPtr hwnd, string lpString);

	[DllImport("user32.dll")]
	public static extern IntPtr FindWindow(string className, string windowName);

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			SetWindowText(FindWindow(null, "Repo"), "R.E.P.O.");
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
