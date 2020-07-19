using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EventLog : MonoBehaviour
{
    public static List<string> events = new List<string>();

    private static bool shouldScrollToBottom = false;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        GetComponent<Text>().text = String.Join("\n\n", events);

        if (shouldScrollToBottom)
        {
            transform.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 0);
            
            shouldScrollToBottom = false;
        }
    }

    /* PUBLIC API */
    
    public static void LogEvent(string eventStr)
    /* Add a line to the stream of events that occurred in this trial.
    
    :param string eventStr:
    */
    {
        events.Add(eventStr);
        
        // Only keep some number of trailing events, to avoid overflowing things.
        int cutoff = Mathf.Max(0, events.Count - 300);
        events = events.Skip(cutoff).ToList();

        shouldScrollToBottom = true;
    }
}
