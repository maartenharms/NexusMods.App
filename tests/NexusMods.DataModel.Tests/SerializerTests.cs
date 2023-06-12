using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NexusMods.DataModel.JsonConverters;
using NexusMods.DataModel.JsonConverters.ExpressionGenerator;


namespace NexusMods.DataModel.Tests;

public class SerializerTests
{
    private readonly IServiceProvider _provider;

    public SerializerTests(IServiceProvider provider)
    {
        _provider = provider;
    }

    [Fact]
    public void CanSerializeASimpleType()
    {
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new ConcreteConverterGenerator<BasicClass>(_provider));

        var json = JsonSerializer.Serialize(new BasicClass { SomeString = "One", SomeOtherString = "Two", BaseString = "Base" }, opts);

        json.Should().Contain("\"$type\":\"a93d7f88-c4da-42af-8efe-eaf168e33ebf|BasicClass\"", "data is serialized with a type hint");

        var data = JsonSerializer.Deserialize<BasicClass>(json, opts)!;

        data.SomeString.Should().Be("One", "SomeString should be deserialized");
        data.SomeOtherString.Should().Be(null, "SomeOtherString has a JsonIgnore attribute");
    }

    [Fact]
    public void CanSerializeAnAdvancedType()
    {
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new ConcreteConverterGenerator<BasicClass>(_provider));
        opts.Converters.Add(new ConcreteConverterGenerator<AdvancedClass>(_provider));

        var originalData = new AdvancedClass()
        {
            SubClass = new BasicClass { SomeString = "Some", SomeOtherString = "String", BaseString = "Base2" },
            ListOfInts = new List<int> { 4, 2 },
            SomeInt = 42,
            BaseString = "Base"
        };

        var json = JsonSerializer.Serialize(originalData, opts);

        json.Should().Contain("\"$type\":\"fefd32f4-d430-4cfc-9119-46662aa8036b|AdvancedClass\"", "data is serialized with a type hint");
        json.Should().Contain("\"$type\":\"a93d7f88-c4da-42af-8efe-eaf168e33ebf|BasicClass\"", "sub data is serialized with a type hint");

        originalData.SubClass.SomeOtherString = null!;
        var data = JsonSerializer.Deserialize<AdvancedClass>(json, opts);

        data.Should().BeEquivalentTo(originalData);
    }

    [Fact]
    public void CanSerializeViaInterfaceTypes()
    {
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new AbstractClassConverterFactory<IInterface>(_provider));

        var json = JsonSerializer.Serialize<IInterface>(new BasicClass { SomeString = "One", SomeOtherString = "Two", BaseString = "Base" }, opts);

        json.Should().Contain("\"$type\":\"a93d7f88-c4da-42af-8efe-eaf168e33ebf|BasicClass\"", "data is serialized with a type hint");

        var data = (BasicClass)JsonSerializer.Deserialize<IInterface>(json, opts)!;

        data.SomeString.Should().Be("One", "SomeString should be deserialized");
        data.SomeOtherString.Should().Be(null, "SomeOtherString has a JsonIgnore attribute");

    }


    [Fact]
    public void CanSerializeGenericInterfaace()
    {
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new AbstractClassConverterFactory<IGeneric<int, string>>(_provider));

        IGeneric<int, string> data1 = new Specific<int, string>()
        {
            T1Val = 42,
            T2Val = "forty-two"
        };
        IGeneric<int, string> data2 = new Specific2<int, string>()
        {
            T1Val = 42,
            T2Val = "forty-three",
            T3Val = 43
        };

        foreach (var row in new[] { data1, data2 })
        {
            var json = JsonSerializer.Serialize(row, opts);
            json.Should().Contain("$type", "because the value supports polymorphism");

            var data = JsonSerializer.Deserialize<IGeneric<int, string>>(json, opts);

            data.Should().BeEquivalentTo(row);
        }
    }

    [Fact]
    public void CanNestGenericInterfaces()
    {
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new AbstractClassConverterFactory<IGeneric<int, string>>(_provider));
        opts.Converters.Add(new ConcreteConverterGenerator<NestedGeneric>(_provider));

        IGeneric<int, string> data1 = new Specific<int, string>()
        {
            T1Val = 42,
            T2Val = "forty-two"
        };
        IGeneric<int, string> data2 = new Specific2<int, string>()
        {
            T1Val = 42,
            T2Val = "forty-three",
            T3Val = 43
        };

        var row = new NestedGeneric()
        {
            SomeInt = 4,
            SomeSet = ImmutableHashSet<IGeneric<int, string>>.Empty.Add(data1).Add(data2)
        };

        var json = JsonSerializer.Serialize(row, opts);
        json.Should().Contain("$type", "because the value supports polymorphism");

        var data = JsonSerializer.Deserialize<NestedGeneric>(json, opts);

        data.Should().BeEquivalentTo(row);
    }
    
    [Fact]
    public void CanSerializeCollectionOfInterfaces()
    {
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new AbstractClassConverterFactory<IGeneric<int, string>>(_provider));
        opts.Converters.Add(new ConcreteConverterGenerator<NestedGeneric>(_provider));
        
        var row = new Specific<int, ImmutableList<IGeneric<int, string>>>()
        {
            T1Val = 42, 
            T2Val = new List<IGeneric<int, string>>
            {
                new Specific<int, string> {T1Val = 22, T2Val = "test"}
            }.ToImmutableList()
        };
        
        var json = JsonSerializer.Serialize(row, opts);

        var data = JsonSerializer.Deserialize<Specific<int, ImmutableList<IGeneric<int, string>>>>(json, opts);

        data.Should().BeEquivalentTo(row);
    }

    [Fact]
    public void CanInjectProperties()
    {
        var data = new InjectableClass { SomeInt = 42 };
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new ConcreteConverterGenerator<InjectableClass>(_provider));

        var json = JsonSerializer.Serialize(data, opts);
        json.Should().Contain("$type", "because the value supports polymorphism");

        var data2 = JsonSerializer.Deserialize<InjectableClass>(json, opts)!;
        data2.TypeFinder.Should().NotBeNull("because the type finder was injected");
    }
    
    

    [Fact]
    public void CanSerializeEnums()
    {
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new ConcreteConverterGenerator<EnumClass>(_provider));

        var data = new EnumClass()
        {
            SomeString = "Data",
            SomeEnum = new[] { MyEnum.One, MyEnum.Two }
        };

        var json = JsonSerializer.Serialize(data, opts);
        json.Should().Contain("$type", "because the value supports polymorphism");

        var data2 = JsonSerializer.Deserialize<EnumClass>(json, opts)!;

        data2.Should().BeEquivalentTo(data);
    }



    [JsonTypeId<EnumClass>("4E8E7E72-9515-412E-9435-8645F2AC0D6F")]
    public class EnumClass
    {
        public string SomeString { get; set; } = null!;
        public MyEnum[] SomeEnum { get; set; } = Array.Empty<MyEnum>();
    }
    public enum MyEnum
    {
        One,
        Two,
        Three
    }

    [JsonTypeId<InjectableClass>("AFAE6CE2-C0B6-416F-82D0-FCAEBDAB09E5")]
    public class InjectableClass
    {
        public int SomeInt { get; set; }

        [JsonInjected]
        public ITypeFinder? TypeFinder { get; set; }
    }

    public interface IInterface
    {
        public string BaseString { get; init; }
    }

    public abstract class ABase : IInterface
    {
        public required string BaseString { get; init; } = "";
    }

    [JsonTypeId<BasicClass>("A93D7F88-C4DA-42AF-8EFE-EAF168E33EBF")]
    public class BasicClass : ABase
    {
        public string SomeString { get; set; } = "";

        [JsonIgnore]
        public string? SomeOtherString { get; set; }
    }

    [JsonTypeId<AdvancedClass>("FEFD32F4-D430-4CFC-9119-46662AA8036B")]
    public class AdvancedClass : ABase
    {
        public int SomeInt { get; set; }
        // ReSharper disable once CollectionNeverQueried.Global
        public List<int> ListOfInts { get; set; } = new();
        public BasicClass SubClass { get; set; } = new() { BaseString = "" };
    }

    [JsonTypeId<NestedGeneric>("A64AC1A5-271F-4E4D-B443-8D22572DA9D6")]
    public class NestedGeneric
    {
        public int SomeInt { get; set; }

        public ImmutableHashSet<IGeneric<int, string>> SomeSet { get; set; } = ImmutableHashSet.Create<IGeneric<int, string>>();
    }

    public interface IGeneric<T1, T2>
    {
        public T1 T1Val { get; }
        public T2 T2Val { get; }

    }

    [JsonTypeId("54825EDD-860A-4BF7-9281-9EC594726A78", "Specific")]
    public class Specific<T1, T2> : IGeneric<T1, T2>
    {
        public required T1 T1Val { get; init; }
        public required T2 T2Val { get; init; }
    }

    [JsonTypeId("35323E5E-3D46-43B1-AADF-D0D96BEBA404", "Specific2")]
    public class Specific2<T1, T2> : IGeneric<T1, T2>
    {
        public required T1 T1Val { get; init; }
        public required T2 T2Val { get; init; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public required int T3Val { get; init; }
    }
}
