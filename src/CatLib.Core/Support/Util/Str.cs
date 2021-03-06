﻿/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: https://catlib.io/
 */

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CatLib
{
    /// <summary>
    /// 字符串
    /// </summary>
    public static class Str
    {
        /// <summary>
        /// 填充类型
        /// </summary>
        public enum PadTypes
        {
            /// <summary>
            /// 填充字符串的两侧。如果不是偶数，则右侧获得额外的填充。
            /// </summary>
            Both,

            /// <summary>
            /// 填充字符串的左侧。
            /// </summary>
            Left,

            /// <summary>
            /// 填充字符串的右侧。默认。
            /// </summary>
            Right
        }

        /// <summary>
        /// 空格字符串
        /// </summary>
        public const string Space = " ";

        /// <summary>
        /// 获取字符串所表达的函数名
        /// </summary>
        /// <param name="pattern">输入字符串</param>
        /// <returns>函数名</returns>
        public static string Method(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return string.Empty;
            }

            var chars = new char[pattern.Length];
            var count = 0;
            for (var i = pattern.Length - 1; i >= 0; i--)
            {
                var segment = pattern[i];
                if ((segment >= 'A' && segment <= 'Z')
                    || (segment >= 'a' && segment <= 'z')
                    || (segment >= '0' && segment <= '9')
                    || segment == '_')
                {
                    chars[count++] = segment;
                    continue;
                }

                if (count > 0)
                {
                    break;
                }
            }

            for (var i = count - 1; i >= 0; i--)
            {
                if ((chars[i] >= '0' && chars[i] <= '9'))
                {
                    count--;
                    continue;
                }
                break;
            }

            Array.Resize(ref chars, count);
            Array.Reverse(chars);

            return new string(chars);
        }

        /// <summary>
        /// 将规定字符串翻译为星号匹配表达式
        /// <para>即删减正则表达式中除了星号外的所有功能</para>
        /// </summary>
        /// <param name="pattern">匹配表达式</param>
        /// <param name="value">规定字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Is(string pattern, string value)
        {
            return pattern == value || Regex.IsMatch(value, "^" + AsteriskWildcard(pattern) + "$");
        }

        /// <summary>
        /// 通过星号匹配表达式来对规定数组进行匹配。
        /// <para>任意匹配式符合则返回true。</para>
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="patterns">匹配表达式</param>
        /// <param name="source">规定数组</param>
        /// <returns>是否通过检查</returns>
        public static bool Is<T>(string[] patterns, T source)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<ArgumentNullException>(patterns != null);

            foreach (var pattern in patterns)
            {
                if (Is(pattern, source.ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 将规定字符串翻译为星号匹配表达式
        /// <para>即删减正则表达式中除了星号外的所有功能</para>
        /// </summary>
        /// <param name="pattern">规定字符串</param>
        /// <returns>处理后的字符串</returns>
        public static string AsteriskWildcard(string pattern)
        {
            pattern = Regex.Escape(pattern);
            pattern = pattern.Replace(@"\*", ".*?");

            return pattern;
        }

        /// <summary>
        /// 根据长度将字符串分割到数组中
        /// </summary>
        /// <param name="str">要分割的字符串</param>
        /// <param name="length">规定每个数组元素的长度。默认是 1。</param>
        /// <returns>分割的字符串</returns>
        public static string[] Split(string str, int length = 1)
        {
            Guard.Requires<ArgumentNullException>(str != null);
            Guard.Requires<ArgumentOutOfRangeException>(length > 0);
            var requested = new string[str.Length / length + (str.Length % length == 0 ? 0 : 1)];

            for (var i = 0; i < str.Length; i += length)
            {
                requested[i / length] = str.Substring(i, Math.Min(str.Length - i, length));
            }

            return requested;
        }

        /// <summary>
        /// 将字符串重复指定的次数
        /// </summary>
        /// <param name="str">需要被重复的字符串</param>
        /// <param name="num">重复的次数</param>
        /// <returns>重复后的字符串</returns>
        public static string Repeat(string str, int num)
        {
            Guard.Requires<ArgumentNullException>(str != null);
            Guard.Requires<ArgumentOutOfRangeException>(num >= 0);

            if (num <= 0)
            {
                return string.Empty;
            }

            var requested = new StringBuilder();
            for (var i = 0; i < num; i++)
            {
                requested.Append(str);
            }
            return requested.ToString();
        }

        /// <summary>
        /// 随机打乱字符串中的所有字符
        /// </summary>
        /// <param name="str">需要被打乱的字符串</param>
        /// <param name="seed">种子</param>
        /// <returns>被打乱的字符串</returns>
        public static string Shuffle(string str, int? seed = null)
        {
            Guard.Requires<ArgumentNullException>(str != null);
            var random = Util.MakeRandom(seed);

            var requested = new string[str.Length];
            for (var i = 0; i < str.Length; i++)
            {
                var index = random.Next(0, str.Length - 1);

                requested[i] = requested[i] ?? str.Substring(i, 1);
                requested[index] = requested[index] ?? str.Substring(index, 1);

                if (index == i)
                {
                    continue;
                }

                var temp = requested[i];
                requested[i] = requested[index];
                requested[index] = temp;
            }

            return Arr.Reduce(requested, (v1, v2) => v1 + v2, string.Empty);
        }

        /// <summary>
        /// 计算子串在字符串中出现的次数
        /// <para>该函数不计数重叠的子串</para>
        /// </summary>
        /// <param name="str">规定字符串</param>
        /// <param name="subStr">子字符串</param>
        /// <param name="start">起始位置</param>
        /// <param name="length">需要扫描的长度</param>
        /// <param name="comparison">扫描规则</param>
        /// <returns>子字符串出现的次数</returns>
        public static int SubstringCount(string str, string subStr, int start = 0, int? length = null, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
        {
            Guard.Requires<ArgumentNullException>(str != null);
            Guard.Requires<ArgumentNullException>(subStr != null);

            Util.NormalizationPosition(str.Length, ref start, ref length);

            var count = 0;
            while (length.Value > 0)
            {
                int index;
                if ((index = str.IndexOf(subStr, start, length.Value, comparison)) < 0)
                {
                    break;
                }
                count++;
                length -= index + subStr.Length - start;
                start = index + subStr.Length;
            }

            return count;
        }

        /// <summary>
        /// 反转规定字符串
        /// </summary>
        /// <param name="str">规定字符串</param>
        /// <returns>反转后的字符串</returns>
        public static string Reverse(string str)
        {
            var chars = str.ToCharArray();
            Array.Reverse(chars);

            return new string(chars);
        }

        /// <summary>
        /// 把字符串填充为新的长度。
        /// </summary>
        /// <param name="str">规定要填充的字符串</param>
        /// <param name="length">规定新的字符串长度。如果该值小于字符串的原始长度，则不进行任何操作。</param>
        /// <param name="padStr">
        /// 规定供填充使用的字符串。默认是空白。
        /// <para>如果传入的字符串长度小于等于0那么会使用空白代替。</para>
        /// <para>注释：空白不是空字符串</para>
        /// </param>
        /// <param name="type">
        /// 规定填充字符串的哪边。
        /// <para><see cref="PadTypes.Both"/>填充字符串的两侧。如果不是偶数，则右侧获得额外的填充。</para>
        /// <para><see cref="PadTypes.Left"/>填充字符串的左侧。</para>
        /// <para><see cref="PadTypes.Right"/>填充字符串的右侧。默认。</para>
        /// </param>
        /// <returns>被填充的字符串</returns>
        [Obsolete("The overload method wile be remove in 2.0 version.")]
        public static string Pad(string str, int length, string padStr = null, PadTypes type = PadTypes.Right)
        {
            return Pad(length, str, padStr, type);
        }

        /// <summary>
        /// 把字符串填充为新的长度。
        /// </summary>
        /// <param name="length">规定新的字符串长度。如果该值小于字符串的原始长度，则不进行任何操作。</param>
        /// <param name="str">原始字符串,如果为null则为空值</param>
        /// <param name="padStr">
        /// 规定供填充使用的字符串。默认是空白。
        /// <para>如果传入的字符串长度小于等于0那么会使用空白代替。</para>
        /// <para>注释：空白不是空字符串</para>
        /// </param>
        /// <param name="type">
        /// 规定填充字符串的哪边。
        /// <para><see cref="PadTypes.Both"/>填充字符串的两侧。如果不是偶数，则右侧获得额外的填充。</para>
        /// <para><see cref="PadTypes.Left"/>填充字符串的左侧。</para>
        /// <para><see cref="PadTypes.Right"/>填充字符串的右侧。默认。</para>
        /// </param>
        /// <returns>被填充的字符串</returns>
        public static string Pad(int length, string str = null, string padStr = null, PadTypes type = PadTypes.Right)
        {
            str = str ?? string.Empty;

            var needPadding = length - str.Length;
            if (needPadding <= 0)
            {
                return str;
            }

            int rightPadding;
            var leftPadding = rightPadding = 0;

            if (type == PadTypes.Both)
            {
                leftPadding = needPadding >> 1;
                rightPadding = (needPadding >> 1) + (needPadding % 2 == 0 ? 0 : 1);
            }
            else if (type == PadTypes.Right)
            {
                rightPadding = needPadding;
            }
            else
            {
                leftPadding = needPadding;
            }

            padStr = padStr ?? Space;
            padStr = padStr.Length <= 0 ? Space : padStr;

            var leftPadCount = leftPadding / padStr.Length + (leftPadding % padStr.Length == 0 ? 0 : 1);
            var rightPadCount = rightPadding / padStr.Length + (rightPadding % padStr.Length == 0 ? 0 : 1);

            return Repeat(padStr, leftPadCount).Substring(0, leftPadding) + str +
                   Repeat(padStr, rightPadCount).Substring(0, rightPadding);
        }

        /// <summary>
        /// 在规定字符串中查找在规定搜索值，并在规定搜索值之后返回规定字符串的剩余部分。
        /// <para>如果没有找到则返回规定字符串本身</para>
        /// </summary>
        /// <param name="str">规定字符串</param>
        /// <param name="search">规定搜索值</param>
        /// <returns>剩余部分</returns>
        public static string After(string str, string search)
        {
            Guard.Requires<ArgumentNullException>(str != null);
            Guard.Requires<ArgumentNullException>(search != null);

            var index = str.IndexOf(search, StringComparison.Ordinal);
            return index < 0 ? str : str.Substring(index + search.Length, str.Length - index - search.Length);
        }

        /// <summary>
        /// 判断规定字符串是否包含规定子字符串
        /// <para>子字符串是识别大小写的</para>
        /// <para></para>
        /// </summary>
        /// <param name="str">规定字符串</param>
        /// <param name="needles">规定子字符串</param>
        /// <returns>是否包含</returns>
        public static bool Contains(string str, params string[] needles)
        {
            Guard.Requires<ArgumentNullException>(str != null);
            Guard.Requires<ArgumentNullException>(needles != null);

            foreach (var needle in needles)
            {
                if (str.Contains(needle))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 在规定字符串中替换匹配项
        /// </summary>
        /// <param name="matches">匹配项</param>
        /// <param name="replace">替换的值</param>
        /// <param name="str">规定字符串</param>
        /// <returns></returns>
        public static string Replace(string[] matches, string replace, string str)
        {
            Guard.Requires<ArgumentNullException>(matches != null);
            Guard.Requires<ArgumentNullException>(replace != null);
            Guard.Requires<ArgumentNullException>(str != null);

            foreach (var match in matches)
            {
                str = str.Replace(match, replace);
            }
            return str;
        }

        /// <summary>
        /// 替换规定字符串中第一次遇到的匹配项
        /// <para>该函数对大小写敏感</para>
        /// </summary>
        /// <param name="match">匹配项</param>
        /// <param name="replace">替换的内容</param>
        /// <param name="str">规定字符串</param>
        /// <returns>替换后的字符串</returns>
        public static string ReplaceFirst(string match, string replace, string str)
        {
            Guard.Requires<ArgumentNullException>(match != null);
            Guard.Requires<ArgumentNullException>(replace != null);
            Guard.Requires<ArgumentNullException>(str != null);

            var index = str.IndexOf(match, StringComparison.Ordinal);
            return index < 0 ? str : str.Remove(index, match.Length).Insert(index, replace);
        }

        /// <summary>
        /// 替换规定字符串中从后往前第一次遇到的匹配项
        /// <para>该函数对大小写敏感</para>
        /// </summary>
        /// <param name="match">匹配项</param>
        /// <param name="replace">替换的内容</param>
        /// <param name="str">规定字符串</param>
        /// <returns>替换后的字符串</returns>
        public static string ReplaceLast(string match, string replace, string str)
        {
            Guard.Requires<ArgumentNullException>(match != null);
            Guard.Requires<ArgumentNullException>(replace != null);
            Guard.Requires<ArgumentNullException>(str != null);

            var index = str.LastIndexOf(match, StringComparison.Ordinal);
            return index < 0 ? str : str.Remove(index, match.Length).Insert(index, replace);
        }

        /// <summary>
        /// 生成一个随机字母（含大小写），数字的字符串。
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <param name="seed">种子</param>
        /// <returns>随机的字符串</returns>
        public static string Random(int length = 16, int? seed = null)
        {
            Guard.Requires<ArgumentOutOfRangeException>(length > 0);

            var requested = string.Empty;
            var random = Util.MakeRandom(seed);
            for (int len; (len = requested.Length) < length;)
            {
                var size = length - len;
                var bytes = new byte[size];
                random.NextBytes(bytes);

                var code = Replace(new[] { "/", "+", "=" }, string.Empty, Convert.ToBase64String(bytes));
                requested += code.Substring(0, Math.Min(size, code.Length));
            }

            return requested;
        }

        /// <summary>
        /// 如果长度超过给定的最大字符串长度，则截断字符串。 截断的字符串的最后一个字符将替换为缺省字符串
        /// <para>eg: Str.Truncate("hello world , the sun is shine", 15, Str.Space) => hello world...</para>
        /// </summary>
        /// <param name="str">要截断的字符串</param>
        /// <param name="length">截断长度(含缺省字符长度)</param>
        /// <param name="separator">临近的分隔符，如果设定则截断长度为截断长度最近的分隔符位置,如果传入的是一个正则表达式那么使用正则匹配。</param>
        /// <param name="mission">缺省字符</param>
        /// <returns>截断后的字符串</returns>
        public static string Truncate(string str, int length, object separator = null, string mission = null)
        {
            if (str == null || length > str.Length)
            {
                return str;
            }

            mission = mission ?? "...";
            var end = length - mission.Length;

            if (end < 1)
            {
                return mission;
            }

            var result = str.Substring(0, end);

            if (separator == null)
            {
                return result + mission;
            }

            var separatorStr = separator.ToString();
            var index = -1;
            if (separator is Regex separatorRegex)
            {
                if (separatorRegex.IsMatch(result))
                {
                    index = (separatorRegex.RightToLeft
                        ? separatorRegex.Match(result)
                        : Regex.Match(result, separatorRegex.ToString(),
                            separatorRegex.Options | RegexOptions.RightToLeft)).Index;
                }
            }
            else if (!string.IsNullOrEmpty(separatorStr) && str.IndexOf(separatorStr, StringComparison.Ordinal) != end)
            {
                index = result.LastIndexOf(separatorStr, StringComparison.Ordinal);
            }

            if (index > -1)
            {
                result = result.Substring(0, index);
            }

            return result + mission;
        }

        /// <summary>
        /// 计算两个字符串之间的相似度。
        /// </summary>
        /// <param name="str1">字符串 1.</param>
        /// <param name="str2">字符串 2.</param>
        /// <returns>
        /// 通过Levenshtein算法返回两个字符串的相似度。如果两个字符串之间的
        /// 任意一个参数长度大于255那么。将会返回-1。
        /// </returns>
        public static int Levenshtein(string str1, string str2)
        {
            if (str1 == null || str2 == null)
            {
                return -1;
            }

            var length1 = str1.Length;
            var length2 = str2.Length;

            if (length1 > 255 || length2 > 255)
            {
                return -1;
            }

            var p1 = new int[length2 + 1];
            var p2 = new int[length2 + 1];

            for (var i = 0; i <= length2; i++)
            {
                p1[i] = i;
            }

            int Min(int num1, int num2, int num3)
            {
                var min = num1;
                if (min > num2)
                {
                    min = num2;
                }
                if (min > num3)
                {
                    min = num3;
                }
                return min;
            }

            for (var i = 0; i < length1; i++)
            {
                p2[0] = p1[0] + 1;
                for (var n = 0; n < length2; n++)
                {
                    var distance = str1[i] == str2[n]
                        ? Min(p1[n], p1[n + 1] + 1, p2[n] + 1)
                        : Min(p1[n] + 1, p1[n + 1] + 1, p2[n] + 1);
                    p2[n + 1] = distance;
                }

                var temp = p1;
                p1 = p2;
                p2 = temp;
            }

            return p1[length2];
        }

        /// <summary>
        /// Returns all sequential combination of the given array.
        /// </summary>
        /// <remarks>
        /// v[0] = "hello"
        /// v[1] = "world"
        /// var result = Str.JoinList(v, "/"); 
        /// result[0] == "hello";
        /// result[1] == "hello/world";
        /// </remarks>
        /// <param name="source">The source array.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The sequential combination array.</returns>
        public static string[] JoinList(string[] source, string separator = null)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            var builder = new StringBuilder();
            for (var index = 1; index < source.Length; index++)
            {
                builder.Append(source[index - 1]);
                if (!string.IsNullOrEmpty(separator))
                {
                    builder.Append(separator);
                }
                builder.Append(source[index]);
                source[index] = builder.ToString();
                builder.Remove(0, source[index].Length);
            }
            return source;
        }

        /// <inheritdoc cref="JoinList(string[], char)"/>
        public static string[] JoinList(string[] source, char separator)
        {
            return JoinList(source, separator.ToString());
        }
    }
}

