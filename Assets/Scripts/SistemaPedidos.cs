using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SistemaPedidos : MonoBehaviour
{
    [Header("Filas del pedido")]
    [SerializeField] private GameObject filaPedido1;
    [SerializeField] private Image imagePedido1;
    [SerializeField] private TMP_Text textPedido1;
    [SerializeField] private GameObject filaPedido2;
    [SerializeField] private Image imagePedido2;
    [SerializeField] private TMP_Text textPedido2;

    [Header("Progreso y recompensa")]
    [SerializeField] private TMP_Text textoPreparadas;
    [SerializeField] private Slider barraProgreso;
    [SerializeField] private TMP_Text textoRecompensa;
    [SerializeField] private TMP_Text textoMonedas;
    [SerializeField] private Image imageMonedas;
    [SerializeField] private TMP_Text textoResultado;

    [Header("Cliente")]
    [SerializeField] private Image imageCliente;
    [SerializeField] private Sprite[] spritesClientes;


    [Header("Catálogo")]
    [SerializeField] private SeleccionCalderoUI seleccionCalderoUI;

    private int[] pedido;
    private int[] pocionesPreparadas;
    private int monedas;

    public int Monedas => monedas;

    public bool IntentarGastarMonedas(int cantidad)
    {
        if (cantidad < 0 || monedas < cantidad)
            return false;

        monedas -= cantidad;
        ActualizarMonedas();
        return true;
    }

    public void AgregarMonedas(int cantidad)
    {
        if (cantidad <= 0)
            return;

        monedas += cantidad;
        ActualizarMonedas();
    }

    private RecetaPocion[] Recetas =>
        seleccionCalderoUI != null
            ? seleccionCalderoUI.Recetas
            : new RecetaPocion[0];

    private void Start()
    {
        if (Recetas == null || Recetas.Length == 0)
        {
            Debug.LogError("SistemaPedidos necesita al menos una receta en el catálogo.");
            enabled = false;
            return;
        }

        pedido = new int[Recetas.Length];
        pocionesPreparadas = new int[Recetas.Length];

        ActualizarMonedas();
        GenerarPedido();
    }

    public void NotificarPocionPreparada(string nombrePocion)
    {
        int indice = BuscarPocion(nombrePocion);
        if (indice < 0)
        {
            MostrarResultado($"No reconozco esta poción: {nombrePocion}");
            return;
        }

        if (seleccionCalderoUI == null ||
            !seleccionCalderoUI.EstaDesbloqueada(Recetas[indice]))
        {
            MostrarResultado("Esta poción aún no está desbloqueada.");
            return;
        }

        pocionesPreparadas[indice]++;
        ActualizarProgreso();
        MostrarResultado($"Poción lista: {Recetas[indice].Nombre}");
    }

    public void EntregarPedido()
    {
        if (pedido == null || pocionesPreparadas == null)
            return;

        for (int i = 0; i < pedido.Length; i++)
        {
            if (pocionesPreparadas[i] < pedido[i])
            {
                MostrarResultado("Aún faltan pociones para completar el pedido.");
                return;
            }
        }

        int recompensaTotal = 0;
        for (int i = 0; i < pedido.Length; i++)
        {
            pocionesPreparadas[i] -= pedido[i];
            recompensaTotal += pedido[i] * Recetas[i].Recompensa;
            pedido[i] = 0;
        }

        monedas += recompensaTotal;
        ActualizarMonedas();
        GenerarPedido();

        if (seleccionCalderoUI != null)
            seleccionCalderoUI.NotificarCambioPedido();

        MostrarResultado($"¡Pedido entregado! Ganaste {recompensaTotal} monedas.");
    }

    private void GenerarPedido()
    {
        if (pedido == null || Recetas == null || Recetas.Length == 0)
            return;

        for (int i = 0; i < pedido.Length; i++)
            pedido[i] = 0;

        List<int> indicesDesbloqueados = new List<int>();
        for (int i = 0; i < Recetas.Length; i++)
        {
            if (seleccionCalderoUI != null &&
                seleccionCalderoUI.EstaDesbloqueada(Recetas[i]))
            {
                indicesDesbloqueados.Add(i);
            }
        }

        if (indicesDesbloqueados.Count == 0)
        {
            ActualizarFilasPedido();
            ActualizarRecompensa();
            ActualizarProgreso();
            return;
        }

        for (int i = 0; i < indicesDesbloqueados.Count; i++)
        {
            int indiceAleatorio = Random.Range(i, indicesDesbloqueados.Count);
            (indicesDesbloqueados[i], indicesDesbloqueados[indiceAleatorio]) =
                (indicesDesbloqueados[indiceAleatorio], indicesDesbloqueados[i]);
        }

        int tiposSolicitados = Mathf.Min(2, indicesDesbloqueados.Count);
        int cantidadDeTipos = Random.Range(1, tiposSolicitados + 1);
        for (int i = 0; i < cantidadDeTipos; i++)
            pedido[indicesDesbloqueados[i]] = 1;

        ActualizarFilasPedido();
        ActualizarRecompensa();
        ActualizarProgreso();
        ElegirCliente();
    }

    private void ActualizarFilasPedido()
    {
        int primeraPocion = -1;
        int segundaPocion = -1;

        for (int i = 0; i < pedido.Length; i++)
        {
            if (pedido[i] <= 0)
                continue;

            if (primeraPocion == -1)
                primeraPocion = i;
            else
                segundaPocion = i;
        }

        ConfigurarFila(filaPedido1, imagePedido1, textPedido1, primeraPocion);
        ConfigurarFila(filaPedido2, imagePedido2, textPedido2, segundaPocion);
    }

    private void ConfigurarFila(
        GameObject fila,
        Image imagen,
        TMP_Text texto,
        int indicePocion)
    {
        if (fila == null)
            return;

        bool mostrarFila = indicePocion >= 0;
        fila.SetActive(mostrarFila);
        if (!mostrarFila)
            return;

        RecetaPocion receta = Recetas[indicePocion];
        if (texto != null)
            texto.text = $"{pedido[indicePocion]} × {receta.Nombre}";

        if (imagen != null)
        {
            imagen.sprite = receta.IconoPedido;
            imagen.enabled = receta.IconoPedido != null;
            imagen.preserveAspect = true;
        }
    }

    private void ActualizarProgreso()
    {
        if (pedido == null || pocionesPreparadas == null)
            return;

        int totalPedido = 0;
        int totalPreparadas = 0;
        for (int i = 0; i < pedido.Length; i++)
        {
            totalPedido += pedido[i];
            totalPreparadas += Mathf.Min(pedido[i], pocionesPreparadas[i]);
        }

        if (textoPreparadas != null)
            textoPreparadas.text = $"Preparadas: {totalPreparadas}/{totalPedido}";

        if (barraProgreso != null)
        {
            barraProgreso.maxValue = Mathf.Max(totalPedido, 1);
            barraProgreso.value = totalPreparadas;
        }
    }

    private void ActualizarRecompensa()
    {
        if (pedido == null)
            return;

        int recompensaTotal = 0;
        for (int i = 0; i < pedido.Length; i++)
            recompensaTotal += pedido[i] * Recetas[i].Recompensa;

        if (textoRecompensa != null)
            textoRecompensa.text = $"Recompensa: {recompensaTotal} monedas";
    }

    private void ActualizarMonedas()
    {
        if (textoMonedas != null)
            textoMonedas.text = $"{monedas}";
    }

    private void ElegirCliente()
    {
        if (imageCliente == null || spritesClientes == null || spritesClientes.Length == 0)
            return;

        Sprite cliente = spritesClientes[Random.Range(0, spritesClientes.Length)];
        imageCliente.sprite = cliente;
        imageCliente.enabled = cliente != null;
        imageCliente.preserveAspect = true;
    }

    private int BuscarPocion(string nombre)
    {
        for (int i = 0; i < Recetas.Length; i++)
        {
            if (Recetas[i] != null && Recetas[i].Nombre == nombre)
                return i;
        }

        return -1;
    }

    private void MostrarResultado(string mensaje)
    {
        if (textoResultado != null)
            textoResultado.text = mensaje;
    }
}