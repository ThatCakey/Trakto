using Avalonia.Input;
using System;
using System.Reflection;

foreach (var prop in typeof(DragEventArgs).GetProperties())
{
    Console.WriteLine($"{prop.PropertyType.Name} {prop.Name}");
}
