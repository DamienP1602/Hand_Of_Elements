using UnityEngine;

public class PlayerClickComponent : MonoBehaviour
{
    Ray PointOnScreen => Camera.main.ScreenPointToRay(Input.mousePosition);
    
    public void OnPlayerClick()
    {
        if (Physics.Raycast(PointOnScreen,out RaycastHit _hit,15.0f))
        {

        }
    }
}
