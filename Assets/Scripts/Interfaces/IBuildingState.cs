using UnityEngine;

public interface IBuildingState
{
    void EndState();
    GridData OnAction(Vector3Int gridPosition);
    void UpdateState(Vector3Int gridPosition);
    void UpdateState(Vector3Int gridPosition, int rotation);
}