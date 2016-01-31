using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Rune : MonoBehaviour 
{
    private Text _text;
    private Card _card;

    public void SetGlyph(Card card)
    {
        _card = card;
        _text = GetComponentInChildren<Text>();
        _text.text = card.Symbol;
    }

    private bool _selected;
    public bool Selected { get { return _selected; }
        set
        {
            _selected = value;
            var col = Card.GetColor(_card.Symbol);
            GetComponent<Graphic>().color = _selected ? col : Color.Lerp(col, Color.white, 0.5f);
        }
    }

	public void Use ()
    {
        FindObjectOfType<Game>().Self.CastGlyph(_card);
	}
}
