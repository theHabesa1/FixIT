using System.Collections.Generic;
using UnityEngine;

// A product order / stock item: a product plus a list of spec values.
// Used by buyer customers and the Back Store mini-game.
public class Order
{
    public string product;     // "Laptop" | "Desktop PC" | "CPU"
    public string[] labels;    // e.g. { "CPU", "RAM", "SSD" }
    public string[] values;    // e.g. { "i7", "16GB", "512GB" }
    public int price;

    public bool Matches(Order o)
    {
        if (o == null || o.product != product || o.values.Length != values.Length) return false;
        for (int i = 0; i < values.Length; i++)
            if (values[i] != o.values[i]) return false;
        return true;
    }

    public string SpecLine()
    {
        var parts = new string[values.Length];
        for (int i = 0; i < values.Length; i++) parts[i] = labels[i] + ": " + values[i];
        return string.Join("   ", parts);
    }

    // Compact values only (for small world-space shelf labels).
    public string ValuesLine() => string.Join(" / ", values);
}

// Generates random orders and a shelf of stock that contains exactly one match.
public static class StockGen
{
    class Product
    {
        public string name;
        public string[] labels;
        public string[][] pools;
        public int minPrice, maxPrice;
    }

    static readonly Product[] Products =
    {
        new Product {
            name = "Laptop", labels = new[] { "CPU", "RAM", "SSD" },
            pools = new[] {
                new[] { "i5", "i7", "Ryzen 5", "Ryzen 7" },
                new[] { "8GB", "16GB", "32GB" },
                new[] { "256GB", "512GB", "1TB" },
            },
            minPrice = 600, maxPrice = 1400,
        },
        new Product {
            name = "Desktop PC", labels = new[] { "GPU", "RAM", "PSU" },
            pools = new[] {
                new[] { "GTX 1660", "RTX 3060", "RTX 4070" },
                new[] { "16GB", "32GB", "64GB" },
                new[] { "550W", "750W", "850W" },
            },
            minPrice = 700, maxPrice = 1800,
        },
        new Product {
            name = "CPU", labels = new[] { "Model", "Cores", "Clock" },
            pools = new[] {
                new[] { "i5-12400", "i7-13700", "Ryzen5 5600", "Ryzen7 7700" },
                new[] { "6", "8", "12", "16" },
                new[] { "3.6GHz", "4.2GHz", "5.0GHz" },
            },
            minPrice = 200, maxPrice = 650,
        },
    };

    public static Order RandomOrder()
    {
        var p = Products[Random.Range(0, Products.Length)];
        return MakeOrder(p);
    }

    static Order MakeOrder(Product p)
    {
        var vals = new string[p.pools.Length];
        for (int i = 0; i < p.pools.Length; i++) vals[i] = p.pools[i][Random.Range(0, p.pools[i].Length)];
        int price = Mathf.RoundToInt(Random.Range(p.minPrice, p.maxPrice) / 10f) * 10;
        return new Order { product = p.name, labels = p.labels, values = vals, price = price };
    }

    // Returns a shuffled shelf containing the wanted item + (count-1) distractors.
    public static List<Order> BuildStock(Order want, int count)
    {
        var prod = System.Array.Find(Products, x => x.name == want.product);
        var list = new List<Order> { want };
        int guard = 0;
        while (list.Count < count && guard++ < 500)
        {
            Order d = (Random.value < 0.75f) ? Distractor(prod, want) : MakeOrder(Products[Random.Range(0, Products.Length)]);
            // avoid accidental duplicate match of the wanted item
            if (d.Matches(want)) continue;
            list.Add(d);
        }
        // shuffle
        for (int i = list.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (list[i], list[j]) = (list[j], list[i]); }
        return list;
    }

    // Same product as 'want' but with 1+ spec values changed.
    static Order Distractor(Product p, Order want)
    {
        var vals = (string[])want.values.Clone();
        int changes = Random.Range(1, vals.Length + 1);
        var idxs = new List<int>();
        for (int i = 0; i < vals.Length; i++) idxs.Add(i);
        for (int c = 0; c < changes && idxs.Count > 0; c++)
        {
            int pick = idxs[Random.Range(0, idxs.Count)];
            idxs.Remove(pick);
            string cur = vals[pick];
            var pool = p.pools[pick];
            // pick a different value
            string nv = cur;
            for (int t = 0; t < 8 && nv == cur; t++) nv = pool[Random.Range(0, pool.Length)];
            vals[pick] = nv;
        }
        int price = Mathf.RoundToInt(Random.Range(p.minPrice, p.maxPrice) / 10f) * 10;
        return new Order { product = p.name, labels = p.labels, values = vals, price = price };
    }

    public static string Icon(string product) => product switch
    {
        "Laptop"     => "laptop",
        "Desktop PC" => "pc",
        "CPU"        => "tower",
        _            => "laptop",
    };
}
