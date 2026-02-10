using Godot;
using System;
using System.Collections.Generic;

public class SaveData
{
    public int CheckPointID { get; set; }
    public int MaxPoints { get; set; }
    public List<int> CollectedDotsID { get; set; }
}