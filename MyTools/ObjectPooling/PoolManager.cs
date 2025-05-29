using System;
using System.Collections;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] internal ObjectsToPool[] PoolList;

    private IEnumerator Start()
    {
        yield return CreatePool();
        yield return AutoSpawn();
    }

    IEnumerator CreatePool()
    {
        foreach (var pool in PoolList)
        {
            yield return pool.pool.CreatePool(transform);
        }
    }

    IEnumerator AutoSpawn()
    {
        for (int i = 0; i < PoolList.Length; i++)
        {
            int index = i;
            var pool = PoolList[index];
            if (pool.Unlokced)
            {
                var returned = pool.pool.AutoSpawn();
                for (int j = 0; j < returned.Length; j++)
                {
                    var temp = returned[j];
                    temp.Item2.transform.position = temp.Item2.transform.Reposition(Directioons.Top, new Vector3(0, -2, 0));
                    if (temp.Item2.TryGetComponent(out DragControllerNew controller))
                    {
                        controller.StringKey = temp.Item1;
                        controller.BtnID = index;
                    }
                    yield return new WaitForSeconds(0.2f);
                    returned[j].Item2.SetActive(true);
                }
            }
        }
    }

    internal void ReturnObj(GameObject obj)
    {
        if (obj && obj.TryGetComponent(out DragControllerNew dragController))
        {
       
        }
        else
        {
            Debug.LogError("Object does not have DragController component or is null.");
            return;
        }
        foreach (var pool in PoolList)
        {
            if (pool.Unlokced & pool.pool.Key.Equals(dragController.StringKey))
            {
                pool.pool.ReturnObject(dragController.StringKey, obj);
            }
        }
    }
}

[System.Serializable]
public class ObjectsToPool
{
    [SerializeField] internal bool Unlokced = false;
    [SerializeField] internal LevelTheme Theme = LevelTheme.None;
    public ObjectPool pool = default;
}
