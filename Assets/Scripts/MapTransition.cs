using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundry;
    CinemachineConfiner confiner;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;

    enum Direction { Up, Down, Left, Right, Teleport };
    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            confiner.m_BoundingShape2D = mapBoundry;
            UpdatePlayerPosition(collision.gameObject);
        }
    }
    void UpdatePlayerPosition(GameObject player)
    {
        if (direction == Direction.Teleport)
        {
            player.transform.position = teleportTargetPosition.position;

            return;
        }

        Vector3 additiivePos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                additiivePos.y += 2;
                break;
            case Direction.Down:
                additiivePos.y += -2;
                break;
            case Direction.Left:
                additiivePos.x += -2;
                break;
            case Direction.Right:
                additiivePos.x += 2;
                break;
        }

        player.transform.position = additiivePos;
    }
}