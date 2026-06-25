using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class CambiarTamañoFuentes : EditorWindow
{
    [MenuItem("Herramientas/Achicar Todos los Textos (Escena y Prefabs)")]
    public static void AchicarTextosCompleto()
    {
        float restaFija = 5f; // Bajar 5 puntos fijos. Ej: 40 -> 35, 32 -> 27.
        int contadorComponentes = 0;

        // ==========================================
        // 1. MODIFICAR EN LA ESCENA ACTIVA
        // ==========================================
        GameObject[] objetosRaiz = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject root in objetosRaiz)
        {
            // Registrar para Undo antes de modificar
            TMP_Text[] textosTMP = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (var txt in textosTMP)
                Undo.RecordObject(txt, "Escalar TMP Text");

            Text[] textosViejos = root.GetComponentsInChildren<Text>(true);
            foreach (var txt in textosViejos)
                Undo.RecordObject(txt, "Escalar Legacy Text");

            contadorComponentes += EscalarTextosEnObjeto(root, restaFija);
        }

        if (contadorComponentes > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        // ==========================================
        // 2. MODIFICAR EN LOS PREFABS DEL PROYECTO
        // ==========================================
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int contadorPrefabs = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // ✅ API correcta para Unity 2018.3+: edita el prefab en modo aislado y guarda automáticamente
            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                GameObject root = scope.prefabContentsRoot;
                int modificados = EscalarTextosEnObjeto(root, restaFija);

                if (modificados > 0)
                {
                    contadorPrefabs++;
                    contadorComponentes += modificados;
                    // No hace falta SetDirty ni SaveAssets: el using scope lo hace al cerrarse
                }
            }
        }

        Debug.Log($"<color=green>¡Proceso Terminado!</color> Se bajaron {contadorComponentes} componentes de texto ({contadorPrefabs} Prefabs modificados + Escena activa). Resta aplicada: -{restaFija} puntos");
    }

    private static int EscalarTextosEnObjeto(GameObject obj, float resta)
    {
        int modificados = 0;

        // ---- TMP_Text (cubre TextMeshProUGUI, TMP_InputField placeholder/text, TMP_Dropdown, etc.) ----
        TMP_Text[] textosTMP = obj.GetComponentsInChildren<TMP_Text>(true);
        foreach (var txt in textosTMP)
        {
            txt.fontSize -= resta;
            modificados++;
        }

        // ---- TMP_InputField: también ajustar el pointSize del componente padre ----
        TMP_InputField[] inputs = obj.GetComponentsInChildren<TMP_InputField>(true);
        foreach (var input in inputs)
        {
            input.pointSize -= resta;
            modificados++;
        }

        // ---- Texto Legacy (por las dudas) ----
        Text[] textosViejos = obj.GetComponentsInChildren<Text>(true);
        foreach (var txt in textosViejos)
        {
            txt.fontSize = Mathf.RoundToInt(txt.fontSize - resta);
            modificados++;
        }

        return modificados;
    }
}
