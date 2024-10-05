using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class IdlePusher : MonoBehaviour
{
    public Vector2Int VerticalBounds;
    public Vector2Int HorizontalBounds;

    public Grid grid;
    public Tilemap box;
    public Tilemap ground;

    private Vector3Int m_gridPosition;
    private bool m_bIsMoving = false;

    private Vector3Int m_currentMove;

    // Hardcoded because that's how it is
    public Vector3Int m_boxPosition;
    public Vector3Int m_targetBoxPosition;
    public Vector3Int m_targetPosition;

    private SpriteRenderer m_movingObject;
    [SerializeField] private Tile m_box;

    [SerializeField] private Tile m_grassTile;
    [SerializeField] private Tile m_winTile;

    private Animator m_animator;

    private void Awake()
    {
        m_animator = this.gameObject.GetComponent<Animator>();
        if (m_animator == null)
        {
            Debug.LogError("Player gameobject did not have an animator attached");
        }

        m_gridPosition = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(m_gridPosition);

        GameObject gameObject = new GameObject("Moving object");
        m_movingObject = gameObject.AddComponent<SpriteRenderer>();
        m_movingObject.gameObject.SetActive(false);

        box.SetTile(m_boxPosition, m_box);
        ground.SetTile(m_targetBoxPosition, m_winTile);
    }

    private void Update()
    {
        if (m_bIsMoving)
        {
            return;
        }

        if (m_gridPosition == m_targetPosition)
        {
            PickNewPosition();
        }

        Vector3Int prevMove = m_currentMove;

        int dX = m_targetPosition.x.CompareTo(m_gridPosition.x);
        int dY = m_targetPosition.y.CompareTo(m_gridPosition.y);

        bool bFavorX = (dX != 0) && (dY == 0 || RandomBool());

        m_currentMove = new Vector3Int(
            bFavorX ? dX : 0,
            bFavorX ? 0 : dY
            );


        bool bPushingBox = m_boxPosition == m_currentMove + m_gridPosition;
        bool bRightDirection =
            (m_targetBoxPosition - (m_boxPosition + m_currentMove)).sqrMagnitude
            <= (m_targetBoxPosition - m_boxPosition).sqrMagnitude;
        // Make sure we don't push the box in the wrong direction
        if (bPushingBox && !bRightDirection)
        {
            // do another move
            Debug.Log("Didn't want to push the box in the wrong direction");
            m_currentMove = new Vector3Int(
                bFavorX ? 0 :
                    m_targetPosition.x - m_gridPosition.x > 0 ? 1 : -1,
                !bFavorX ? 0 :
                    m_targetPosition.y - m_gridPosition.y > 0 ? 1 : -1
                );


        }

        // if we're just moving in the opposite direction as we were just going
        if (m_currentMove == -prevMove)
        {
            Debug.Log("Don't want to repeat the same move over and over");
            m_currentMove = new Vector3Int(
                !bFavorX ? m_targetPosition.x - m_gridPosition.x > 0 ? 1 : -1 : 0,
                !bFavorX ? 0 : m_targetPosition.y - m_gridPosition.y > 0 ? 1 : -1);
        }

        TileBase tile = box.GetTile(m_currentMove + m_gridPosition);
        if (tile != null)
        {
            TryPush(tile);
        }

        m_animator.SetFloat("Horizontal", m_currentMove.x);
        m_animator.SetFloat("Vertical", m_currentMove.y);

        m_bIsMoving = true;
        m_animator.SetBool("bIsMoving", m_bIsMoving);
    }

    private void PickNewPosition()
    {
        if (m_boxPosition == m_targetBoxPosition)
        {
            ground.SetTile(m_targetBoxPosition, m_grassTile);

            Debug.Log("Changed box position");
            while (m_boxPosition == m_targetBoxPosition)
            {
                m_targetBoxPosition = GetRandomPosition();
            }

            ground.SetTile(m_targetBoxPosition, m_winTile);

            // 25% shot to just randomly wander
            bool bShouldWander = RandomBool() && RandomBool();
            if (bShouldWander)
            {
                do
                {
                    m_targetPosition = GetRandomPosition();
                }
                while (m_targetPosition == m_boxPosition);
                return;
            }
        }

        if (IsNextToBox())
        {
            Debug.Log("Next to box");
            Vector3Int dir = m_boxPosition - m_gridPosition;
            Vector3Int dBox = m_targetBoxPosition - m_boxPosition;
            Vector3Int moveAmount = dir * dBox;
            int move = Mathf.Max(moveAmount.x, moveAmount.y);
            int moveSign = dir.x.CompareTo(0) + dir.y.CompareTo(0);
            // If they are in opposite directions move will be 0 because the other component
            // will be negative. If they are in the same direction and there is a required movement
            // then it will be a positive value
            if (move > 0)
            {
                m_targetPosition = m_gridPosition + moveAmount * moveSign;
                return;
            }
        }

        int dX = m_targetBoxPosition.x.CompareTo(m_boxPosition.x);
        int dY = m_targetBoxPosition.y.CompareTo(m_boxPosition.y);

        bool bFavorX = (dX != 0) && (dY == 0 || RandomBool());
        Debug.Log("Travelling to next to box");
        m_targetPosition = m_boxPosition - new Vector3Int(
            bFavorX ? dX : 0,
            bFavorX ? 0 : dY);
    }

    private bool IsNextToBox()
    {
        foreach (bool isX in new bool[] { true, false })
        {
            foreach (int d in new int[] { -1, 1 })
            {
                Vector3Int potentialSpot = m_gridPosition + new Vector3Int(
                    isX ? d : 0,
                    isX ? 0 : d);

                if (potentialSpot == m_boxPosition)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Vector3Int GetRandomPosition()
    {
        return new Vector3Int(
            Random.Range(HorizontalBounds.x+1, HorizontalBounds.y-1),
            Random.Range(VerticalBounds.x+1, VerticalBounds.y-1));
    }

    private bool TryPush(TileBase tile)
    {
        Vector3Int gridPosition = m_gridPosition + m_currentMove;

        SetUpMovingObject(gridPosition);
        m_boxPosition += m_currentMove;

        return true;
    }

    private void SetUpMovingObject(Vector3Int gridPosition)
    {
        m_movingObject.gameObject.SetActive(true);
        m_movingObject.sprite = box.GetSprite(gridPosition);
        m_movingObject.transform.position = grid.GetCellCenterWorld(gridPosition);
        box.SetTile(gridPosition, null);
    }

    private void FixedUpdate()
    {
        // It's possible that OnObjectOver() is called in the fixed update loop while
        // the next scene is loading. This causes the `PlayerController` singleton to be instantiated before the
        // version in the scene can load, which will overwrite the correct version
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
                box.SetTile(pushCoordinates, m_box);
                m_movingObject.gameObject.SetActive(false);

                TileBase potentialWin = ground.GetTile(pushCoordinates);
                if (potentialWin is WinTile winTile)
                {
                    winTile.OnObjectOver(m_box, pushCoordinates);
                }
            }
        }
    }

    private bool RandomBool()
    {
        return Random.Range(0, 2) == 1;
    }
}
