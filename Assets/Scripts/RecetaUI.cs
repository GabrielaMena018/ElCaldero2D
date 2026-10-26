using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RecetaUI : MonoBehaviour
{
    [SerializeField] private Image imagenPocion;
    [SerializeField] private Sprite spriteBloqueado;
    [SerializeField] private GameObject bloqueo;
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private TMP_Text textoIngredientes;
    [SerializeField] private Button boton;
    private Sprite spriteDesbloqueado;

    public Image ImagenPocion => imagenPocion;

    private void Awake()
    {
        if (spriteDesbloqueado == null && imagenPocion != null)
            spriteDesbloqueado = imagenPocion.sprite;
    }

    public void Configurar(RecetaPocion receta, bool desbloqueada)
    {
        if (receta == null) return;
        if (imagenPocion != null)
        {
            // Las tarjetas físicas ya traen la imagen completa de la receta.
            // Solo usamos el sprite de la poción como respaldo si la tarjeta no tiene imagen.
            if (spriteDesbloqueado == null)
                spriteDesbloqueado = imagenPocion.sprite != null ? imagenPocion.sprite : receta.SpritePocion;
            imagenPocion.sprite = spriteDesbloqueado;
            imagenPocion.preserveAspect = true;
        }
        if (textoNombre != null)
        {
            textoNombre.text = receta.Nombre;
            textoNombre.fontSize = 22f;
            textoNombre.fontStyle = FontStyles.Bold;
        }
        if (textoIngredientes != null)
        {
            textoIngredientes.text = receta.TextoIngredientes;
            textoIngredientes.fontSize = 16f;
        }
        ActualizarEstado(desbloqueada);
    }

    public void ActualizarEstado(bool desbloqueada)
    {
        if (imagenPocion != null)
        {
            if (desbloqueada)
            {
                imagenPocion.sprite = spriteDesbloqueado;
                imagenPocion.enabled = imagenPocion.sprite != null;
            }
            else if (spriteBloqueado != null)
            {
                imagenPocion.sprite = spriteBloqueado;
                imagenPocion.enabled = true;
            }
            else
            {
                imagenPocion.enabled = false;
            }
        }
        if (bloqueo != null)
            bloqueo.SetActive(!desbloqueada && spriteBloqueado == null);
    }

    public void AsignarSpriteBloqueado(Sprite sprite)
    {
        spriteBloqueado = sprite;
    }

    public void AsignarAccion(UnityEngine.Events.UnityAction accion)
    {
        if (boton == null) return;
        boton.onClick.RemoveAllListeners();
        if (accion != null) boton.onClick.AddListener(accion);
    }
}





