using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

public enum VarType
{
    Int,
    Double,
    String,
    Bool
}

public class Var
{
    // Добавляем поле для определения типа
    public VarType Type { get; private set; }

    // Поля для хранения значений
    public int _intValue;
    public double _floatValue;
    public string _stringValue;
    public bool _boolValue;

    public Var(int Value)
    {
        Type = VarType.Int;

        _intValue = Value;
        _floatValue = 0.0;
        _boolValue = false;
        _stringValue = "";
    }
    public Var(double Value)
    {
        Type = VarType.Double;

        _intValue = 0;
        _floatValue = Value;
        _boolValue = false;
        _stringValue = "";
    }
    public Var(bool Value)
    {
        Type = VarType.Bool;

        _intValue = 0;
        _floatValue = 0.0;
        _boolValue = Value;
        _stringValue = "";
    }
    public Var(string Value)
    {
        Type = VarType.String;

        _intValue = 0;
        _floatValue = 0.0;
        _boolValue = false;
        _stringValue = Value;
    }
    // Получение значения в зависимости от типа
    public object GetValue() => Type switch
    {
        VarType.Int => _intValue,
        VarType.Double => _floatValue,
        VarType.Bool => _boolValue,
        VarType.String => _stringValue,
        _ => throw new ArgumentOutOfRangeException(nameof(Type), $"Unexpected type: {Type}")
    };

    public override string ToString() => Type switch
    {
        VarType.Int => _intValue.ToString(),
        VarType.Double => _floatValue.ToString(),
        VarType.Bool => _boolValue.ToString(),
        VarType.String => _stringValue,
        _ => ""
    };

    // Неявные преобразования для удобства
    public static implicit operator Var(int value) => new Var(value);
    public static implicit operator Var(double value) => new Var(value);
    public static implicit operator Var(string value) => new Var(value);
    public static implicit operator Var(bool value) => new Var(value);
}