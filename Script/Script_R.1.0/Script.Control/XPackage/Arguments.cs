using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XPackage
{
    public enum ShapeType
    {
        String = 0,
        VXml = 1,
        Xml = 2
    }
    public enum ShapeText
    {
        Original = 0,
        LowerXName = 1,
        UpperXName = 2
    }
    public enum XType
    {
        //Null = 0,
        Function = 1,
        //Bool = 2,
        //Number = 3,
        String = 4
    }
    enum XPackPathType
    {
        FindEqualsParamsName = 0,
        EmptyFindNext = 1,
        SelectAllChild = 2,
        AttributeValue = 3,
        ConditionResult = 4
    }

    public enum FindMode
    {
        Up = 0,
        Down = 1
    }
}
