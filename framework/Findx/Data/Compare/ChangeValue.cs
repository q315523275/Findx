namespace Findx.Data.Compare;

/// <summary>
///     变更值
/// </summary>
public class ChangeValue
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="description"></param>
    /// <param name="originalValue"></param>
    /// <param name="newValue"></param>
    public ChangeValue( string propertyName, string description, string originalValue, string newValue ) 
    {
        PropertyName = propertyName;
        Description = description;
        OriginalValue = originalValue;
        NewValue = newValue;
    }
    
    /// <summary>
    ///     属性名
    /// </summary>
    public string PropertyName { get; }
    
    /// <summary>
    ///     描述
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    ///     原始值
    /// </summary>
    public string OriginalValue { get; }
    
    /// <summary>
    ///     新值
    /// </summary>
    public string NewValue { get; }

    /// <summary>
    ///     输出变更信息
    /// </summary>
    public override string ToString()
    {
        return $"{PropertyName}({Description}),原始值:{OriginalValue},新值:{NewValue}";
    }
}