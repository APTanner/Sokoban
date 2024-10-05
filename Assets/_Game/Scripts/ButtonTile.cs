using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/Button Tile")]
public class ButtonTile : Tile
{
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private int ButtonID;

    [SerializeField] private TileBase[] validTiles;

    private HashSet<TileBase> m_validTileSet;

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (!ButtonController.IsAvailable)
        {
            return false;
        }
        m_validTileSet = validTiles.ToHashSet();
        ButtonController.Instance.Initialize(tilemap.GetComponent<Tilemap>(), position, ButtonID);
        return true;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        tileData.sprite =
            ButtonController.IsAvailable &&
            ButtonController.Instance.IsPressed(position) ?
            pressedSprite :
            defaultSprite;
    }

    public void OnObjectOver(TileBase tile, Vector3Int position)
    {
        bool pressed = m_validTileSet.Contains(tile);
        ButtonController.Instance.UpdateButton(position, pressed);
        SpikeController.Instance.OnSpike(ButtonID);
    }
}