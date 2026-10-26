using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public sealed class ArticuloTienda
{
    [SerializeField] private string nombreIngrediente;
    [SerializeField] private string categoria = "Plantas";
    [SerializeField, Min(1)] private int precio = 10;
    [SerializeField, Min(1)] private int cantidad = 1;
    [SerializeField] private string idReceta;
    public string NombreIngrediente => nombreIngrediente;
    public string Categoria => categoria;
    public int Precio => precio;
    public int Cantidad => cantidad;
    public string IdReceta => idReceta;
}

public sealed class TiendaIngredientes : MonoBehaviour
{
    [SerializeField] private SeleccionCalderoUI seleccionCalderoUI;
    [SerializeField] private SistemaPedidos sistemaPedidos;
    [SerializeField] private TMP_Text textoMonedas;
    [SerializeField] private ArticuloTienda[] articulos = new ArticuloTienda[0];

    [Header("Arte de la tienda")]
    [SerializeField] private Sprite fondoTienda;
    [SerializeField] private Sprite tituloTienda;
    [SerializeField] private Sprite botonTodas;
    [SerializeField] private Sprite botonTodasDesactivado;
    [SerializeField] private Sprite botonPlantas;
    [SerializeField] private Sprite botonPlantasDesactivado;
    [SerializeField] private Sprite botonHongos;
    [SerializeField] private Sprite botonHongosDesactivado;
    [SerializeField] private Sprite botonRaices;
    [SerializeField] private Sprite botonRaicesDesactivado;
    [SerializeField] private Sprite botonCristales;
    [SerializeField] private Sprite botonCristalesDesactivado;
    [SerializeField] private Sprite botonOtros;
    [SerializeField] private Sprite botonOtrosDesactivado;
    [SerializeField] private Sprite panelMonedas;

    private GameObject panel;
    private Transform lista;
    private TMP_Text textoMonedasTienda;
    private TMP_Text textoEstado;
    private TarjetaTiendaUI[] tarjetasFisicas = new TarjetaTiendaUI[0];
    private string categoriaActual = "Todas";
    private bool abierta;

    private void Start()
    {
        if (seleccionCalderoUI == null) seleccionCalderoUI = FindFirstObjectByType<SeleccionCalderoUI>();
        if (sistemaPedidos == null) sistemaPedidos = FindFirstObjectByType<SistemaPedidos>();
        CrearArticulosPorDefectoSiHaceFalta();
        CrearInterfaz();
        ActualizarInterfaz();
    }

    public void AlternarTienda()
    {
        abierta = !abierta;
        if (panel != null)
        {
            panel.SetActive(abierta);
            if (abierta)
            {
                panel.transform.SetAsLastSibling();
                ActualizarInterfaz();
            }
        }
    }

    public void CerrarTienda()
    {
        abierta = false;
        if (panel != null) panel.SetActive(false);
    }

    public void MostrarCategoria(string categoria)
    {
        categoriaActual = categoria;
        ActualizarBotonesCategoria();
        ActualizarInterfaz();
    }

    public void Comprar(int indice)
    {
        if (indice < 0 || indice >= articulos.Length || seleccionCalderoUI == null || sistemaPedidos == null) return;
        ArticuloTienda articulo = articulos[indice];
        if (!sistemaPedidos.IntentarGastarMonedas(articulo.Precio))
        {
            MostrarEstado("No tienes suficientes monedas.");
            return;
        }
        seleccionCalderoUI.AgregarIngredientes(articulo.NombreIngrediente, articulo.Cantidad);
        if (!string.IsNullOrWhiteSpace(articulo.IdReceta)) seleccionCalderoUI.DesbloquearReceta(articulo.IdReceta);
        MostrarEstado("Comprado: +" + articulo.Cantidad + " " + articulo.NombreIngrediente);
        ActualizarInterfaz();
    }

    private void CrearArticulosPorDefectoSiHaceFalta()
    {
        if (articulos != null && articulos.Length > 0) return;
        articulos = new[]
        {
            CrearArticulo("Flor lunar", "Plantas", 8, 2),
            CrearArticulo("Hoja de menta", "Plantas", 10, 2),
            CrearArticulo("Seta azul", "Hongos", 8, 2),
            CrearArticulo("Raíz roja", "Raíces", 10, 2),
            CrearArticulo("Cristal del sol", "Cristales", 12, 2)
        };
    }

    private ArticuloTienda CrearArticulo(string nombre, string categoria, int precio, int cantidad)
    {
        return JsonUtility.FromJson<ArticuloTienda>(JsonUtility.ToJson(new ArticuloSerializable
        {
            nombreIngrediente = nombre, categoria = categoria, precio = precio, cantidad = cantidad
        }));
    }

    private void CrearInterfaz()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        Transform panelExistente = canvas.transform.Find("PanelTienda");
        if (panelExistente != null)
        {
            panel = panelExistente.gameObject;
            lista = panel.transform.Find("Lista");
            Transform monedasExistentes = panel.transform.Find("PanelMonedas/Texto");
            textoMonedasTienda = monedasExistentes != null ? monedasExistentes.GetComponent<TMP_Text>() : null;
            Transform estadoExistente = panel.transform.Find("TextoEstado");
            textoEstado = estadoExistente != null ? estadoExistente.GetComponent<TMP_Text>() : null;
            tarjetasFisicas = lista != null ? lista.GetComponentsInChildren<TarjetaTiendaUI>(true) : new TarjetaTiendaUI[0];
            ConfigurarBotonApertura(canvas.transform);
            ConfigurarBotonesPanelExistente();
            panel.SetActive(false);
            return;
        }

        GameObject boton = CrearUI("BotonTienda", canvas.transform);
        RectTransform botonRect = boton.GetComponent<RectTransform>();
        botonRect.anchorMin = new Vector2(0, 0);
        botonRect.anchorMax = new Vector2(0, 0);
        botonRect.pivot = new Vector2(0, 0);
        botonRect.anchoredPosition = new Vector2(28, 24);
        botonRect.sizeDelta = new Vector2(190, 54);
        Image botonImagen = boton.AddComponent<Image>();
        botonImagen.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * .5f);
        botonImagen.color = new Color(.25f, .12f, .05f, .95f);
        boton.AddComponent<Button>().onClick.AddListener(AlternarTienda);
        CrearTexto("Abrir tienda", boton.transform, 22);

        panel = CrearUI("PanelTienda", canvas.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(.5f, .5f);
        panelRect.anchorMax = new Vector2(.5f, .5f);
        panelRect.pivot = new Vector2(.5f, .5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(1920, 1080);
        GameObject fondoObjeto = CrearUI("Fondo", panel.transform);
        RectTransform fondoRect = fondoObjeto.GetComponent<RectTransform>();
        fondoRect.anchorMin = Vector2.zero;
        fondoRect.anchorMax = Vector2.one;
        fondoRect.offsetMin = Vector2.zero;
        fondoRect.offsetMax = Vector2.zero;
        fondoObjeto.transform.SetAsFirstSibling();
        Image fondo = fondoObjeto.AddComponent<Image>();
        fondo.sprite = fondoTienda != null ? fondoTienda : Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * .5f);
        fondo.type = Image.Type.Simple;
        fondo.color = Color.white;

        GameObject titulo = CrearUI("TituloTienda", panel.transform);
        RectTransform tituloRect = titulo.GetComponent<RectTransform>();
        tituloRect.anchorMin = new Vector2(.5f, 1); tituloRect.anchorMax = new Vector2(.5f, 1); tituloRect.pivot = new Vector2(.5f, 1);
        tituloRect.anchoredPosition = new Vector2(0, -20); tituloRect.sizeDelta = new Vector2(1000, 160);
        Image tituloImagen = titulo.AddComponent<Image>(); tituloImagen.sprite = tituloTienda; tituloImagen.preserveAspect = true; tituloImagen.raycastTarget = false;
        GameObject monedas = CrearUI("PanelMonedas", panel.transform);
        RectTransform monedasRect = monedas.GetComponent<RectTransform>();
        monedasRect.anchorMin = new Vector2(1, 1); monedasRect.anchorMax = new Vector2(1, 1); monedasRect.pivot = Vector2.one;
        monedasRect.anchoredPosition = new Vector2(-205, -25); monedasRect.sizeDelta = new Vector2(310, 78);
        Image monedasImagen = monedas.AddComponent<Image>(); monedasImagen.sprite = panelMonedas; monedasImagen.preserveAspect = true; monedasImagen.raycastTarget = false;
        textoMonedasTienda = CrearTexto("0", monedas.transform, 25);
        textoMonedasTienda.rectTransform.sizeDelta = new Vector2(230, 48);
        textoEstado = CrearTexto("", panel.transform, 20, new Vector2(0, -455));

        GameObject cerrar = CrearUI("Cerrar", panel.transform);
        RectTransform cerrarRect = cerrar.GetComponent<RectTransform>();
        cerrarRect.anchorMin = new Vector2(1, 1);
        cerrarRect.anchorMax = new Vector2(1, 1);
        cerrarRect.pivot = Vector2.one;
        cerrarRect.anchoredPosition = new Vector2(-30, -25);
        cerrarRect.sizeDelta = new Vector2(150, 54);
        Image cerrarImagen = cerrar.AddComponent<Image>();
        cerrarImagen.sprite = fondo.sprite;
        cerrarImagen.color = new Color(.35f, .12f, .04f, 1);
        cerrar.AddComponent<Button>().onClick.AddListener(CerrarTienda);
        CrearTexto("Volver", cerrar.transform, 22);

        string[] categorias = { "Todas", "Plantas", "Hongos", "Raíces", "Cristales", "Otros" };
        for (int i = 0; i < categorias.Length; i++)
        {
            int indice = i;
            GameObject cat = CrearUI("Categoria_" + categorias[i], panel.transform);
            RectTransform r = cat.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0, 1);
            r.anchorMax = new Vector2(0, 1);
            r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(55, -135 - i * 62);
            r.sizeDelta = new Vector2(220, 52);
            Image ci = cat.AddComponent<Image>();
            ci.sprite = ObtenerSpriteCategoria(categorias[i], false);
            ci.preserveAspect = true;
            ci.color = Color.white;
            cat.AddComponent<Button>().onClick.AddListener(() => MostrarCategoria(categorias[indice]));
        }

        ActualizarBotonesCategoria();

        GameObject listaObjeto = CrearUI("Lista", panel.transform);
        RectTransform listaRect = listaObjeto.GetComponent<RectTransform>();
        listaRect.anchorMin = new Vector2(.5f, 1);
        listaRect.anchorMax = new Vector2(.5f, 1);
        listaRect.pivot = new Vector2(.5f, 1);
        listaRect.anchoredPosition = new Vector2(-70, -180);
        listaRect.sizeDelta = new Vector2(1100, 200);
        lista = listaObjeto.transform;
        GridLayoutGroup grid = listaObjeto.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(200, 180);
        grid.spacing = new Vector2(20, 10);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.padding = new RectOffset(0, 0, 0, 0);

        boton.transform.SetAsLastSibling();
        panel.transform.SetAsLastSibling();
        panel.SetActive(false);
    }

    private void ConfigurarBotonesPanelExistente()
    {
        Button cerrar = panel.transform.Find("Cerrar")?.GetComponent<Button>();
        if (cerrar != null)
        {
            cerrar.onClick.RemoveAllListeners();
            cerrar.onClick.AddListener(CerrarTienda);
        }

        string[] categorias = { "Todas", "Plantas", "Hongos", "Raíces", "Cristales", "Otros" };
        for (int i = 0; i < categorias.Length; i++)
        {
            Button boton = panel.transform.Find("Categoria_" + categorias[i])?.GetComponent<Button>();
            if (boton == null)
                continue;

            string categoria = categorias[i];
            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(() => MostrarCategoria(categoria));
        }
    }

    private void ConfigurarBotonApertura(Transform canvasTransform)
    {
        Button abrir = canvasTransform.Find("BotonTienda")?.GetComponent<Button>();
        if (abrir == null)
            return;

        abrir.onClick.RemoveAllListeners();
        abrir.onClick.AddListener(AlternarTienda);
    }

    private void ActualizarInterfaz()
    {
        if (textoMonedasTienda != null && sistemaPedidos != null) textoMonedasTienda.text = sistemaPedidos.Monedas.ToString();
        if (textoMonedas != null && sistemaPedidos != null) textoMonedas.text = sistemaPedidos.Monedas.ToString();
        if (lista == null) return;

        if (tarjetasFisicas != null && tarjetasFisicas.Length > 0)
        {
            for (int i = 0; i < tarjetasFisicas.Length; i++)
            {
                TarjetaTiendaUI tarjeta = tarjetasFisicas[i];
                if (tarjeta == null)
                    continue;

                bool disponible = i < articulos.Length &&
                    (categoriaActual == "Todas" || articulos[i].Categoria == categoriaActual);
                tarjeta.gameObject.SetActive(disponible);
                if (disponible)
                {
                    int indice = i;
                    ArticuloTienda articulo = articulos[i];
                    tarjeta.Configurar(
                        articulo.NombreIngrediente,
                        articulo.Precio,
                        seleccionCalderoUI != null ? seleccionCalderoUI.ObtenerSpriteParaTienda(articulo.NombreIngrediente) : null,
                        () => Comprar(indice));
                }
            }
            return;
        }

        for (int i = lista.childCount - 1; i >= 0; i--)
        {
            GameObject anterior = lista.GetChild(i).gameObject;
            anterior.SetActive(false);
            Destroy(anterior);
        }
        for (int i = 0; i < articulos.Length; i++)
        {
            if (categoriaActual != "Todas" && articulos[i].Categoria != categoriaActual) continue;
            CrearTarjeta(articulos[i], i);
        }
    }

    private void ActualizarBotonesCategoria()
    {
        if (panel == null)
            return;

        string[] categorias = { "Todas", "Plantas", "Hongos", "Raíces", "Cristales", "Otros" };
        for (int i = 0; i < categorias.Length; i++)
        {
            Transform boton = panel.transform.Find("Categoria_" + categorias[i]);
            if (boton == null)
                continue;

            Image imagen = boton.GetComponent<Image>();
            if (imagen != null)
                imagen.sprite = ObtenerSpriteCategoria(categorias[i], categoriaActual == categorias[i]);
        }
    }

    private Sprite ObtenerSpriteCategoria(string categoria, bool activo)
    {
        if (categoria == "Plantas") return activo ? botonPlantas : botonPlantasDesactivado;
        if (categoria == "Hongos") return activo ? botonHongos : botonHongosDesactivado;
        if (categoria == "Raíces") return activo ? botonRaices : botonRaicesDesactivado;
        if (categoria == "Cristales") return activo ? botonCristales : botonCristalesDesactivado;
        if (categoria == "Otros") return activo ? botonOtros : botonOtrosDesactivado;
        return activo ? botonTodas : botonTodasDesactivado;
    }

    private void CrearTarjeta(ArticuloTienda articulo, int indice)
    {
        GameObject tarjeta = CrearUI("Articulo_" + articulo.NombreIngrediente, lista);
        Image fondo = tarjeta.AddComponent<Image>();
        fondo.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * .5f);
        fondo.color = Color.clear;

        GameObject iconoObjeto = CrearUI("Icono", tarjeta.transform);
        RectTransform ir = iconoObjeto.GetComponent<RectTransform>();
        ir.anchorMin = new Vector2(.5f, 1);
        ir.anchorMax = new Vector2(.5f, 1);
        ir.anchoredPosition = new Vector2(0, -6);
        ir.sizeDelta = new Vector2(90, 90);
        Image icono = iconoObjeto.AddComponent<Image>();
        icono.sprite = seleccionCalderoUI != null ? seleccionCalderoUI.ObtenerSpriteParaTienda(articulo.NombreIngrediente) : null;
        icono.preserveAspect = true;
        icono.enabled = icono.sprite != null;
        icono.raycastTarget = false;

        CrearTexto(articulo.NombreIngrediente, tarjeta.transform, 24, new Vector2(0, -35));
        GameObject precio = CrearUI("Precio", tarjeta.transform);
        RectTransform precioRect = precio.GetComponent<RectTransform>();
        precioRect.anchorMin = new Vector2(.5f, .5f); precioRect.anchorMax = new Vector2(.5f, .5f); precioRect.pivot = new Vector2(.5f, .5f);
        precioRect.anchoredPosition = new Vector2(0, -180); precioRect.sizeDelta = new Vector2(135, 34);
        Image precioImagen = precio.AddComponent<Image>(); precioImagen.sprite = panelMonedas; precioImagen.preserveAspect = true; precioImagen.raycastTarget = false;
        TMP_Text precioTexto = CrearTexto(articulo.Precio.ToString(), precio.transform, 22);
        precioTexto.rectTransform.sizeDelta = new Vector2(120, 30);
        tarjeta.AddComponent<Button>().onClick.AddListener(() => Comprar(indice));
    }

    private GameObject CrearUI(string nombre, Transform padre)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.transform.SetParent(padre, false);
        return objeto;
    }

    private TMP_Text CrearTexto(string contenido, Transform padre, float tamano, Vector2? posicion = null)
    {
        GameObject objeto = CrearUI("Texto", padre);
        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(.5f, .5f);
        rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = posicion ?? Vector2.zero;
        rect.sizeDelta = new Vector2(520, 42);
        TMP_Text texto = objeto.AddComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = Color.white;
        texto.raycastTarget = false;
        return texto;
    }

    private void MostrarEstado(string mensaje)
    {
        if (textoEstado != null) textoEstado.text = mensaje;
    }

    [Serializable]
    private sealed class ArticuloSerializable
    {
        public string nombreIngrediente;
        public string categoria;
        public int precio;
        public int cantidad;
    }
}




















