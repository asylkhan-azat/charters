namespace Charters.Sim.Hexes;

public readonly record struct HexAddress(int Q, int R)
{
    public int S => -Q - R;

    public static readonly HexAddress[] Directions =
    [
        new(1, 0),
        new(1, -1),
        new(0, -1),
        new(-1, 0),
        new(-1, 1),
        new(0, 1)
    ];

    public static HexAddress operator +(HexAddress a, HexAddress b)
    {
        return new HexAddress(a.Q + b.Q, a.R + b.R);
    }

    public static HexAddress operator -(HexAddress a, HexAddress b)
    {
        return new HexAddress(a.Q - b.Q, a.R - b.R);
    }

    public static HexAddress operator *(HexAddress a, int k)
    {
        return new HexAddress(a.Q * k, a.R * k);
    }

    public HexAddress Neighbor(int direction)
    {
        if (direction < 0 || direction >= Directions.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        return this + Directions[direction];
    }

    public static int Distance(HexAddress a, HexAddress b)
    {
        return (Math.Abs(a.Q - b.Q) + Math.Abs(a.R - b.R) + Math.Abs(a.S - b.S)) / 2;
    }

    public static HexAddress Round(double q, double r)
    {
        var s = -q - r;
        var rq = (int)Math.Round(q, MidpointRounding.AwayFromZero);
        var rr = (int)Math.Round(r, MidpointRounding.AwayFromZero);
        var rs = (int)Math.Round(s, MidpointRounding.AwayFromZero);
        var qDiff = Math.Abs(rq - q);
        var rDiff = Math.Abs(rr - r);
        var sDiff = Math.Abs(rs - s);

        if (qDiff > rDiff && qDiff > sDiff)
        {
            rq = -rr - rs;
        }
        else if (rDiff > sDiff)
        {
            rr = -rq - rs;
        }

        return new HexAddress(rq, rr);
    }

    public static List<HexAddress> Line(HexAddress a, HexAddress b)
    {
        var distance = Distance(a, b);
        List<HexAddress> results = new(distance + 1);
        var q1 = a.Q + 1e-6;
        var r1 = a.R + 1e-6;
        var q2 = b.Q + 1e-6;
        var r2 = b.R + 1e-6;

        for (var i = 0; i <= distance; i++)
        {
            var t = distance == 0 ? 0.0 : (double)i / distance;
            results.Add(Round(Lerp(q1, q2, t), Lerp(r1, r2, t)));
        }

        return results;
    }

    public static List<HexAddress> Ring(HexAddress center, int radius)
    {
        if (radius < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be at least 1.");
        }

        List<HexAddress> results = new(6 * radius);
        var hex = center + Directions[4] * radius;
        for (var direction = 0; direction < Directions.Length; direction++)
        {
            for (var step = 0; step < radius; step++)
            {
                results.Add(hex);
                hex = hex.Neighbor(direction);
            }
        }

        return results;
    }

    public static List<HexAddress> Range(HexAddress center, int radius)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(radius);

        List<HexAddress> results = new(3 * radius * (radius + 1) + 1);
        for (var q = -radius; q <= radius; q++)
        {
            var rMin = Math.Max(-radius, -q - radius);
            var rMax = Math.Min(radius, -q + radius);
            for (var r = rMin; r <= rMax; r++)
            {
                results.Add(center + new HexAddress(q, r));
            }
        }

        return results;
    }

    public static List<HexAddress> Spiral(HexAddress center, int radius)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(radius);

        List<HexAddress> results = new(3 * radius * (radius + 1) + 1) { center };
        for (var i = 1; i <= radius; i++)
        {
            results.AddRange(Ring(center, i));
        }

        return results;
    }

    private static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * t;
    }
}