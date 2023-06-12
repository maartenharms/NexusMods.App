using NexusMods.DataModel.Abstractions;

namespace NexusMods.DataModel.JsonConverters;

/// <summary>
/// Defines a GUID for a <see cref="Entity"/> type, this will be used to determine
/// the type of the entity when deserializing (and to support polymorphism).
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class JsonTypeIdAttribute : Attribute
{
    /// <summary>
    /// The unique identifier for the <see cref="Entity"/> type.
    /// </summary>
    public Guid Guid { get; }
    
    /// <summary>
    /// The name of the <see cref="Entity"/> type, this is used only for debugging purposes.
    /// </summary>
    public string TypeName { get; }
    
    /// <summary>
    /// The encoded value of the GUID and the type name.
    /// </summary>
    public string FullId { get; }

    /// <summary>
    /// Just the ID part of the Bytes property.
    /// </summary>
    public string Prefix { get; set; }


    
    /// <summary>
    /// Define the GUID for a <see cref="Entity"/> type. The typeName
    /// is used only for debugging purposes and has no impact on the
    /// serialization process.
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="typeName"></param>
    public JsonTypeIdAttribute(string guid, string typeName)
    {
        Guid = Guid.Parse(guid);
        TypeName = typeName;

        Prefix = Guid.ToString();
        FullId = $"{Prefix}|{typeName}";
    }

}

/// <summary>
/// Defines a GUID for a <see cref="Entity"/> type, this will be used to determine
/// the type of the entity when deserializing (and to support polymorphism). The
/// typeName is derived from the type of the generic parameter.
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class JsonTypeIdAttribute<T> : JsonTypeIdAttribute
{
    /// <summary>
    /// Define the GUID for a <see cref="Entity"/> type.
    /// </summary>
    /// <param name="guid"></param>
    public JsonTypeIdAttribute(string guid) : base(guid, typeof(T).Name)
    {
    }
}


