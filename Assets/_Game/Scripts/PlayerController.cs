using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerController : UnitySingleton<PlayerController>
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap collidableTileMap;
    [SerializeField] private Tilemap groundTileMap;

    [SerializeField] private TileBase[] pushables;
    [SerializeField] private TileBase[] impassables;

    private List<WinTile> m_winTiles = new List<WinTile>();

    private HashSet<TileBase> pushableSet;
    private HashSet<TileBase> impassableSet;

    private Animator m_animator;
    private PlayerActionAsset m_inputActions;

    private SpriteRenderer m_movingObject;
    private TileBase m_movingTile;

    private Vector3Int m_gridPosition;
    private Vector3Int m_currentMove;
    private bool m_bIsMoving = false;

    public void CheckWin()
    {
        if (WinController.Instance.Won && SceneManager.GetActiveScene().buildIndex != 0)
        {
            StartCoroutine(Win());
        }
    }

    private IEnumerator Win()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }

    public void RegisterWinTile(WinTile tile)
    {
        m_winTiles.Add(tile);
    }

    protected override void Awake()
    {
        base.Awake();

        m_inputActions = new PlayerActionAsset();
        m_inputActions.Enable();

        m_animator = GetComponent<Animator>();
        if (m_animator == null)
        {
            Debug.LogError("Player gameobject did not have an animator attached");
        }

        m_gridPosition = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(m_gridPosition);

        impassableSet = impassables.ToHashSet();
        pushableSet = pushables.ToHashSet();

        GameObject gameObject = new GameObject("Moving object");
        m_movingObject = gameObject.AddComponent<SpriteRenderer>();
        m_movingObject.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (m_bIsMoving)
        {
            return;
        }

        Vector2 input = m_inputActions.Player.Move.ReadValue<Vector2>();
        int inputX = Mathf.RoundToInt(input.x);
        int inputY = Mathf.RoundToInt(input.y);

        if (input.x != 0 || input.y != 0)
        {
            m_currentMove = new Vector3Int(inputX, inputX == 0 ? inputY : 0, 0);
            Vector3Int newGridPosition = m_gridPosition + m_currentMove;

            m_animator.SetFloat("Horizontal", m_currentMove.x);
            m_animator.SetFloat("Vertical", m_currentMove.y);

            bool bIsSpike = IsSpike(newGridPosition);
            if (bIsSpike)
            {
                return;
            }

            TileBase tile = collidableTileMap.GetTile(newGridPosition);
            if (tile != null)
            {
                bool bIsImpassableTile = impassableSet.Contains(tile);
                bool bIsStuckPushable = pushableSet.Contains(tile) && !TryPush(tile);
                if (bIsImpassableTile || bIsStuckPushable)
                {
                    return;
                }
            }

            m_bIsMoving = true;
            m_animator.SetBool("bIsMoving", m_bIsMoving);
        }

    }

    private bool IsSpike(Vector3Int newGridPosition)
    {
        TileBase potentialSpike = groundTileMap.GetTile(newGridPosition);
        if (potentialSpike is SpikeTile spikeTile)
        {
            return SpikeController.Instance.IsSpikey(newGridPosition);
        }
        return false;
    }

    private bool TryPush(TileBase tile)
    {
        Vector3Int gridPosition = m_gridPosition + m_currentMove;
        Vector3Int destinationGridPosition = gridPosition + m_currentMove;

        TileBase blockingTile = collidableTileMap.GetTile(destinationGridPosition);

        if (blockingTile != null && impassableSet.Contains(blockingTile) || pushableSet.Contains(blockingTile))
        {
            return false;
        }

        CheckSpecialTilesExit(gridPosition);
        CheckSpecialTilesEnter(destinationGridPosition, gridPosition);
        SetUpMovingObject(gridPosition);

        return true;
    }

    private void CheckSpecialTilesExit(Vector3Int position)
    {
        TileBase currentTile = groundTileMap.GetTile(position);
        if (currentTile is ButtonTile buttonTile)
        {
            buttonTile.OnObjectOver(null, position);
        }
        if (currentTile is WinTile winTile)
        {
            winTile.OnObjectOver(null, position);
        }
    }

    private void CheckSpecialTilesEnter(Vector3Int destination, Vector3Int current, bool preMove = true)
    {
        TileBase currentTile = groundTileMap.GetTile(destination);
        TileBase movingTile = collidableTileMap.GetTile(current);
        if (preMove && currentTile is ButtonTile buttonTile)
        {
            buttonTile.OnObjectOver(movingTile, destination);
        }
        if (!preMove && currentTile is WinTile winTile)
        {
            winTile.OnObjectOver(movingTile, destination);
        }
    }

    private void SetUpMovingObject(Vector3Int gridPosition)
    {
        m_movingObject.gameObject.SetActive(true);
        m_movingTile = collidableTileMap.GetTile(gridPosition);
        m_movingObject.sprite = collidableTileMap.GetSprite(gridPosition);
        m_movingObject.transform.position = grid.GetCellCenterWorld(gridPosition);
        collidableTileMap.SetTile(gridPosition, null);
    }

    private void FixedUpdate()
    {
        if (!SceneManager.GetActiveScene().isLoaded || !m_bIsMoving)
        {
            return;
        }

        Vector3Int destinationCellCoordinates = m_gridPosition + m_currentMove;
        Vector3 destinationCellPosition = grid.GetCellCenterWorld(destinationCellCoordinates);

        Vector3Int pushCoordinates = destinationCellCoordinates + m_currentMove;

        float movement = 2 * Time.fixedDeltaTime;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destinationCellPosition,
            movement);

        if (m_movingObject.gameObject.activeSelf)
        {
            Vector3 pushPosition = grid.GetCellCenterWorld(pushCoordinates);
            m_movingObject.transform.position = Vector3.MoveTowards(
                m_movingObject.transform.position,
                pushPosition,
                movement);
        }

        if (Vector3.SqrMagnitude(destinationCellPosition - transform.position) < 0.001f)
        {
            transform.position = destinationCellPosition;
            m_gridPosition = destinationCellCoordinates;
            m_bIsMoving = false;
            m_animator.SetBool("bIsMoving", m_bIsMoving);

            if (m_movingObject.gameObject.activeSelf)
            {
                collidableTileMap.SetTile(pushCoordinates, m_movingTile);
                m_movingObject.gameObject.SetActive(false);
                CheckSpecialTilesEnter(pushCoordinates, pushCoordinates, false);
            }
        }
    }
}
