using System;
using System.Collections.Generic;

[Serializable]
public class PalletData
{
    public int id;
    public int[] pos;
    public string state;
}

[Serializable]
public class AgvData
{
    public int id;
    public int[] pos;
    public string state;
    public int? pallet_id;
    public int orientation;
}

[Serializable]
public class RackData
{
    public int id;
    public int[] pos;
    public int? pallet_id;
    public string state;
}

[Serializable]
public class DockData
{
    public int id;
    public int[] pos;
    public int? pallet_id;
    public string state;
}

[Serializable]
public class ProductionLineData
{
    public int id;
    public int[] pos;
    public int? pallet_id;
    public string state;
}

[Serializable]
public class PersonData
{
    public int id;
    public int[] pos;
    public string state;
    public int orientation;
}

[Serializable]
public class FrameData
{
    public int idx;
    public List<AgvData> avgs;
    public List<RackData> racks;
    public List<DockData> docks;
    public List<ProductionLineData> productionLines;
    public List<PalletData> pallets;
    public List<PersonData> persons;
}

