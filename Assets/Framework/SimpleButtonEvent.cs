using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleButtonEvent : MonoBehaviour {

    string _btnName;

	// Use this for initialization
	void Start () {
        Button btn = GetComponent<Button>();
        Debug.Assert(btn != null);
        _btnName = btn.name;
        btn.onClick.AddListener(onClick);
	}

    void OnDestroy()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveListener(onClick);
    }
	
    void onClick()
    {
        EventBus.ui.dispatch(new EventObject(_btnName));
    }
}

