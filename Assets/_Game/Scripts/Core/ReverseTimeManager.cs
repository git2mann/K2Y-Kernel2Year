using UnityEngine;
using System.Collections.Generic;

public class ReverseTimeManager : MonoBehaviour
{
    // Singleton - only one can exist
    public static ReverseTimeManager Instance { get; private set; }
    
    [Header("Reverse Time Settings")]
    public bool reverseTimeActive = true;
    public float reverseTimeMultiplier = 1f;
    
    // List of all objects moving in reverse time
    private List<IReverseTimeObject> registeredObjects = new List<IReverseTimeObject>();
    
    void Awake()
    {
        // Make sure only one exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ReverseTimeManager created successfully!");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        // Update all reverse-time objects every frame
        if (reverseTimeActive)
        {
            UpdateAllReverseObjects();
        }
    }
    
    private void UpdateAllReverseObjects()
    {
        // Go through all registered objects
        for (int i = registeredObjects.Count - 1; i >= 0; i--)
        {
            if (registeredObjects[i] != null)
            {
                registeredObjects[i].OnReverseTimeUpdate(reverseTimeMultiplier);
            }
            else
            {
                // Remove objects that no longer exist
                registeredObjects.RemoveAt(i);
            }
        }
    }
    
    // Add an object to reverse time
    public void RegisterObject(IReverseTimeObject obj)
    {
        if (!registeredObjects.Contains(obj))
        {
            registeredObjects.Add(obj);
            Debug.Log("Registered reverse time object: " + obj);
        }
    }
    
    // Remove an object from reverse time
    public void UnregisterObject(IReverseTimeObject obj)
    {
        registeredObjects.Remove(obj);
        Debug.Log("Unregistered reverse time object: " + obj);
    }
    
    // Turn reverse time on/off
    public void SetReverseTimeActive(bool active)
    {
        reverseTimeActive = active;
        Debug.Log("Reverse time active: " + active);
    }
}

// Interface that all reverse-time objects must use
public interface IReverseTimeObject
{
    void OnReverseTimeUpdate(float deltaTime);
}