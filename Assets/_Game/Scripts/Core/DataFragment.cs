using System;
using UnityEngine;

public class DataFragment : MonoBehaviour, IItem
{
    public static event Action<int> OnFragmentCollect;
    public int worth = 5;
    public void Collect()
    {
        OnFragmentCollect.Invoke(worth);
        Destroy(gameObject);
    }
}
