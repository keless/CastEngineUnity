using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputEventBridge : MonoBehaviour {

    Dictionary<string, bool> m_eventStates = new Dictionary<string, bool>();

    bool modLeftShift;
    bool modRightShift;
    bool modLeftCtrl;
    bool modRightCtrl;
    bool modLeftAlt;
    bool modRightAlt;

	// Update is called once per frame
	void Update () {

        _checkModifierKeys();

        _checkMouseButton(0);
        _checkMouseButton(1);
        _checkMouseButton(2);

        _checkKey(KeyCode.Tab);
        _checkKey(KeyCode.Space);
        _checkKey(KeyCode.Return);
        _checkKey(KeyCode.LeftShift);
        for (int i= (int)KeyCode.A; i <= (int)KeyCode.Z; i++)
        {
            _checkKey((KeyCode)i);
        }
    }

    void _checkMouseButton(int idx)
    {
        string stateName = "mouse_" + idx;
        bool value = Input.GetMouseButton(idx);
        if (!m_eventStates.ContainsKey(stateName)) m_eventStates.Add(stateName, false);
        if(m_eventStates[stateName] != value)
        {
            m_eventStates[stateName] = value;
            EventBus.ui.dispatch(new MouseEvent(idx, value));
        }
    }

    void _checkKey(KeyCode key)
    {
        string stateName = "key_" + key;
        bool value = Input.GetKey(key);
        if (!m_eventStates.ContainsKey(stateName)) m_eventStates.Add(stateName, false);
        if (m_eventStates[stateName] != value)
        {
            m_eventStates[stateName] = value;
            EventBus.ui.dispatch(new KeyEvent(key, value, modLeftShift || modRightShift, modLeftAlt || modRightAlt, modLeftCtrl || modRightCtrl));
        }
    }

    void _checkModifierKeys()
    {
        modLeftShift = Input.GetKey(KeyCode.LeftShift);
        modRightShift = Input.GetKey(KeyCode.RightShift);
        modLeftCtrl = Input.GetKey(KeyCode.LeftControl);
        modRightCtrl = Input.GetKey(KeyCode.RightControl);
        modLeftAlt = Input.GetKey(KeyCode.LeftAlt);
        modRightAlt = Input.GetKey(KeyCode.RightAlt);
    }
}

public class MouseEvent : EventObject
{
    public const string EvtName = "MouseEvent";

    int _mouseIdx;
    public int mouseIdx { get { return _mouseIdx; } protected set { _mouseIdx = value; } }

    bool _isDown;
    /// <summary>
    /// true if event is due to mouseDown, false if mouseUp
    /// </summary>
    public bool isDown { get { return _isDown; } protected set { _isDown = value; } }

    public MouseEvent(int idx, bool down) : base(EvtName)
    {
        _mouseIdx = idx;
        _isDown = down;
    }
}

public class KeyEvent : EventObject
{
    public const string EvtName = "KeyEvent";

    KeyCode _keyCode;
    public KeyCode keyCode { get { return _keyCode; } protected set { _keyCode = value; } }

    bool _isDown;
    /// <summary>
    /// true if event is due to keyDown, false if keyUp
    /// </summary>
    public bool isDown { get { return _isDown; } protected set { _isDown = value; } }

    bool _shift;
    /// <summary>
    /// true if either left or right shift keys are held down
    /// </summary>
    public bool shift { get { return _shift; } protected set { _shift = value; } }

    bool _alt;
    /// <summary>
    /// true if either left or right alt keys are held down
    /// </summary>
    public bool alt { get { return _alt; } protected set { _alt = value; } }

    bool _ctrl;
    /// <summary>
    /// true if either left or right ctrl keys are held down
    /// </summary>
    public bool ctrl { get { return _ctrl; } protected set { _ctrl = value; } }

    public KeyEvent(KeyCode code, bool down, bool shift = false, bool alt = false, bool ctrl = false) : base(EvtName)
    {
        _keyCode = code;
        _isDown = down;
        _shift = shift;
        _alt = alt;
        _ctrl = ctrl;
    }
}