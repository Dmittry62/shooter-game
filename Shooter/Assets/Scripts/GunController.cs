using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour
{
	public Transform weaponHold;
	public Gun startingGun;
	Gun mGun;

	void Start ()
	{
		if (startingGun)
			EquipGun (startingGun);
	}

	public void EquipGun (Gun gun)
	{
		if (mGun)
			Destroy (mGun.gameObject);
		
		mGun = Instantiate (gun);
		mGun.transform.SetParent(weaponHold, false);
	}

	public void Shoot ()
	{
		if (mGun)
			mGun.Shoot ();
	}
}
