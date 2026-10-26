using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class IngredienteUI : MonoBehaviour
{
    [Header("Datos")]
    [SerializeField] private string nombreIngrediente;
    [SerializeField] private string categoria = "Plantas";
    [SerializeField] private int cantidad;

    [Header("Referencias visuales")]
    [SerializeField] private Image imagenIngrediente;
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private TMP_Text textoCantidad;
    [SerializeField] private Button boton;
    [SerializeField] private SeleccionCalderoUI seleccionCalderoUI;

    public string NombreIngrediente => nombreIngrediente;
    public string Categoria => categoria;
    public int Cantidad => cantidad;

    public void Configurar(string nombre, string nuevaCategoria, int nuevaCantidad, Sprite sprite)
    {
        nombreIngrediente = nombre;
        categoria = string.IsNullOrWhiteSpace(nuevaCategoria) ? "Otros" : nuevaCategoria;
        cantidad = Mathf.Max(0, nuevaCantidad);

        if (imagenIngrediente != null)
        {
            imagenIngrediente.sprite = sprite;
            imagenIngrediente.preserveAspect = true;
            imagenIngrediente.enabled = sprite != null;
        }

        ActualizarTextos();
    }

    public void ActualizarCantidad(int nuevaCantidad)
    {
        cantidad = Mathf.Max(0, nuevaCantidad);
        ActualizarTextos();
    }

    public void AsignarSprite(Sprite sprite)
    {
        if (imagenIngrediente == null)
            return;

        imagenIngrediente.sprite = sprite;
        imagenIngrediente.preserveAspect = true;
        imagenIngrediente.enabled = sprite != null;
    }

    public void AsignarAccion(UnityEngine.Events.UnityAction accion)
    {
        if (boton == null)
            return;

        boton.onClick.RemoveAllListeners();
        if (accion != null)
            boton.onClick.AddListener(accion);
    }

    private void Awake()
    {
        if (boton != null)
            boton.onClick.AddListener(AlSeleccionar);
        ActualizarTextos();
    }

    public void AlSeleccionar()
    {
        if (seleccionCalderoUI != null)
            seleccionCalderoUI.SeleccionarIngrediente(nombreIngrediente);
    }

    private void ActualizarTextos()
    {
        if (textoNombre != null)
            textoNombre.text = nombreIngrediente;

        if (textoCantidad != null)
            textoCantidad.text = cantidad + " unidades";
    }
}

