using UnityEngine;
using System.Collections.Generic;

public class BundleData 
{
    //asset bundle's md5, used to hot update
    public string               md5 = "";

    //asset bundle's name
    public string               name = "";

    //asset bundle depend list
    public List<string>         dependAssets = new List<string>();
}
