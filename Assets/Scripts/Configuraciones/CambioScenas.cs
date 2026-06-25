using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioScenas : MonoBehaviour
{
    public static CambioScenas Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Login()
    {
        SceneManager.LoadScene("Login"); //cargar la esena
    }

    public void MenuPrincipal()
    {
        SceneManager.LoadScene("MenuPrincipal"); //cargar la esena
    }


    public void Salir()
    {
        Application.Quit(); // Salida de la soft
        Debug.Log("Hasta la proxima");
    }
}
