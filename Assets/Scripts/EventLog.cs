using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventLog : MonoBehaviour
{
    public TMP_Text EventLogText;

    private List<string> _events;

    private float _lastDeleteTime = 0f;

    void Start()
    {
        _events = new List<string>();
        UpdateEventLogText();
    }

    void Update()
    {
        if (_events.Count > 0 && Time.time - _lastDeleteTime >= 3f) {
            _lastDeleteTime = Time.time;
            _events.RemoveAt(0);
            UpdateEventLogText();
        }
    }

    public void AddEvent(string s) {
        if (_events.Count >= 5) {
            _events.RemoveAt(0);
        }
        _events.Add(s);
        _lastDeleteTime = Time.time;
        UpdateEventLogText();
    }

    public void UpdateEventLogText() {
        string log = "";
        foreach (string s in _events) { 
            log = s + "\n" + log;
        }
        EventLogText.text = log;    
    }
}
