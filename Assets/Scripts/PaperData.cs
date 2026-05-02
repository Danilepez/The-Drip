using UnityEngine;

[CreateAssetMenu(fileName = "Paper_", menuName = "The Drip/Paper Data")]
public class PaperData : ScriptableObject
{
    [Tooltip("ID único del papel (0-10). Debe coincidir con el Paper en la escena.")]
    public int paperId;

    [Tooltip("Título del documento (aparece en el HUD y en la examinación).")]
    public string title;

    [Tooltip("Contenido completo del documento.")]
    [TextArea(6, 25)]
    public string content;

    [Tooltip("Imagen miniatura para el slot del inventario. Puede ser una textura del papel.")]
    public Sprite thumbnail;
}
