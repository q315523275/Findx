﻿using System.Security.Cryptography;
using System.Xml;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - RSA
/// </summary>
public partial class Extensions
{
    /// <summary>
    ///     RSA载入xml格式密钥
    /// </summary>
    /// <param name="rsa"></param>
    /// <param name="xmlString"></param>
    public static void FromXmlStringEx(this RSA rsa, string xmlString)
    {
        var parameters = new RSAParameters();

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlString);

        if (xmlDoc.DocumentElement != null && xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                switch (node.Name)
                {
                    case "Modulus":
                        parameters.Modulus = string.IsNullOrEmpty(node.InnerText)
                            ? null
                            : Convert.FromBase64String(node.InnerText);
                        break;
                    case "Exponent":
                        parameters.Exponent = string.IsNullOrEmpty(node.InnerText)
                            ? null
                            : Convert.FromBase64String(node.InnerText);
                        break;
                    case "P":
                        parameters.P = string.IsNullOrEmpty(node.InnerText)
                            ? null
                            : Convert.FromBase64String(node.InnerText);
                        break;
                    case "Q":
                        parameters.Q = string.IsNullOrEmpty(node.InnerText)
                            ? null
                            : Convert.FromBase64String(node.InnerText);
                        break;
                    case "DP":
                        parameters.DP = string.IsNullOrEmpty(node.InnerText)
                            ? null
                            : Convert.FromBase64String(node.InnerText);
                        break;
                    case "DQ":
                        parameters.DQ = string.IsNullOrEmpty(node.InnerText)
                            ? null
                            : Convert.FromBase64String(node.InnerText);
                        break;
                    case "InverseQ":
                        parameters.InverseQ = string.IsNullOrEmpty(node.InnerText)
                            ? null
                            : Convert.FromBase64String(node.InnerText);
                        break;
                    case "D":
                        parameters.D = string.IsNullOrEmpty(node.InnerText)
                            ? null
                            : Convert.FromBase64String(node.InnerText);
                        break;
                }
        else
            throw new Exception("Invalid XML RSA key.");

        rsa.ImportParameters(parameters);
    }

    /// <summary>
    ///     RSA导出xml格式密钥
    /// </summary>
    /// <param name="rsa"></param>
    /// <param name="includePrivateParameters"></param>
    /// <returns></returns>
    public static string ToXmlStringEx(this RSA rsa, bool includePrivateParameters)
    {
        var parameters = rsa.ExportParameters(includePrivateParameters);

        return string.Format(
            "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
            parameters.Modulus != null ? Convert.ToBase64String(parameters.Modulus) : null,
            parameters.Exponent != null ? Convert.ToBase64String(parameters.Exponent) : null,
            parameters.P != null ? Convert.ToBase64String(parameters.P) : null,
            parameters.Q != null ? Convert.ToBase64String(parameters.Q) : null,
            parameters.DP != null ? Convert.ToBase64String(parameters.DP) : null,
            parameters.DQ != null ? Convert.ToBase64String(parameters.DQ) : null,
            parameters.InverseQ != null ? Convert.ToBase64String(parameters.InverseQ) : null,
            parameters.D != null ? Convert.ToBase64String(parameters.D) : null);
    }
}