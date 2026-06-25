using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [Header("Paneles de contenido")]
    public GameObject[] paneles;

    [Header("Post-Login")]
    public GameObject scrollView;

    private Animaciones anim;

    private void Awake()
    {
        if (anim == null)
        {
            anim = FindFirstObjectByType<Animaciones>();
        }
    }

    private void Start()
    {
        MostrarMenuPostLogin();
    }

    /// <summary>
    /// Activa solo el panel del menú principal y el ScrollView.
    /// Desactiva todos los demás paneles.
    /// </summary>
    public void MostrarMenuPostLogin()
    {
        Ocultar();
        paneles[0].SetActive(true);

        if (scrollView != null)
            scrollView.SetActive(true);
    }

    private void Ocultar()
    {
        for (int i = 0; i < paneles.Length; i++)
        {
            paneles[i].SetActive(false);
        }
    }

    public void MostrarPanel(int index)
    {
        Ocultar();
        paneles[index].SetActive(true);
        if (anim != null)
        {
            anim.SiempreFalso();
        }
        else
        {
            Debug.LogWarning("Animaciones no asignado en MenuUI");
        }
    }
}
