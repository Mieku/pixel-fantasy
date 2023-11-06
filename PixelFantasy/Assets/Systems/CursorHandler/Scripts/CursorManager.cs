using System;
using Managers;
using UnityEngine;

namespace Systems.CursorHandler.Scripts
{
    public enum ECursorState
    {
        Default,
        AreaSelect,
    }
    
    public class CursorManager : Singleton<CursorManager>
    {
        [SerializeField] private Texture2D _defaultCursor;
        [SerializeField] private Texture2D _areaSelectCursor;

        private ECursorState _cursorState;

        public void ChangeCursorState(ECursorState cursorState)
        {
            switch (cursorState)
            {
                case ECursorState.Default:
                    ChangeCursor(_defaultCursor);
                    break;
                case ECursorState.AreaSelect:
                    ChangeCursor(_areaSelectCursor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cursorState), cursorState, null);
            }
        }

        private void ChangeCursor(Texture2D cursorTexture)
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
    }
}
