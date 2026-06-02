using System.Collections.Generic;
using UnityEngine;

public class ItemFirstTimeDialogueSystem : MonoBehaviour
{
    public static ItemFirstTimeDialogueSystem Instance;

    private readonly HashSet<string> itemsVistos = new HashSet<string>();

    private void Awake()
    {
        Instance = this;
    }

    public void MostrarFrasePrimeraVez(ItemData itemData)
    {
        if (itemData == null)
            return;

        string id = itemData.name;

        if (itemsVistos.Contains(id))
            return;

        itemsVistos.Add(id);

        string frase = ObtenerFrase(itemData);

        if (!string.IsNullOrWhiteSpace(frase))
        {
            NightMessageUI.Instance?.ShowMessage(frase);
        }
    }

    private string ObtenerFrase(ItemData itemData)
    {
        string nombre = itemData.name.ToLower();

        // COMIDA / SUPERVIVENCIA

        if (nombre.Contains("platano") || nombre.Contains("plátano"))
        {
            string[] frases =
            {
                "Oh, un plátano... me lo guardo para cuando tenga hambre.",
                "Nunca viene mal algo de comida.",
                "Perfecto, esto me dará fuerzas."
            };

            return frases[Random.Range(0, frases.Length)];
        }

        if (nombre.Contains("coco"))
        {
            string[] frases =
            {
                "Un coco... esto me vendrá bien para sobrevivir.",
                "Algo de comida siempre viene bien.",
                "Perfecto, me servirá para aguantar más."
            };

            return frases[Random.Range(0, frases.Length)];
        }

        if (nombre.Contains("cafe") || nombre.Contains("café"))
        {
            string[] frases =
            {
                "Un café... esto me despejará un poco.",
                "Nada mal, esto me ayudará a seguir.",
                "Perfecto, algo para recuperar energía."
            };

            return frases[Random.Range(0, frases.Length)];
        }

        // CONSTRUCCIÓN

        if (nombre.Contains("madera"))
        {
            string[] frases =
            {
                "Madera... perfecta para seguir construyendo.",
                "Esto me acerca un poco más al barco.",
                "Nunca sobra algo de madera."
            };

            return frases[Random.Range(0, frases.Length)];
        }

        if (nombre.Contains("clavo"))
        {
            string[] frases =
            {
                "Clavos... pequeños, pero muy útiles.",
                "Todo ayuda para reconstruir el barco.",
                "Esto seguro me viene bien."
            };

            return frases[Random.Range(0, frases.Length)];
        }

        if (nombre.Contains("cuerda"))
        {
            string[] frases =
            {
                "Una cuerda... seguro que me viene bien más adelante.",
                "Esto me servirá para arreglar cosas.",
                "Perfecto, otro material útil."
            };

            return frases[Random.Range(0, frases.Length)];
        }

        // GEMAS

        if (nombre.Contains("gema"))
        {
            string[] frases =
            {
                "Vaya... esta gema parece importante.",
                "Será mejor guardarla bien.",
                "Esto parece bastante valioso."
            };

            return frases[Random.Range(0, frases.Length)];
        }

        // OBJETOS DE TIENDA (MONEDA)

        if (nombre.Contains("concha"))
        {
            string[] frases =
            {
                "Una concha... me servirá para comprar en la tienda.",
                "Perfecto, otra concha para conseguir cosas útiles.",
                "Con esto podré comprar algo interesante."
            };

            return frases[Random.Range(0, frases.Length)];
        }

        if (nombre.Contains("tulipan") || nombre.Contains("tulipán"))
        {
            string[] frases =
            {
                "Un tulipán... esto también sirve para comprar en la tienda.",
                "Bien, otra cosa útil para intercambiar.",
                "Seguro que esto me ayuda a conseguir algo interesante."
            };

            return frases[Random.Range(0, frases.Length)];
        }

        if (nombre.Contains("piña") || nombre.Contains("pina"))
        {
            string[] frases =
            {
                "Una piña... me servirá para comprar en la tienda.",
                "Otra piña para conseguir objetos útiles.",
                "Perfecto, esto puede ayudarme a comprar algo."
            };

            return frases[Random.Range(0, frases.Length)];
        }

        // SI NO HAY FRASE
        return "";
    }
}