using UnityEngine;
using UnityEngine.UI;
//using Unity.Netcode;

/// <summary>
/// Handles updating the health bar UI above a unit, synced via NetworkVariable.
/// </summary>
public class UnitHealthBar : MonoBehaviour
{
    public Image fillImage;
    //private NetworkUnitBase unit;

    void Awake()
    {
        //unit = GetComponentInParent<NetworkUnitBase>();
    }

    void Start()
    {
        /* if (unit != null)
        {
            // Set the initial fill color to match the player's color
            if (fillImage != null)
                fillImage.color = unit.outlineColor;

            if (unit.NetworkHP != null)
            {
                unit.NetworkHP.OnValueChanged += OnHPChanged;
                UpdateBar(unit.NetworkHP.Value, unit.maxHP);
            }
            
            // Subscribe to outline color changes
            unit.OnOutlineColorChanged += OnOutlineColorChanged;
        } */
    }

    void OnDestroy()
    {
       /*  if (unit != null)
        {
            if (unit.NetworkHP != null)
                unit.NetworkHP.OnValueChanged -= OnHPChanged;
            
            // Unsubscribe from outline color changes
            unit.OnOutlineColorChanged -= OnOutlineColorChanged;
        } */
    }

    void OnHPChanged(int oldValue, int newValue)
    {
        //UpdateBar(newValue, unit.maxHP);
    }

    void OnOutlineColorChanged(Color newColor)
    {
        if (fillImage != null)
            fillImage.color = newColor;
    }

    /// <summary>
    /// Manually update the health bar color (useful for testing or edge cases)
    /// </summary>
    public void UpdateHealthBarColor(Color newColor)
    {
        if (fillImage != null)
            fillImage.color = newColor;
    }

    void UpdateBar(int current, int max)
    {
        if (fillImage != null)
            fillImage.fillAmount = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
    }
}