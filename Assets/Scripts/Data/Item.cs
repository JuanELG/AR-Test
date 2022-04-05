using System;
using Antlr4.Runtime.Misc;

/// <summary>
/// class containing all the data structure necessary to download the information of the elements to be displayed.
/// </summary>
[Serializable]
public class Item
{
    public string uid;
    public string id;
    public string name;

    public string creatorId;
    public bool isActive;
    public string description;
    public string[] tag;
    public string[] format;
    public string category;
    public string subcategory;
    public string addedBy;
    public DateTime dateOfAdition;
    public DateTime dateOfLastEdit;

    public string[] imgArray;
    public string sketchfabCode;

    public PolyFiles HighPoly;
    public PolyFiles LowPoly;

    public string zipPath;
    public ObjProps[] objs;

    public float price;
    public int likes;
    public int saved;
    public int shared;
    public int views;
    public int toCart;

    public ValidProduct[] validProducts;
    public BuyProduct[] buyProducts;
    public float timesUsed;
}
 
/// <summary>
/// complementary classes with the necessary information to download the data of the elements.
/// </summary>
#region Additional_Data
[Serializable]
public class ValidProduct
{
    public string productId;
    public string businessId;
}
        
[Serializable]
public class BuyProduct
{
    public string buyURL;
    public string businessId;
}
        
[Serializable]
public class ObjProps
{
    
}
        
[Serializable]
public class PolyFiles
{
    public string Model;
    public string Collider;
    public string DiffuseMap;
    public string MetallicMap;
    public string Shadow;
    public string NormalMap;
    public string AmbientOcclusionMap;
    public string IOR;
    public string Glossiness;
    public string Roughness;
    public string Specular;
    public string Displacement;
    public string Transparency;
    public string Emission;
    public string Music;
    public bool realTime;
}
#endregion
