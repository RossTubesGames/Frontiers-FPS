using UnityEngine;

public class WeaponWheel : MonoBehaviour
{
    [Header("Weapons (Slot 1 = element 0, Slot 2 = element 1, etc.)")]
    [SerializeField] private GameObject[] weapons;

    [SerializeField] private int startSlot = 1; // 1..9

    private int currentIndex = -1;

    private void Start()
    {
        int startIndex = Mathf.Clamp(startSlot, 1, weapons.Length) - 1;
        SwitchTo(startIndex);
    }

    private void Update()
    {
        // Alpha keys (top row)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchTo(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchTo(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchTo(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchTo(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchTo(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SwitchTo(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SwitchTo(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) SwitchTo(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) SwitchTo(8);

        // Optional: Numpad keys too
        if (Input.GetKeyDown(KeyCode.Keypad1)) SwitchTo(0);
        if (Input.GetKeyDown(KeyCode.Keypad2)) SwitchTo(1);
        if (Input.GetKeyDown(KeyCode.Keypad3)) SwitchTo(2);
        if (Input.GetKeyDown(KeyCode.Keypad4)) SwitchTo(3);
        if (Input.GetKeyDown(KeyCode.Keypad5)) SwitchTo(4);
        if (Input.GetKeyDown(KeyCode.Keypad6)) SwitchTo(5);
        if (Input.GetKeyDown(KeyCode.Keypad7)) SwitchTo(6);
        if (Input.GetKeyDown(KeyCode.Keypad8)) SwitchTo(7);
        if (Input.GetKeyDown(KeyCode.Keypad9)) SwitchTo(8);
    }

    private void SwitchTo(int index)
    {
        if (weapons == null || weapons.Length == 0) return;
        if (index < 0 || index >= weapons.Length) return;
        if (index == currentIndex) return;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(i == index);
        }

        currentIndex = index;
    }
}