using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/Spike Tile")]
public class SpikeTile : Tile
{
    [SerializeField] private Sprite spikeSprite;
    [SerializeField] private Sprite downSprite;

    public bool IsInitiallySpikey;
    public int ButtonID;

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (!SpikeController.IsAvailable)
        {
            return false;
        }

        SpikeController.Instance.Initialize(
            tilemap.GetComponent<Tilemap>(),
            position,
            ButtonID,
            IsInitiallySpikey);

        return true;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        tileData.sprite =
            SpikeController.IsAvailable &&
            !SpikeController.Instance.IsSpikey(position) ?
            downSprite :
            spikeSprite;
    }
}
