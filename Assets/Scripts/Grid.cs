using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPos;
    private int[,] gridArray;
    private TextMesh[,] debugTextArray;

    public Grid(int width, int height, float cellSize, Vector3 originPos)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPos = originPos;

        gridArray = new int [width,height];
        debugTextArray = new TextMesh[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
          for (int y = 0; y < gridArray.GetLength(1); y++)
          {
              // Debug.Log(x+" "+y);

              GameObject gameObject = new GameObject("wText", typeof(TextMesh));
              gameObject.transform.localPosition = GetPos(x,y) + new Vector3(cellSize, cellSize) * 0.5f;
              TextMesh textMesh = gameObject.GetComponent<TextMesh>();
              textMesh.text = "0";
              textMesh.anchor = TextAnchor.MiddleCenter;
              debugTextArray[x,y] = textMesh;

              Debug.DrawLine(GetPos(x, y), GetPos(x, y+1), Color.white, 1000f);
              Debug.DrawLine(GetPos(x, y), GetPos(x+1, y), Color.white, 1000f);
          }
        }
        Debug.DrawLine(GetPos(0, height), GetPos(width, height), Color.white, 1000f);
        Debug.DrawLine(GetPos(width, 0), GetPos(width, height), Color.white, 1000f);

    }

    private Vector3 GetPos(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPos;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPos).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPos).y / cellSize);
    }

    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            debugTextArray[x, y].text = gridArray[x, y].ToString();
        }
    }

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return -1;
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetValue(x, y, value);
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public TextMesh GetTextObj(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return debugTextArray[x, y];
        }
        else
        {
            return null;
        }
    }

    public int[] getRow(int index)
    {
        int[] thisRow = new int[width];
        for (int i = 0; i < width; i++)
        {
            thisRow[i] = GetValue(i,index);
            // Debug.Log(thisRow[i]);
        }
        return thisRow;
    }

    public int[] getCol(int index)
    {
        int[] thisCol = new int[height];
        for (int j = 0; j < height; j++)
        {
            thisCol[j] = GetValue(index,j);
            // Debug.Log(thisCol[j]);
        }
        return thisCol;
    }

}
