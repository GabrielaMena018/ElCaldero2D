using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TarjetaTiendaUI : MonoBehaviour
{
    [SerializeField] private Image imagenIngrediente;
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private TMP_Text textoPrecio;
    [SerializeField] private Button botonComprar;

    public void Configurar(string nombre, int precio, Sprite sprite, UnityEngine.Events.UnityAction accion)
    {
        if (imagenIngrediente != null)
        {
            imagenIngrediente.sprite = sprite;
            imagenIngrediente.preserveAspect = true;
            imagenIngrediente.enabled = sprite != null;
        }

        if (textoNombre != null)
        {
            textoNombre.text = nombre;
            textoNombre.fontSize = 24f;
            textoNombre.fontStyle = FontStyles.Bold;
        }

        if (textoPrecio != null)
        {
            textoPrecio.text = precio.ToString();
            textoPrecio.fontSize = 22f;
            textoPrecio.fontStyle = FontStyles.Bold;
        }

        if (botonComprar != null)
        {
            botonComprar.onClick.RemoveAllListeners();
            if (accion != null)
                botonComprar.onClick.AddListener(accion);
        }
    }
}



