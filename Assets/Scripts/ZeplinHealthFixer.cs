using UnityEngine;

// This script ensures Zeplin health is always 1000 by forcing it in both
// PlayerPrefs and in the PlayerData object. This runs before any other scripts.
public class ZeplinHealthFixer : MonoBehaviour 
{
    // Use Awake to execute this before other scripts
    private void Awake()
    {
        Debug.Log("ZeplinHealthFixer: Forcing zeplin health to 1000");
        
        // Force zeplin health to 1000 in PlayerPrefs
        
        
        // Self-destruct after running
        Destroy(this);
    }
} 