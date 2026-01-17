#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// A Gizmo to demonstrate the functionality of the BiggestRect method
    /// GUI Buttons allow generating Vector3Int sheet grids and find Rects in them
    /// </summary>
    public class CSGUISampleBiggestRect : MonoBehaviour
    {
        [Header("Button placements")]
        [SerializeField] private Rect _buttonPlaceNodes = new Rect(20, 20, 160, 40);
        [SerializeField] private Rect _buttonRectFind = new Rect(200, 20, 160, 40);
        [SerializeField] private Rect _buttonRectClear = new Rect(380, 20, 160, 40);
        [Header("Node placement")]
        [SerializeField] private int _gridWidth = 40;
        [SerializeField] private int _gridheight = 24;
        [SerializeField] private float _gridMaxScreen = 0.8f;
        [SerializeField] private float _nodeChance = 0.6f;
        [SerializeField] private float _nodeWidth = 0.5f;
        [Header("Selection")]
        [SerializeField] private int _rectMinX = 1;
        [SerializeField] private int _rectMinY = 1;
        // n.b. don't serialize enums for production - but this is fine just as a sample
        [SerializeField] private RectSelectionPreference[] _rectChoices;
        [SerializeField] private bool _rectDeterministic = true;
        [Header("Colors")]
        [SerializeField] private Color _colorNode = Color.yellow;
        [SerializeField] private Color _colorNodeHighlight = Color.green;
        [SerializeField] private Color _colorRectFound = Color.purple;
        private List<Vector3Int> _nodes = new List<Vector3Int>();
        private Vector3Int _nodeStart = Vector3Int.zero;
        private bool _nodesActive = false;
        private Rect _rectFound = Rect.zero;
        private bool _rectActive = false;
        private Vector2 _gridScale = Vector2.zero;
        private Rect _gridStart = Rect.zero;
        private Vector2 _nodeSize = Vector2.zero;

        private Vector2 GridPosToScreenPos(float x, float y)
        {
            return new Vector2(x * _gridScale.x, y * _gridScale.y);
        }

        void OnGUI()
        {
            if (_rectActive)
            {
                if (_rectActive)
                {
                    Vector2 gridOrigin = GridPosToScreenPos(_rectFound.x - 0.25f, _rectFound.y - 0.25f);
                    Vector2 gridSize = GridPosToScreenPos(_rectFound.width, _rectFound.height);
                    Rect rectDisplay = _gridStart.Add(new Rect(gridOrigin, gridSize));

                    GUI.color = _colorRectFound;
                    GUI.DrawTexture(rectDisplay, Texture2D.whiteTexture);
                }
            }
            if (_nodesActive)
            {
                GUI.color = _colorNode;
                for (int i = 0; i < _nodes.Count; i++)
                {
                    // Using x,y directly as screen-space pixels
                    Rect rect = _gridStart.Add(new Rect(_nodes[i].x * _gridScale.x,
                        _nodes[i].y * _gridScale.y,
                        _nodeSize.x, _nodeSize.y));

                    if (_nodes[i] == _nodeStart)
                    {
                        GUI.color = _colorNodeHighlight;
                        GUI.DrawTexture(rect, Texture2D.whiteTexture);
                        GUI.color = _colorNode;
                    }
                    else
                        GUI.DrawTexture(rect, Texture2D.whiteTexture);
                }
            }
            GUI.color = Color.white;
            if (GUI.Button(_buttonPlaceNodes, "Place nodes"))
            {
                // set up the grid
                if (_gridMaxScreen <= 0 || _gridMaxScreen >= 1f)
                {
                    Debug.Log("Invalid _gridMaxScreen - must be between 0 and 1");
                    return;
                }
                else
                {
                    int nodeCount = 0;
                    _rectActive = false;
                    _nodes.Clear();
                    for (int x = 0; x < _gridWidth; x++)
                    {
                        for (int y = 0; y < _gridheight; y++)
                        {
                            if (Random.value < _nodeChance)
                            {
                                _nodes.Add(new Vector3Int(x, y));
                                nodeCount++;
                            }
                        }
                    }

                    if (_nodes.Count > 0)
                    {
                        _gridStart = new Rect((1f - _gridMaxScreen) * Screen.width * 0.5f, (1f - _gridMaxScreen) * Screen.height * 0.5f, 0, 0);
                        _gridScale = new Vector2(_gridMaxScreen * Screen.width / _gridWidth, _gridMaxScreen * Screen.height / _gridheight);
                        _nodeSize = _gridScale * _nodeWidth;

                        _nodeStart = _nodes[Random.Range(0, _nodes.Count)];
                        _nodesActive = true;
                    }
                    else
                    {
                        Debug.Log("Nodes placed: 0");
                        _nodesActive = false;
                    }
                }
            }
            if (GUI.Button(_buttonRectFind, "Find rect"))
            {
                if (_nodesActive)
                {
                    Rect rectFound;
                    if (CSMathGeometry.BiggestRect(out rectFound, _nodes, _nodeStart, _rectChoices, _rectMinX, _rectMinY, _rectDeterministic))
                    {
                        _rectFound = rectFound;
                        _rectActive = true;
                    }
                    else
                    {
                        _rectActive = false;
                        Debug.Log("Could not find acceptable Rect");
                    }
                }
                else
                    Debug.Log("Place nodes first");
            }
            if (GUI.Button(_buttonRectClear, "Clear"))
            {
                _nodesActive = false;
                _rectActive = false;
            }
        }
    }
}
#endif