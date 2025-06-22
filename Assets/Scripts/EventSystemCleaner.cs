using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemCleaner : MonoBehaviour
{
    void Awake()
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>(true); // T�m EventSystem'leri al
        bool oneEnabled = false;

        foreach (EventSystem es in systems)
        {
            if (!oneEnabled)
            {
                es.gameObject.SetActive(true);
                oneEnabled = true;
            }
            else
            {
                es.gameObject.SetActive(false);
            }
        }
    }
}
