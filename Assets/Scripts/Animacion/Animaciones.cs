using UnityEngine;
using TMPro;

public class Animaciones : MonoBehaviour
{
    [SerializeField] private Animator scrollViewAnimator;

    private bool isVisible = true;

    private void Awake()
    {
        // Estado inicial forzado
        scrollViewAnimator.SetBool("isOpen", isVisible);    
    }

    // ================= BOTON DEL SCROLL =================
    public void ToggleScrollView()
    {
        isVisible = !isVisible;
        AplicarEstado();
    }

    // ================= BOTONES DEL MENU =================
    public void SiempreFalso()
    {
        scrollViewAnimator.SetBool("isOpen", false);
        isVisible = false;
    }

    // ================= METODO CENTRAL =================
    private void AplicarEstado()
    {
        scrollViewAnimator.SetBool("isOpen", isVisible);
    }
}