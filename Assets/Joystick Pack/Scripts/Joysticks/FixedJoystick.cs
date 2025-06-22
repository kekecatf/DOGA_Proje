using UnityEngine;
using UnityEngine.EventSystems;

public class FixedJoystick : Joystick
{
    protected override void Start()
    {
        base.Start();
        // Arka plan her zaman aktif kalsın
        background.gameObject.SetActive(true);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
    }
}
