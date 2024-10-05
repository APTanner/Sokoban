using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ButtonController : UnitySingleton<ButtonController>
{
    public Tilemap m_tilemap;
    private Dictionary<Vector3Int, ValueTuple<bool, int>> m_pressedMap = new();
    private Dictionary<int, int> m_buttonIDPressedMap = new();

    public void Initialize(Tilemap tilemap, Vector3Int position, int ButtonID)
    {
        if (!m_pressedMap.TryAdd(position, (false, ButtonID)))
        {
            return;
        }

        if (m_tilemap == null)
        {
            m_tilemap = tilemap;
        }

        m_buttonIDPressedMap.TryAdd(ButtonID, 0);
    }

    public void UpdateButton(Vector3Int position, bool isPressed)
    {
        var (state, id) = m_pressedMap[position];
        m_pressedMap[position] = (isPressed, id);
        m_buttonIDPressedMap[id] += isPressed ? 1 : -1;
        m_tilemap.RefreshTile(position);
    }

    public bool IsPressed(Vector3Int position)
    {
        if (!m_pressedMap.ContainsKey(position))
        {
            return false;
        }

        return m_pressedMap[position].Item1;
    }

    public bool IsPressed(int buttonID)
    {
        if (!m_buttonIDPressedMap.ContainsKey(buttonID))
        {
            return false;
        }

        return m_buttonIDPressedMap[buttonID] > 0;
    }
}
