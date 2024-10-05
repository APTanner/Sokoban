using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WinController : UnitySingleton<WinController>
{
    private Dictionary<Vector3Int, bool> m_winMap = new();
    private Tilemap m_tilemap;

    public bool Won => m_winMap.All(x => x.Value);

    public void Initialize(Vector3Int position, Tilemap tilemap)
    {
        if (!m_winMap.TryAdd(position, false))
        {
            return;
        }

        if (m_tilemap == null)
        {
            m_tilemap = tilemap;
        }
    }

    public void UpdateWin(Vector3Int position, bool isWinning, ParticleSystem ps = null)
    {
        m_winMap[position] = isWinning;
        Debug.Log("Updating win");
        if (isWinning && ps != null)
        {
            Debug.Log("Playing system");
            ParticleSystem system = Instantiate(ps, m_tilemap.GetCellCenterWorld(position), Quaternion.identity);
            StartCoroutine(DestroyAfterPlay(system));
        }
    }

    IEnumerator DestroyAfterPlay(ParticleSystem ps)
    {
        ps.Play();

        yield return new WaitForSeconds(ps.main.duration);

        ps.Stop();

        Destroy(ps.gameObject);
    }
}

