# ConstrainCSharp
This is a repository for constraining C# syntax so that C# can express clearer meaning and improve code readability and maintainability.

# What is ConstrainCSharp?
This library contains some C# Attributes, such as `OnlyOneAssignmentStatement`, which you can use to limit C# private fields to only one assignment statement.

# Why use it?  
<!--当公司里一个C#项目，需要转手多人，而仅仅使用常规C#语法转达的意思可能还不够清晰时。此时你能使用其提供的特性表达更加清晰的意思并限制其他人的不合理操作。就像下面这样-->
When a C# project in the company needs to change hands among many people, the meaning conveyed by just using regular C# syntax may not be clear enough. At this time, you can use the `OnlyOneAssignmentStatement` Attribute it provides to express clearer meaning and limit other people's unreasonable operations. Like below：
```
using ConstrainCSharp.Attributes;

namespace TestConstrainCSharp
{

    public class Boy
    {
        /// <summary>
        /// Private fields marked with the OnlyOneAssignmentStatement attribute
        /// </summary>
        [OnlyOneAssignmentStatement]
        private int _age;

        /// <summary>
        /// The functions you defined when you wrote the code have already determined that you don’t want _age to be modified in future code.
        /// </summary>
        /// <param name="age">age</param>
        public void SetAge(int age) {
            _age = age;
        }

        /// <summary>
        /// For functions added in the future that attempt to modify _age, Visual Studio will perform static code analysis and display a red wavy line in the assignment statement below as an error message, and the code will not compile until the new assignment statement is deleted.
        /// </summary>
        /// <param name="value">value</param>
        public void AddAge(int value) {
            _age += value; // receive static code analysis warnings when writing assignment statements here
        }
    }
}
```
