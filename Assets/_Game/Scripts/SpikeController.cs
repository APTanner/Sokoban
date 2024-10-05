using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpikeController : UnitySingleton<SpikeController>
{
    private Tilemap m_tilemap;
    private Dictionary<Vector3Int, ValueTuple<bool, int>> m_spikeyMap = new();
    private Dictionary<int, List<Vector3Int>> m_spikeButtonMap = new();

    public void Initialize(Tilemap tilemap, Vector3Int position, int ButtonID, bool spikey = false)
    {
        if (!m_spikeyMap.TryAdd(position, (spikey, ButtonID)))
        {
            return;
        }

        if (m_tilemap == null)
        {
            m_tilemap = tilemap;
        }

        if (!m_spikeButtonMap.TryAdd(ButtonID, new() { position }))
        {
            m_spikeButtonMap[ButtonID].Add(position);
        }

        UpdateSpike(position);
    }

    public void UpdateSpike(Vector3Int position)
    {
        m_tilemap.RefreshTile(position);
    }

    public bool IsSpikey(Vector3Int position)
    {
        if (!m_spikeyMap.ContainsKey(position))
        {
            return false;
        }

        var (state, ID) = m_spikeyMap[position];
        Debug.Log(ButtonController.Instance.IsPressed(ID));
        return ButtonController.Instance.IsPressed(ID) ^ state;
    }

    public void OnSpike(int buttonID)
    {
        Debug.Log("On Spike.");

        if (!m_spikeButtonMap.ContainsKey(buttonID))
        {
            return;
        }

        foreach (Vector3Int position in m_spikeButtonMap[buttonID])
        {
            UpdateSpike(position);
        }
    }
}
