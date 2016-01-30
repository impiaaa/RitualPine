using UnityEngine;
using System.Collections.Generic;
using FableLabs.Anim;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

public enum RuneColor
{
    Red,
    Blue,
    Green,
    Orange
}


public class Card
{
    public static string Symbols = "-|v^<>ZO";
    private static Color[] _colors = { 
                                         new Color(1.0f, 0.0f, 0.0f, 1.0f),
                                         new Color(0.0f, 1.0f, 0.0f, 1.0f),
                                         new Color(0.0f, 0.0f, 1.0f, 1.0f),
                                         new Color(1.0f, 0.7f, 0.0f, 1.0f),
                                     };
    public static RuneColor[] _symbolColor = 
                                    { 
                                        RuneColor.Red, RuneColor.Red, 
                                        RuneColor.Blue, RuneColor.Blue, 
                                        RuneColor.Green, RuneColor.Green, 
                                        RuneColor.Orange, RuneColor.Orange
                                    };
    public static Color GetColor (RuneColor rc) { return _colors[(int)rc]; }

    public string Symbol;
    public RuneColor Color;
    public Card(string sym)
    {
        Symbol = sym;
        Color = _symbolColor[Symbols.IndexOf(sym)];
    }
}

public class Deck
{
    public List<Card> _cards = new List<Card>();
    public Deck(int size)
    {
        for(int i=0;i<size;i++)
        {
            var c = new Card(new string( new char[] { Card.Symbols[Random.Range(0, Card.Symbols.Length)] }));
            _cards.Add(c);
        }
    }

    public Card Draw()
    {
        var c = _cards[0];
        _cards.RemoveAt(0);
        return c;
    }
}

public class TreeNode
{
    public TreeNode Parent;
    public List<TreeNode> Children = new List<TreeNode>();
    public Card Symbol;
    public int Valence = 2;
    public string Id = "";
    public int Depth = 0;

    public TreeNode Grow(Card sym)
    {
        var tn = new TreeNode();
        tn.Symbol = sym;
        Attach(tn, 4);
        return tn;
    }

    public bool Attach(TreeNode n, int max)
    {
        Debug.Log("Attach " + Id + "? Valence: "+Children.Count+"/"+Valence);
        if (Children.Count < Valence)
        {
            n.Parent = this;
            n.Id = (n.Parent.Id + "-" + Children.Count).TrimStart('-');
            n.Depth = Depth + 1;
            Children.Add(n);
            return true;
        }
        for (int i = Depth + 1; i <= max; i++)
        {
            for(int j=0;j<Children.Count;j++)
            {
                if (Children[j].Attach(n, max)) { return true; }
            }
        }
        return false;
    }
}

public class Game : MonoBehaviour 
{
    public Slider Timer;
    public float TurnTime = 4.0f;
    public int HandSize = 3;
    public Deck Pool;
    public List<Card> Hand = new List<Card>();
    public TreeNode Root;
    public TreeNode Cursor;
    public Transform Tree;
    public Card Selection;

    public Card RootCard { get { return Root.Children.Count > 0 ? Root.Children[0].Symbol : null; } }

    public int Phase;
    public Action<Action>[] Phases;

    private void Draw()
    {
        Hand.Clear();
        for(int i=0;i<HandSize;i++)
        {
            var c = Pool.Draw();
            bool repeat = false;
            if (RootCard != null && c.Symbol == RootCard.Symbol) { repeat = true; }
            for (int j = 0; j < Hand.Count;j++ )
            {
                if (Hand[j].Symbol == c.Symbol) { repeat = true; }
            }
            if (repeat) { i--; continue; }
            Hand.Add(c);
            var cui = GameObject.Find(string.Format("Card{0}", Hand.Count));
            var cuit = cui.transform.FindChild("Text").GetComponent<Text>();
            cuit.text = c.Symbol;
        }
    }

    private void StartTurn(Action next)
    {
        Draw();
        next();
    }

    private void StartMove(Action next)
    {
        Tween.FromTo<float>(Timer, f => Timer.value = f, 1.0f, 0.0f, 4.0f)
            .Curve(null)
            .OnComplete(next);
    }

    public Card GetHandCard(string sym)
    {
        for (int i = 0; i < Hand.Count; i++)
        {
            if (Hand[i].Symbol == sym) { return Hand[i]; }
        }
        return null;
    }

    public void ChooseIndex(int i)
    {
        Selection = Hand[i];
    }

    public TreeNode GetNodeById(string id)
    {
        var parts = id.Split('-');
        TreeNode tn = Root;
        for(int i=0;i<parts.Length;i++)
        {
            var p = parts[i];
            tn = tn.Children[int.Parse(p)];
        }
        return tn;
    }

    public void Build(Card c)
    {
        var prefab = Resources.Load<GameObject>("Card");
        var ci = Object.Instantiate<GameObject>(prefab);
        var n = Cursor.Grow(c);

        var id = n.Id;
        //Debug.Log("Node ID " + id);
        ci.transform.SetParent(Tree.Find(id));

        var rt = ci.GetComponent<RectTransform>();
        var cit = ci.GetComponentInChildren<Text>();
        cit.text = c.Symbol;
        rt.offsetMax = new Vector2(30f, 30f);
        rt.offsetMin = new Vector2(-30f, -30f);
    }

    private void EndMove(Action next)
    {
        if (Selection != null) 
        {
            Build(Selection);
        }
        Selection = null;
        next();
    }

    private void EndTurn(Action next)
    {
        next();
    }

    private void NextPhase()
    {
        var cp = Phases[Phase++ % Phases.Length];
        var np = Phases[Phase % Phases.Length];
        cp(NextPhase);
    }

    public void Start()
    {
        Pool = new Deck(250);
        Root = Cursor = new TreeNode();
        Root.Valence = 1;
        Phases = new Action<Action>[]{ StartTurn, StartMove, EndMove, EndTurn };
        Phase = 0;
        NextPhase();
    }
}
