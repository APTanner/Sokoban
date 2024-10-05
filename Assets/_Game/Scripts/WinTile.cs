using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/Win Tile")]
public class WinTile : Tile
{
    [SerializeField] private TileBase[] validTiles;
    [SerializeField] private ParticleSystem particleSystem;

    private HashSet<TileBase> m_validTileSet;

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (!WinController.IsAvailable)
        {
            return false;
        }
        m_validTileSet = validTiles.ToHashSet();
        WinController.Instance.Initialize(position, tilemap.GetComponent<Tilemap>());
        return true;
    }

    public void OnObjectOver(TileBase tile, Vector3Int position)
    {
        WinController.Instance.UpdateWin(position, m_validTileSet.Contains(tile), particleSystem);
        PlayerController.Instance.CheckWin();
    }
}
