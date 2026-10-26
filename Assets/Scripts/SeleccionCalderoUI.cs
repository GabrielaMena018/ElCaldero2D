using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeleccionCalderoUI : MonoBehaviour
{
    [Header("Espacios del caldero")]
    [SerializeField] private TMP_Text textoIngrediente1;
    [SerializeField] private TMP_Text textoIngrediente2;
    [SerializeField] private TMP_Text textoEstado;
    [SerializeField] private TMP_Text textoResultado;

    [Header("Iconos de los espacios")]
    [SerializeField] private Image iconoIngrediente1;
    [SerializeField] private Image iconoIngrediente2;

    [Header("Sprites de ingredientes")]
    [SerializeField] private Sprite spriteFlorLunar;
    [SerializeField] private Sprite spriteSetaAzul;
    [SerializeField] private Sprite spriteRaizRoja;
    [SerializeField] private Sprite spriteHojaMenta;
    [SerializeField] private Sprite spriteCristalSol;

    [Header("Catálogo de pociones")]
    [SerializeField] private RecetaPocion[] recetas = new RecetaPocion[0];

    [Header("Imagen temporal de la poción creada")]
    [SerializeField] private Image imagenPocionCreada;
    [SerializeField] private float segundosVisible = 2.5f;

    [Header("Banners de estado")]
    [SerializeField] private Image imagenBannerEstado;
    [SerializeField] private Sprite bannerSeleccionaDos;
    [SerializeField] private Sprite bannerSeleccionaOtro;
    [SerializeField] private Sprite bannerListoMezclar;
    [SerializeField] private Sprite bannerPocionListaVender;

    [Header("Botones del inventario")]
    [SerializeField] private Button botonFlorLunar;
    [SerializeField] private Button botonSetaAzul;
    [SerializeField] private Button botonRaizRoja;
    [SerializeField] private Button botonHojaMenta;
    [SerializeField] private Button botonCristalSol;

    [Header("Pedidos")]
    [SerializeField] private SistemaPedidos sistemaPedidos;

    private int unidadesFlorLunar = 5;
    private int unidadesSetaAzul = 6;
    private int unidadesRaizRoja = 4;
    private int unidadesHojaMenta = 8;
    private int unidadesCristalSol = 3;

    private int ingredientesSeleccionados;
    private Coroutine rutinaOcultarPocion;
    private Sprite bannerActual;

    public RecetaPocion[] Recetas => recetas ?? new RecetaPocion[0];

    public int ObtenerCantidadIngrediente(string nombre)
    {
        switch (nombre)
        {
            case "Flor lunar": return unidadesFlorLunar;
            case "Seta azul": return unidadesSetaAzul;
            case "Raíz roja": return unidadesRaizRoja;
            case "Hoja de menta": return unidadesHojaMenta;
            case "Cristal del sol": return unidadesCristalSol;
            default: return 0;
        }
    }

    public void AgregarIngredientes(string nombre, int cantidad)
    {
        if (cantidad <= 0)
            return;

        switch (nombre)
        {
            case "Flor lunar": unidadesFlorLunar += cantidad; break;
            case "Seta azul": unidadesSetaAzul += cantidad; break;
            case "Raíz roja": unidadesRaizRoja += cantidad; break;
            case "Hoja de menta": unidadesHojaMenta += cantidad; break;
            case "Cristal del sol": unidadesCristalSol += cantidad; break;
            default: return;
        }

        ActualizarCantidades();
    }

    private const string ClaveDesbloqueoPrefijo = "ElCaldero.Receta.";

    public bool EstaDesbloqueada(RecetaPocion receta)
    {
        if (receta == null)
            return false;

        if (receta.DesbloqueadaInicialmente)
            return true;

        if (string.IsNullOrWhiteSpace(receta.Id))
            return false;

        return PlayerPrefs.GetInt(ClaveDesbloqueoPrefijo + receta.Id, 0) == 1;
    }

    public bool DesbloquearReceta(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        foreach (RecetaPocion receta in Recetas)
        {
            if (receta == null || receta.Id != id)
                continue;

            PlayerPrefs.SetInt(ClaveDesbloqueoPrefijo + receta.Id, 1);
            PlayerPrefs.Save();
            ActualizarVisibilidadRecetario();
            return true;
        }

        return false;
    }

    private void ActualizarVisibilidadRecetario()
    {
        foreach (RecetaPocion receta in Recetas)
        {
            if (receta == null || receta.ImagenRecetario == null)
                continue;

            bool desbloqueada = EstaDesbloqueada(receta);
            RecetaUI recetaUI = receta.ImagenRecetario.GetComponentInParent<RecetaUI>();
            if (recetaUI != null)
                // Las recetas físicas usan la imagen configurada en RecetaPocion como
                // estado desbloqueado y luego alternan su estado visual.
                recetaUI.Configurar(receta, desbloqueada);
            else
                receta.ImagenRecetario.gameObject.SetActive(desbloqueada);
        }
    }

    private void Start()
    {
        if (imagenPocionCreada != null)
            imagenPocionCreada.gameObject.SetActive(false);

        MostrarBanner(bannerSeleccionaDos);
        ActualizarVisibilidadRecetario();
    }

    public void SeleccionarIngrediente(string nombre)
    {
        if (ingredientesSeleccionados >= 2)
        {
            MostrarBanner(bannerListoMezclar);
            return;
        }

        if (textoResultado != null)
            textoResultado.text = "";

        if (ingredientesSeleccionados == 0)
        {
            if (textoIngrediente1 != null)
                textoIngrediente1.text = nombre;

            MostrarIcono(iconoIngrediente1, nombre);
            ingredientesSeleccionados = 1;

            MostrarBanner(bannerSeleccionaOtro);
        }
        else
        {
            if (textoIngrediente2 != null)
                textoIngrediente2.text = nombre;

            MostrarIcono(iconoIngrediente2, nombre);
            ingredientesSeleccionados = 2;

            MostrarBanner(bannerListoMezclar);
        }
    }

    public void LimpiarCaldero()
    {
        LimpiarEspaciosIngrediente();

        if (textoResultado != null)
            textoResultado.text = "";

        OcultarPocionCreada();
        MostrarBanner(bannerSeleccionaDos);
    }

    private void LimpiarEspaciosIngrediente()
    {
        if (textoIngrediente1 != null)
            textoIngrediente1.text = "";

        if (textoIngrediente2 != null)
            textoIngrediente2.text = "";

        if (textoEstado != null)
            textoEstado.text = "";

        ingredientesSeleccionados = 0;

        LimpiarIcono(iconoIngrediente1);
        LimpiarIcono(iconoIngrediente2);
    }

    private void LimpiarIcono(Image icono)
    {
        if (icono == null)
            return;

        icono.sprite = null;
        icono.enabled = false;
    }

    private void MostrarIcono(Image destino, string ingrediente)
    {
        if (destino == null)
            return;

        destino.sprite = ObtenerSpriteIngrediente(ingrediente);
        destino.preserveAspect = true;
        destino.enabled = destino.sprite != null;
    }

    private Sprite ObtenerSpriteIngrediente(string ingrediente)
    {
        switch (ingrediente)
        {
            case "Flor lunar": return spriteFlorLunar;
            case "Seta azul": return spriteSetaAzul;
            case "Raíz roja": return spriteRaizRoja;
            case "Hoja de menta": return spriteHojaMenta;
            case "Cristal del sol": return spriteCristalSol;
            default: return null;
        }
    }

    public Sprite ObtenerSpriteParaTienda(string ingrediente)
    {
        return ObtenerSpriteIngrediente(ingrediente);
    }

    private void MostrarBanner(Sprite banner)
    {
        bannerActual = banner;

        if (imagenBannerEstado == null)
            return;

        if (banner == null)
        {
            imagenBannerEstado.gameObject.SetActive(false);
            return;
        }

        imagenBannerEstado.sprite = banner;
        imagenBannerEstado.preserveAspect = true;
        imagenBannerEstado.gameObject.SetActive(true);

        // El banner reemplaza al texto del estado.
        if (textoEstado != null)
            textoEstado.text = "";
    }

    private void OcultarBanner()
    {
        MostrarBanner(null);
    }

    public void Mezclar()
    {
        if (ingredientesSeleccionados < 2)
        {
            MostrarBanner(ingredientesSeleccionados == 0
                ? bannerSeleccionaDos
                : bannerSeleccionaOtro);
            return;
        }

        string ingrediente1 = textoIngrediente1 != null
            ? textoIngrediente1.text
            : "";
        string ingrediente2 = textoIngrediente2 != null
            ? textoIngrediente2.text
            : "";

        RecetaPocion receta = BuscarReceta(ingrediente1, ingrediente2);
        if (receta == null)
        {
            OcultarBanner();
            if (textoResultado != null)
                textoResultado.text = "La mezcla no produjo una poción conocida.";
            return;
        }

        // Una mezcla correcta descubre la receta por primera vez.
        // Antes BuscarReceta descartaba las recetas bloqueadas, por lo que nunca
        // podían descubrirse desde el caldero.
        if (!EstaDesbloqueada(receta))
            DesbloquearReceta(receta.Id);

        if (!HayIngredientesSuficientes(ingrediente1, ingrediente2))
            return;

        ConsumirIngrediente(ingrediente1);
        ConsumirIngrediente(ingrediente2);

        NotificarPocionPreparada(receta.Nombre);
        MostrarPocionCreada(receta.SpritePocion);
        ActualizarCantidades();
        LimpiarEspaciosIngrediente();
        // Refresca la tarjeta física después de completar la mezcla para que el
        // estado "Bloqueada" desaparezca en el mismo frame del descubrimiento.
        ActualizarVisibilidadRecetario();

        if (textoResultado != null)
            textoResultado.text = "";
    }

    private RecetaPocion BuscarReceta(string ingrediente1, string ingrediente2)
    {
        foreach (RecetaPocion receta in Recetas)
        {
            if (receta != null && receta.Coincide(ingrediente1, ingrediente2))
                return receta;
        }

        return null;
    }

    private void NotificarPocionPreparada(string nombrePocion)
    {
        if (sistemaPedidos != null)
        {
            sistemaPedidos.NotificarPocionPreparada(nombrePocion);
        }
        else
        {
            Debug.LogWarning(
                "No asignaste SistemaPedidos en el Inspector de ControlCaldero."
            );
        }
    }

    private void MostrarPocionCreada(Sprite sprite)
    {
        if (rutinaOcultarPocion != null)
            StopCoroutine(rutinaOcultarPocion);

        if (imagenPocionCreada != null && sprite != null)
        {
            imagenPocionCreada.sprite = sprite;
            imagenPocionCreada.preserveAspect = true;
            imagenPocionCreada.gameObject.SetActive(true);
        }
        else if (imagenPocionCreada == null)
        {
            Debug.LogWarning("No asignaste Imagen Poción Creada en el Inspector.");
        }
        else
        {
            imagenPocionCreada.gameObject.SetActive(false);
            Debug.LogWarning("No asignaste el sprite de esta poción en el Inspector.");
        }

        MostrarBanner(bannerPocionListaVender);
        rutinaOcultarPocion = StartCoroutine(OcultarPocionDespues());
    }

    private IEnumerator OcultarPocionDespues()
    {
        yield return new WaitForSeconds(segundosVisible);

        if (imagenPocionCreada != null)
            imagenPocionCreada.gameObject.SetActive(false);

        // Si el jugador cambió a otro banner durante la espera, lo conserva.
        if (bannerActual == bannerPocionListaVender)
            OcultarBanner();

        rutinaOcultarPocion = null;
    }

    private void OcultarPocionCreada()
    {
        if (rutinaOcultarPocion != null)
        {
            StopCoroutine(rutinaOcultarPocion);
            rutinaOcultarPocion = null;
        }

        if (imagenPocionCreada != null)
            imagenPocionCreada.gameObject.SetActive(false);
    }

    // SistemaPedidos llama este método cuando se entrega el pedido
    // y aparece uno nuevo.
    public void NotificarCambioPedido()
    {
        OcultarPocionCreada();
        OcultarBanner();
    }

    private bool HayIngredientesSuficientes(
        string ingrediente1,
        string ingrediente2)
    {
        if (ObtenerCantidad(ingrediente1) <= 0 ||
            ObtenerCantidad(ingrediente2) <= 0)
        {
            OcultarBanner();

            if (textoResultado != null)
                textoResultado.text = "No tienes suficientes ingredientes.";

            return false;
        }

        return true;
    }

    private int ObtenerCantidad(string ingrediente)
    {
        switch (ingrediente)
        {
            case "Flor lunar": return unidadesFlorLunar;
            case "Seta azul": return unidadesSetaAzul;
            case "Raíz roja": return unidadesRaizRoja;
            case "Hoja de menta": return unidadesHojaMenta;
            case "Cristal del sol": return unidadesCristalSol;
            default: return 0;
        }
    }

    private void ConsumirIngrediente(string ingrediente)
    {
        switch (ingrediente)
        {
            case "Flor lunar":
                unidadesFlorLunar--;
                break;

            case "Seta azul":
                unidadesSetaAzul--;
                break;

            case "Raíz roja":
                unidadesRaizRoja--;
                break;

            case "Hoja de menta":
                unidadesHojaMenta--;
                break;

            case "Cristal del sol":
                unidadesCristalSol--;
                break;
        }
    }

    private void ActualizarCantidades()
    {
        ActualizarTextoBoton(botonFlorLunar, unidadesFlorLunar);
        ActualizarTextoBoton(botonSetaAzul, unidadesSetaAzul);
        ActualizarTextoBoton(botonRaizRoja, unidadesRaizRoja);
        ActualizarTextoBoton(botonHojaMenta, unidadesHojaMenta);
        ActualizarTextoBoton(botonCristalSol, unidadesCristalSol);
    }

    private void ActualizarTextoBoton(Button boton, int cantidad)
    {
        if (boton == null)
            return;

        IngredienteUI ingredienteUI = boton.GetComponent<IngredienteUI>();
        if (ingredienteUI != null)
        {
            ingredienteUI.ActualizarCantidad(cantidad);
            return;
        }

        TMP_Text texto = boton.GetComponentInChildren<TMP_Text>();

        if (texto != null)
            texto.text = $"{cantidad} unidades";
    }
}




