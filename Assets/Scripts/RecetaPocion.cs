using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public sealed class RecetaPocion
{
    [SerializeField] private string id;
    [SerializeField] private string nombre;
    [SerializeField] private string ingredienteA;
    [SerializeField] private string ingredienteB;
    [SerializeField] private Sprite spritePocion;
    [SerializeField] private Sprite iconoPedido;
    [SerializeField] private Image imagenRecetario;
    [SerializeField, Min(0)] private int recompensa;
    [SerializeField] private bool desbloqueadaInicialmente;

    public string Id => id;
    public string Nombre => nombre;
    public string IngredienteA => ingredienteA;
    public string IngredienteB => ingredienteB;
    public Sprite SpritePocion => spritePocion;
    public Sprite IconoPedido => iconoPedido;
    public Image ImagenRecetario => imagenRecetario;
    public int Recompensa => recompensa;
    public bool DesbloqueadaInicialmente => desbloqueadaInicialmente;

    public bool Coincide(string ingrediente1, string ingrediente2)
    {
        return
            (ingrediente1 == ingredienteA && ingrediente2 == ingredienteB) ||
            (ingrediente1 == ingredienteB && ingrediente2 == ingredienteA);
    }

    public string TextoIngredientes => $"{ingredienteA} + {ingredienteB}";
}