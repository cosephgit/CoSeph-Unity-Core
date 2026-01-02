using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// CSFlexibleGridLayoutGroup
// credit to Game Dev Guide
// https://www.youtube.com/watch?v=CGsEJToeXmA&ab_channel=GameDevGuide
// created 13/11/24
// modified 13/11/24

public class CSFlexibleGridLayoutGroup : LayoutGroup
{
    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }
    [Header("Settings")]
    public FitType fitType;
    public Vector2 spacing;
    [Header("Calculated(sometimes)")]
    public int rows;
    public int columns;
    public Vector2 cellSize;
    public bool fitX;
    public bool fitY;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (fitType == FitType.Uniform || fitType == FitType.Width || fitType == FitType.Height)
        {
            float sqrRt = Mathf.Sqrt(transform.childCount);

            fitX = true;
            fitY = true;
            rows = columns = Mathf.CeilToInt(sqrRt);
        }

        if (fitType == FitType.Width || fitType == FitType.FixedColumns)
        {
            rows = Mathf.CeilToInt(transform.childCount / (float)(columns));
        }
        else if (fitType == FitType.Height || fitType == FitType.FixedRows)
        {
            columns = Mathf.CeilToInt(transform.childCount / (float)(rows));
        }

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;
        float cellWidth = parentWidth / (float)columns - ((spacing.x / (float)columns) * 2) - ((padding.left + padding.right) / (float)columns);
        float cellHeight = parentHeight / (float)rows - ((spacing.y / (float)rows) * 2) - ((padding.top + padding.bottom) / (float)rows);

        cellSize.x = fitX ? cellWidth : cellSize.x;
        cellSize.y = fitY ? cellHeight : cellSize.y;

        int columnCount = 0;
        int rowCount = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / columns;
            columnCount = i % columns;

            var item = rectChildren[i];
            var xPos = (cellSize.x + spacing.x ) * columnCount + padding.left;
            var yPos = (cellSize.y + spacing.y) * rowCount + padding.top;

            SetChildAlongAxis(item, 0, xPos, cellSize.x);
            SetChildAlongAxis(item, 1, yPos, cellSize.y);

        }
    }

    public override void CalculateLayoutInputVertical()
    {
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void SetLayoutVertical()
    {
    }
}
