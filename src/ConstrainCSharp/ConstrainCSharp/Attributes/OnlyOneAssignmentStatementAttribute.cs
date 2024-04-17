using System;

namespace ConstrainCSharp.Attributes {
    /// <summary>
    /// Make the private field only have one assignment statement
    /// 使私有字段只存在一个赋值语句
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OnlyOneAssignmentStatementAttribute : Attribute { }
}