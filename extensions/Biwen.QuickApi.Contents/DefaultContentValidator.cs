// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-07 21:44:37 DefaultContentValidator.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Biwen.QuickApi.Contents;

/// <summary>
/// 文档验证器接口
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IContentValidator
{
    /// <summary>
    /// 异步验证内容
    /// </summary>
    /// <param name="content">内容对象</param>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateAsync<T>(T content) where T : class, IContent;
}


/// <summary>
/// 默认内容验证器
/// </summary>
/// <typeparam name="T">内容类型</typeparam>
public class DefaultContentValidator : IContentValidator
{
    /// <summary>
    /// 异步验证内容
    /// </summary>
    /// <param name="content">内容对象</param>
    /// <returns>验证结果</returns>
    public virtual Task<ValidationResult> ValidateAsync<T>(T content) where T : class, IContent
    {
        if (content == null)
        {
            return Task.FromResult(new ValidationResult("内容对象不能为空"));
        }

        // 获取类型中所有字段属性
        var properties = typeof(T).GetProperties()
            .Where(p => typeof(IFieldType).IsAssignableFrom(p.PropertyType));

        var validationResults = new List<ValidationResult>();

        foreach (var property in properties)
        {
            ValidateProperty(content, property, validationResults);
        }

        // 如果没有验证错误，返回 Success
        if (validationResults.Count == 0)
        {
            return Task.FromResult(ValidationResult.Success!);
        }

        // 返回第一个验证错误
        return Task.FromResult(validationResults[0]);
    }

    /// <summary>
    /// 验证单个属性
    /// </summary>
    /// <param name="content">内容对象</param>
    /// <param name="property">属性信息</param>
    /// <param name="validationResults">验证结果集合</param>
    protected virtual void ValidateProperty<T>(T content, PropertyInfo property, List<ValidationResult> validationResults)
        where T : class, IContent
    {
        // 获取字段值
        var fieldValue = property.GetValue(content) as IFieldType;

        // 获取字段名称
        var propertyName = property.Name;

        // 获取属性上的验证特性
        var attributes = property.GetCustomAttributes(true);

        // 用于保存成员名称的列表，用于 ValidationResult
        var memberNames = new[] { propertyName };

        // 首先检查字段是否为必填
        var requiredAttr = attributes.OfType<RequiredAttribute>().FirstOrDefault();
        if (requiredAttr != null)
        {
            // 如果是必填字段且字段值为null，则验证失败
            if (fieldValue == null)
            {
                var errorMessage = requiredAttr.ErrorMessage ?? $"字段'{propertyName}'不能为空";
                validationResults.Add(new ValidationResult(errorMessage, memberNames));
                return; // 如果必填验证失败，不继续进行其他验证
            }
        }

        // 如果字段值为null且不是必填字段，则跳过其他验证
        if (fieldValue == null) return;

        // 执行内部字段类型的验证
        var fieldTypeValid = fieldValue.Validate(fieldValue.Value);
        if (!fieldTypeValid)
        {
            var errorMessage = fieldValue.GetValidationErrorMessage() ?? $"字段'{propertyName}'验证失败";
            validationResults.Add(new ValidationResult(errorMessage, memberNames));
            return; // 如果基本类型验证失败，不继续进行其他验证
        }

        // --- 验证 DataAnnotations 特性 ---

        // 1. 验证 Required 特性 (现在只需要验证值是否为空，因为null已经在前面处理过)
        if (requiredAttr != null && string.IsNullOrWhiteSpace(fieldValue.Value))
        {
            var errorMessage = requiredAttr.ErrorMessage ?? $"字段'{propertyName}'不能为空";
            validationResults.Add(new ValidationResult(errorMessage, memberNames));
            return; // 如果必填验证失败，不继续进行其他验证
        }

        // 如果值为空且不是必填字段，后续验证可以跳过
        if (string.IsNullOrEmpty(fieldValue.Value))
        {
            return;
        }

        // 2. 验证 StringLength 特性
        var stringLengthAttr = attributes.OfType<StringLengthAttribute>().FirstOrDefault();
        if (stringLengthAttr != null)
        {
            int length = fieldValue.Value.Length;
            if (length < stringLengthAttr.MinimumLength || length > stringLengthAttr.MaximumLength)
            {
                var errorMessage = stringLengthAttr.ErrorMessage ??
                    $"字段'{propertyName}'长度必须在{stringLengthAttr.MinimumLength}到{stringLengthAttr.MaximumLength}之间";
                validationResults.Add(new ValidationResult(errorMessage, memberNames));
            }
        }

        // 3. 验证 MinLength 特性
        var minLengthAttr = attributes.OfType<MinLengthAttribute>().FirstOrDefault();
        if (minLengthAttr != null && fieldValue.Value.Length < minLengthAttr.Length)
        {
            var errorMessage = minLengthAttr.ErrorMessage ??
                $"字段'{propertyName}'长度必须大于或等于{minLengthAttr.Length}";
            validationResults.Add(new ValidationResult(errorMessage, memberNames));
        }

        // 4. 验证 MaxLength 特性
        var maxLengthAttr = attributes.OfType<MaxLengthAttribute>().FirstOrDefault();
        if (maxLengthAttr != null && fieldValue.Value.Length > maxLengthAttr.Length)
        {
            var errorMessage = maxLengthAttr.ErrorMessage ??
                $"字段'{propertyName}'长度必须小于或等于{maxLengthAttr.Length}";
            validationResults.Add(new ValidationResult(errorMessage, memberNames));
        }

        // 5. 验证 Range 特性
        var rangeAttr = attributes.OfType<RangeAttribute>().FirstOrDefault();
        if (rangeAttr != null)
        {
            try
            {
                var convertedValue = Convert.ChangeType(fieldValue.GetValue(), rangeAttr.OperandType);
                var min = Convert.ChangeType(rangeAttr.Minimum, rangeAttr.OperandType);
                var max = Convert.ChangeType(rangeAttr.Maximum, rangeAttr.OperandType);

                var comparer = Comparer<object>.Default;
                if (comparer.Compare(convertedValue, min) < 0 || comparer.Compare(convertedValue, max) > 0)
                {
                    var errorMessage = rangeAttr.ErrorMessage ??
                        $"字段'{propertyName}'的值必须在{rangeAttr.Minimum}到{rangeAttr.Maximum}之间";
                    validationResults.Add(new ValidationResult(errorMessage, memberNames));
                }
            }
            catch
            {
                validationResults.Add(new ValidationResult($"字段'{propertyName}'的值无法转换为有效的数值类型", memberNames));
            }
        }

        // 6. 验证 RegularExpression 特性
        var regexAttr = attributes.OfType<RegularExpressionAttribute>().FirstOrDefault();
        if (regexAttr != null)
        {
            if (!Regex.IsMatch(fieldValue.Value, regexAttr.Pattern))
            {
                var errorMessage = regexAttr.ErrorMessage ??
                    $"字段'{propertyName}'不符合规定的格式";
                validationResults.Add(new ValidationResult(errorMessage, memberNames));
            }
        }

        // 7. 验证 EmailAddress 特性
        var emailAttr = attributes.OfType<EmailAddressAttribute>().FirstOrDefault();
        if (emailAttr != null)
        {
            if (!emailAttr.IsValid(fieldValue.Value))
            {
                var errorMessage = emailAttr.ErrorMessage ??
                    $"字段'{propertyName}'必须是有效的电子邮件地址";
                validationResults.Add(new ValidationResult(errorMessage, memberNames));
            }
        }

        // 8. 验证 Phone 特性
        var phoneAttr = attributes.OfType<PhoneAttribute>().FirstOrDefault();
        if (phoneAttr != null)
        {
            if (!phoneAttr.IsValid(fieldValue.Value))
            {
                var errorMessage = phoneAttr.ErrorMessage ??
                    $"字段'{propertyName}'必须是有效的电话号码";
                validationResults.Add(new ValidationResult(errorMessage, memberNames));
            }
        }

        // 9. 验证 Url 特性
        var urlAttr = attributes.OfType<UrlAttribute>().FirstOrDefault();
        if (urlAttr != null)
        {
            if (!urlAttr.IsValid(fieldValue.Value))
            {
                var errorMessage = urlAttr.ErrorMessage ??
                    $"字段'{propertyName}'必须是有效的URL地址";
                validationResults.Add(new ValidationResult(errorMessage, memberNames));
            }
        }

        // 10. 验证 CreditCard 特性
        var creditCardAttr = attributes.OfType<CreditCardAttribute>().FirstOrDefault();
        if (creditCardAttr != null)
        {
            if (!creditCardAttr.IsValid(fieldValue.Value))
            {
                var errorMessage = creditCardAttr.ErrorMessage ??
                    $"字段'{propertyName}'必须是有效的信用卡号";
                validationResults.Add(new ValidationResult(errorMessage, memberNames));
            }
        }

        // 11. 验证 Compare 特性
        var compareAttr = attributes.OfType<CompareAttribute>().FirstOrDefault();
        if (compareAttr != null)
        {
            var otherProperty = typeof(T).GetProperty(compareAttr.OtherProperty);
            if (otherProperty != null)
            {
                var otherFieldValue = otherProperty.GetValue(content) as IFieldType;
                if (otherFieldValue != null && !string.Equals(fieldValue.Value, otherFieldValue.Value))
                {
                    var errorMessage = compareAttr.ErrorMessage ??
                        $"字段'{propertyName}'必须与'{compareAttr.OtherProperty}'相同";
                    validationResults.Add(new ValidationResult(errorMessage, new[] { propertyName, compareAttr.OtherProperty }));
                }
            }
        }

        // 12. 验证 CustomValidation 特性
        var customValidationAttrs = attributes.OfType<CustomValidationAttribute>();
        foreach (var customAttr in customValidationAttrs)
        {
            try
            {
                var validationContext = new ValidationContext(content)
                {
                    MemberName = propertyName
                };

                var customResult = customAttr.GetValidationResult(fieldValue.Value, validationContext);
                if (customResult != null && customResult != ValidationResult.Success)
                {
                    validationResults.Add(customResult); // 直接使用自定义验证返回的结果
                }
            }
            catch (Exception ex)
            {
                validationResults.Add(new ValidationResult($"字段'{propertyName}'自定义验证异常: {ex.Message}", memberNames));
            }
        }

        // 13. 验证 DataType 特性
        var dataTypeAttr = attributes.OfType<DataTypeAttribute>().FirstOrDefault();
        if (dataTypeAttr != null)
        {
            bool isValid = true;
            var errorMessage = dataTypeAttr.ErrorMessage;

            switch (dataTypeAttr.DataType)
            {
                case DataType.Date:
                    isValid = DateTime.TryParse(fieldValue.Value, out _);
                    errorMessage ??= $"字段'{propertyName}'必须是有效的日期格式";
                    break;
                case DataType.Time:
                    isValid = TimeSpan.TryParse(fieldValue.Value, out _);
                    errorMessage ??= $"字段'{propertyName}'必须是有效的时间格式";
                    break;
                case DataType.Currency:
                    isValid = decimal.TryParse(fieldValue.Value, NumberStyles.Currency, CultureInfo.CurrentCulture, out _);
                    errorMessage ??= $"字段'{propertyName}'必须是有效的货币格式";
                    break;
                case DataType.Password:
                    // 此处可以添加密码复杂度验证
                    break;
                case DataType.PostalCode:
                    // 邮政编码验证可以通过正则表达式处理
                    // 中国邮政编码是6位数字
                    isValid = Regex.IsMatch(fieldValue.Value, @"^\d{6}$");
                    errorMessage ??= $"字段'{propertyName}'必须是有效的邮政编码";
                    break;
            }

            if (!isValid)
            {
                validationResults.Add(new ValidationResult(errorMessage, memberNames));
            }
        }

        // 特殊处理数组字段类型
        var arrayAttr = attributes.OfType<ArrayFieldAttribute>().FirstOrDefault();
        if (arrayAttr != null && fieldValue is ArrayFieldType arrayField)
        {
            var arrayValues = arrayField.GetValue() as string[];
            if (arrayValues != null)
            {
                // 验证数组元素数量
                if (arrayValues.Length > arrayAttr.MaxCount)
                {
                    validationResults.Add(new ValidationResult(
                        $"字段'{propertyName}'最多只能包含{arrayAttr.MaxCount}个元素",
                        memberNames));
                }

                // 验证数组元素长度
                foreach (var item in arrayValues)
                {
                    if (item.Length > arrayAttr.MaxLength)
                    {
                        validationResults.Add(new ValidationResult(
                            $"字段'{propertyName}'中的元素长度不能超过{arrayAttr.MaxLength}",
                            memberNames));
                        break;
                    }
                }
            }
        }

        // 最后，执行自定义验证逻辑
        CustomValidate(content, property, fieldValue, validationResults);
    }

    /// <summary>
    /// 自定义验证逻辑，子类可以重写此方法实现特定的验证需求
    /// </summary>
    /// <param name="content">内容对象</param>
    /// <param name="property">属性信息</param>
    /// <param name="fieldValue">字段值</param>
    /// <param name="validationResults">验证结果集合</param>
    protected virtual void CustomValidate<T>(T content, PropertyInfo property, IFieldType fieldValue, List<ValidationResult> validationResults)
        where T : class, IContent
    {
        // 默认实现为空，子类可以重写此方法添加自定义验证逻辑
    }
}