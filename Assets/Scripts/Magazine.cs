using UnityEngine;

public class Magazine : MonoBehaviour
{
    public enum MagType { Pistol, Rifle }
    public MagType type;

    public int ammoCount = 15;
    public int maxAmmoCount = 15;

    public bool HasAmmo()
    {
        return ammoCount > 0;
    }

    public void UseAmmo()
    {
        if (ammoCount > 0)
            ammoCount--;
    }
}
