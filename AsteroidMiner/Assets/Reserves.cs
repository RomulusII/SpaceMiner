public class Reserves
{
    public float Iron { get; set; } = 0;
    public float Uranium { get; set; } = 0;
    public float Aluminium { get; set; } = 0;
    public float Power { get; set; } = 0;

    public float IronPercent => Iron / TotalOre;
    public float UraniumPercent => Uranium / TotalOre;
    public float AluminiumPercent => Aluminium / TotalOre;

    public float TotalOre => Iron + Uranium + Aluminium;

    public Reserves MineChunkOfOre(float totalAmount)
    {
        var ores = new Reserves()
        {
            Iron = totalAmount * IronPercent,
            Uranium = totalAmount * UraniumPercent,
            Aluminium = totalAmount * AluminiumPercent
        };

        Subtract(ores);

        return ores;
    }

    public void Add(Reserves reserves)
    {
        Iron += reserves.Iron;
        Uranium += reserves.Uranium;
        Aluminium += reserves.Aluminium;
        Power += reserves.Power;
    }

    public void Subtract(Reserves reserves)
    {
        Iron -= reserves.Iron;
        Uranium -= reserves.Uranium;
        Aluminium -= reserves.Aluminium;
        Power -= reserves.Power;
    }
}
