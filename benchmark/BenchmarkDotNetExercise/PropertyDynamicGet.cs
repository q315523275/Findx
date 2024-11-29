using BenchmarkDotNet.Attributes;
using BenchmarkDotNetExercise.Models;
using Findx.Expressions;
using Findx.Utilities;
using MassTransit;

namespace BenchmarkDotNetExercise;

[MemoryDiagnoser]
public class PropertyDynamicGet
{
    private readonly SysAppInfo _model = new SysAppInfo { Id = NewId.NextSequentialGuid(), Code = "test", Name = "史册" };
    private readonly Type _type = typeof(SysAppInfo);
    private readonly string _propertyName = "Name";
    private readonly PropertyDynamicGetter<SysAppInfo> _propertyDynamicGetter = new();
    private readonly Func<SysAppInfo, object> _expressionGetter;
    private readonly Func<SysAppInfo, object> _emitGetter;
    
    public PropertyDynamicGet()
    {
        _expressionGetter = PropertyUtility.ExpressionGetter<SysAppInfo>(_propertyName);
        _emitGetter = PropertyUtility.EmitGetter<SysAppInfo>(_propertyName);
        PropertyValueGetter<SysAppInfo>.GetPropertyValue<string>(_type, _model, _propertyName);
    }
    
    [Benchmark]
    public void Model_Get()
    {
        _ = _model.Name;
    }
    
    [Benchmark]
    public void PropertyDynamicGetter()
    {
        _propertyDynamicGetter.GetPropertyValue(_model, _propertyName);
    }
    
    [Benchmark]
    public void ClassExpressionGetter()
    {
        PropertyValueGetter<SysAppInfo>.GetPropertyValue<string>(_type, _model, _propertyName);
    }
    
    [Benchmark]
    public void InstanceExpressionGetter()
    {
        _expressionGetter.Invoke(_model);
    }
    
    [Benchmark]
    public void InstanceEmitGetter()
    {
        _emitGetter.Invoke(_model);
    }
}