﻿using System.ComponentModel;

#nullable enable

[AutoCodeGenerator.AutoCodeNullable(Type = AutoCodeGenerator.NullableEnum.Enable)]
[AutoCodeGenerator.AutoCodeClassModes(Id = "a1", Namespace = "bbb", Suffix = "ccc", Summary = "eeeeeeeeeeeeeeeeee", Modifier = AutoCodeGenerator.AccessibilityEnum.Internal, ClassStyle = AutoCodeGenerator.ClassStyleEnum.Record)]
[AutoCodeGenerator.AutoCodeClassModes(Id = "a2", Namespace = "ccc", InheritAttribute = true, ClassStyle = AutoCodeGenerator.ClassStyleEnum.Inpc,InheritType =typeof(AutoCodeGenerator.InpcPropertyBindableBaseAbstract))]
public interface Point1
{
    [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "a1", "a2" })]
    public double Xxxxx { get; set; }
}

[AutoCodeGenerator.AutoCodeNullable(Type = AutoCodeGenerator.NullableEnum.Enable)]
[AutoCodeGenerator.AutoCodeClassModes(Id = "a1", Namespace = "bbb", Suffix = "ccc", Summary = "eeeeeeeeeeeeeeeeee", Modifier = AutoCodeGenerator.AccessibilityEnum.Internal)]
[AutoCodeGenerator.AutoCodeClassModes(Id = "a2", Namespace = "ccc", InheritAttribute = true)]
public interface Point2: eeee
{
    [AutoCodeGenerator.AutoCodeProperty(Id = "a1")]
    [AutoCodeGenerator.AutoCodeProperty(Id = "a2")]
    double Y { get; set; }

    [AutoCodeGenerator.AutoCodeProperty(Id = "a1")]
    [AutoCodeGenerator.AutoCodeProperty(Id = "a2")]
    string? Name { get; set; }

    [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "a1", "a2" })]
    [DisplayName("sssssssssssssssss")]
    //[AutoCodeGenerator.CustomAttribute(Ids = ["a1"], Attributes = ["dddddddddddddddddddddddddddddddddd"])]
    public long? ProjectId { get; set; }
}
public interface eeee: eeee1
{

}
//[AutoCodeGenerator.CustomAttribute(Ids = ["a2"], Attributes = ["dddddddddddddddddddddddddddddddddd"])]
public interface eeee1
{
    [AutoCodeGenerator.AutoCodeProperty(Id = "a1")]
    [AutoCodeGenerator.AutoCodeProperty(Id = "a2")]
    string? Name111 { get; set; }
}