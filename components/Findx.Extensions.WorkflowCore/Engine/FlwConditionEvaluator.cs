using System.Globalization;
using System.Text;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     条件表达式求值器（FlowLong 模型中的 condition 表达式）
///     <para>支持：&amp;&amp; / || / 括号 / == != &gt;= &lt;= &gt; &lt; / contains / not contains / empty() / notEmpty() / 布尔字面量 / 变量取值</para>
/// </summary>
public static class FlwConditionEvaluator
{
    /// <summary>
    ///     求值
    /// </summary>
    /// <param name="expression">条件表达式，为空视为 true（默认分支）</param>
    /// <param name="variables">流程变量</param>
    public static bool Evaluate(string expression, IReadOnlyDictionary<string, object> variables)
    {
        if (string.IsNullOrWhiteSpace(expression)) return true;
        variables ??= new Dictionary<string, object>();

        var exp = expression.Trim();
        while (IsWrappedByParentheses(exp))
        {
            exp = exp.Substring(1, exp.Length - 2).Trim();
        }
        if (exp.Length == 0) return true;

        foreach (var orPart in SplitTopLevel(exp, "||"))
        {
            if (EvaluateAnd(orPart, variables)) return true;
        }
        return false;
    }

    private static bool EvaluateAnd(string expression, IReadOnlyDictionary<string, object> variables)
    {
        foreach (var andPart in SplitTopLevel(expression, "&&"))
        {
            if (!EvaluateTerm(andPart, variables)) return false;
        }
        return true;
    }

    private static bool EvaluateTerm(string term, IReadOnlyDictionary<string, object> variables)
    {
        var exp = term.Trim();
        if (exp.Length == 0) return true;

        //  括号子表达式：交回 Evaluate 处理内部的 || / && / 比较
        if (IsWrappedByParentheses(exp)) return Evaluate(exp, variables);

        //  取反
        if (exp.StartsWith("!") && !exp.StartsWith("!="))
        {
            return !EvaluateTerm(exp.Substring(1).Trim(), variables);
        }

        if (TrySplitOperator(exp, "!=", out var left, out var right))
            return Compare(left, right, variables) != 0;
        if (TrySplitOperator(exp, ">=", out left, out right))
            return Compare(left, right, variables) >= 0;
        if (TrySplitOperator(exp, "<=", out left, out right))
            return Compare(left, right, variables) <= 0;
        if (TrySplitOperator(exp, "==", out left, out right))
            return Compare(left, right, variables) == 0;
        if (TrySplitOperator(exp, ">", out left, out right))
            return Compare(left, right, variables) > 0;
        if (TrySplitOperator(exp, "<", out left, out right))
            return Compare(left, right, variables) < 0;

        //  contains / not contains
        var containsIndex = IndexOfWord(exp, "not contains");
        if (containsIndex > 0)
        {
            var l = exp.Substring(0, containsIndex).Trim();
            var r = exp.Substring(containsIndex + "not contains".Length).Trim();
            return !ContainsValue(ResolveValue(l, variables), ResolveValue(r, variables));
        }
        containsIndex = IndexOfWord(exp, "contains");
        if (containsIndex > 0)
        {
            var l = exp.Substring(0, containsIndex).Trim();
            var r = exp.Substring(containsIndex + "contains".Length).Trim();
            return ContainsValue(ResolveValue(l, variables), ResolveValue(r, variables));
        }

        //  empty() / notEmpty()
        if (exp.StartsWith("empty(", StringComparison.OrdinalIgnoreCase) && exp.EndsWith(")"))
        {
            var inner = exp.Substring(6, exp.Length - 7).Trim();
            return IsEmpty(ResolveValue(inner, variables));
        }
        if (exp.StartsWith("notEmpty(", StringComparison.OrdinalIgnoreCase) && exp.EndsWith(")"))
        {
            var inner = exp.Substring(9, exp.Length - 10).Trim();
            return !IsEmpty(ResolveValue(inner, variables));
        }

        return ToBool(ResolveValue(exp, variables));
    }

    /// <summary>
    ///     解析单个值：${var} / var / 'string' / "string" / 数字 / true / false
    /// </summary>
    private static object ResolveValue(string token, IReadOnlyDictionary<string, object> variables)
    {
        var value = token.Trim();
        if (value.Length == 0) return null;

        if (value.StartsWith("${") && value.EndsWith("}"))
        {
            var key = value.Substring(2, value.Length - 3).Trim();
            return variables.TryGetValue(key, out var v) ? v : null;
        }

        if (value.StartsWith("'") && value.EndsWith("'") && value.Length >= 2)
            return value.Substring(1, value.Length - 2);
        if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
            return value.Substring(1, value.Length - 2);

        if (bool.TryParse(value, out var b)) return b;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;

        //  裸标识符必须是变量；变量缺失视为 null（字符串字面量须加引号）
        return variables.TryGetValue(value, out var variable) ? variable : null;
    }

    /// <summary>
    ///     比较两个值：都能转数值走数值比较，否则字符串比较
    /// </summary>
    private static int Compare(string leftToken, string rightToken, IReadOnlyDictionary<string, object> variables)
    {
        var left = ResolveValue(leftToken, variables);
        var right = ResolveValue(rightToken, variables);

        var leftIsNumber = ToDecimal(left, out var leftDecimal);
        var rightIsNumber = ToDecimal(right, out var rightDecimal);
        if (leftIsNumber && rightIsNumber)
        {
            return leftDecimal.CompareTo(rightDecimal);
        }

        return string.Compare(ToText(left), ToText(right), StringComparison.Ordinal);
    }

    private static bool ContainsValue(object container, object value)
    {
        if (container == null) return false;
        var text = ToText(container);
        var target = ToText(value);
        return text.Contains(target, StringComparison.Ordinal);
    }

    private static bool IsEmpty(object value)
    {
        if (value == null) return true;
        if (value is string s) return string.IsNullOrWhiteSpace(s);
        if (value is System.Collections.ICollection collection) return collection.Count == 0;
        return false;
    }

    private static bool ToBool(object value)
    {
        if (value == null) return false;
        if (value is bool b) return b;
        if (value is string s)
        {
            if (bool.TryParse(s, out var parsed)) return parsed;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var number)) return number != 0;
            return !string.IsNullOrWhiteSpace(s) && !s.Equals("false", StringComparison.OrdinalIgnoreCase) && s != "0";
        }
        return true;
    }

    private static bool ToDecimal(object value, out decimal result)
    {
        result = 0;
        if (value == null) return false;
        switch (value)
        {
            case decimal d: result = d; return true;
            case int i: result = i; return true;
            case long l: result = l; return true;
            case double db: result = (decimal)db; return true;
            case float f: result = (decimal)f; return true;
            case short sh: result = sh; return true;
            case byte by: result = by; return true;
        }
        return decimal.TryParse(ToText(value), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static string ToText(object value)
    {
        if (value == null) return string.Empty;
        return value as string ?? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static bool IsWrappedByParentheses(string expression)
    {
        if (expression.Length < 2 || expression[0] != '(' || expression[expression.Length - 1] != ')') return false;

        var depth = 0;
        for (var i = 0; i < expression.Length; i++)
        {
            if (expression[i] == '(') depth++;
            else if (expression[i] == ')')
            {
                depth--;
                if (depth == 0 && i != expression.Length - 1) return false;
            }
        }
        return depth == 0;
    }

    /// <summary>
    ///     按顶层运算符拆分（忽略括号与引号内的运算符）
    /// </summary>
    private static List<string> SplitTopLevel(string expression, string separator)
    {
        var parts = new List<string>();
        var depth = 0;
        var inSingleQuote = false;
        var inDoubleQuote = false;
        var start = 0;

        for (var i = 0; i < expression.Length; i++)
        {
            var c = expression[i];
            if (c == '\'' && !inDoubleQuote) inSingleQuote = !inSingleQuote;
            else if (c == '"' && !inSingleQuote) inDoubleQuote = !inDoubleQuote;
            else if (!inSingleQuote && !inDoubleQuote)
            {
                if (c == '(') depth++;
                else if (c == ')') depth--;
            }

            if (depth != 0 || inSingleQuote || inDoubleQuote) continue;
            if (i + separator.Length > expression.Length) continue;
            if (string.Compare(expression, i, separator, 0, separator.Length, StringComparison.Ordinal) != 0) continue;

            parts.Add(expression.Substring(start, i - start));
            i += separator.Length - 1;
            start = i + 1;
        }

        parts.Add(expression.Substring(start));
        return parts;
    }

    /// <summary>
    ///     拆分二元运算符（按运算符最后一次出现，且左侧非空）
    /// </summary>
    private static bool TrySplitOperator(string expression, string op, out string left, out string right)
    {
        left = right = null;
        var index = expression.IndexOf(op, StringComparison.Ordinal);
        //  排除 != 被 ! 处理、>= 被 > 处理的情况：>= 用首次出现即可，因为 >= 一定先于单独的 > 匹配
        if (index <= 0) return false;
        if (op == ">" && index + 1 < expression.Length && expression[index + 1] == '=') return false;
        if (op == "<" && index + 1 < expression.Length && expression[index + 1] == '=') return false;

        left = expression.Substring(0, index).Trim();
        right = expression.Substring(index + op.Length).Trim();
        return left.Length > 0 && right.Length > 0;
    }

    private static int IndexOfWord(string expression, string word)
    {
        var index = expression.IndexOf(word, StringComparison.Ordinal);
        if (index <= 0) return -1;
        //  词边界检查，避免 "xnot contains" 误匹配
        if (index + word.Length < expression.Length && char.IsLetter(expression[index + word.Length])) return -1;
        return index;
    }
}
