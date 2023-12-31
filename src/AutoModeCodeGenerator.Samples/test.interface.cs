﻿using System.ComponentModel;

#nullable enable

[AutoCodeGenerator.AutoCodeNullable(Type = AutoCodeGenerator.NullableEnum.Enable)]
[AutoCodeGenerator.AutoCodeClassModes(Id = "a1", Namespace = "bbb", Suffix = "ccc", Summary = "eeeeeeeeeeeeeeeeee", Modifier = AutoCodeGenerator.AccessibilityEnum.Internal)]
[AutoCodeGenerator.AutoCodeClassModes(Id = "a2", Namespace = "ccc", InheritAttribute = true)]
public interface Point1
{
    [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "a1", "a2" })]
    public double X { get; set; }
}

[AutoCodeGenerator.AutoCodeNullable(Type = AutoCodeGenerator.NullableEnum.Enable)]
[AutoCodeGenerator.AutoCodeClassModes(Id = "a1", Namespace = "bbb", Suffix = "ccc", Summary = "eeeeeeeeeeeeeeeeee",Modifier = AutoCodeGenerator.AccessibilityEnum.Internal)]
[AutoCodeGenerator.AutoCodeClassModes(Id = "a2", Namespace = "ccc", InheritAttribute=true)]
public abstract class Point2 //: Point1
{
    [AutoCodeGenerator.AutoCodeProperty(Id = "a1")]
    [AutoCodeGenerator.AutoCodeProperty(Id = "a2")]
    double Y { get; set; }

    [AutoCodeGenerator.AutoCodeProperty(Id = "a1")]
    [AutoCodeGenerator.AutoCodeProperty(Id = "a2")]
    string? Name { get; set; }

    [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "a1", "a2" })]
    [DisplayName("sssssssssssssssss")]
    public long? ProjectId { get; set; } = 300;
}
