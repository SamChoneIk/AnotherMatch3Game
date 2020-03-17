using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class ArrayLayout
{
	[Serializable]
	public struct rowData
    {
		public bool[] row;
	}

    public Grid grid;
    public rowData[] rows = new rowData[14]; //Grid of 7x7
}