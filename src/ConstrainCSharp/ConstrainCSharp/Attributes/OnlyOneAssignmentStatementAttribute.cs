using System;

namespace ConstrainCSharp.Attributes {
    /// <summary>
    /// 使私有字段只存在一个赋值语句
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OnlyOneAssignmentStatementAttribute : Attribute { }
}