using System;
using System.Text;

public static class UtilsString
{
	public static string Capitalize(this string str)
	{
		if (string.IsNullOrEmpty(str))
			return str;
		if (str.Length == 1)
			return str.ToUpper();
		return char.ToUpper(str[0]) + str.Substring(1).ToLower();
	}

	//XOR Encryption for save purposes
	public static int key = 183;

	/// <summary>
	/// XOR Encryption, used for safety reason
	/// </summary>
	/// <param name="textToEncrypt">String to encrypt</param>
	/// <returns>Encrypted string</returns>
	public static string EncryptDecrypt(string textToEncrypt)
	{
		StringBuilder inSb = new StringBuilder(textToEncrypt);
		StringBuilder outSb = new StringBuilder(textToEncrypt.Length);
		char c;
		for (int i = 0; i < textToEncrypt.Length; i++)
		{
			c = inSb[i];
			c = (char)(c ^ key);
			outSb.Append(c);
		}
		return outSb.ToString();
	}

	public static string randomObfuscatingKey = "jhg a ,qsfdH1805Amélie86TEDHJ-(54=$*";

	public static string Encrypt(string textToEncrypt)
	{
		StringBuilder inSb = new StringBuilder(textToEncrypt+randomObfuscatingKey);
		StringBuilder outSb = new StringBuilder((textToEncrypt+randomObfuscatingKey).Length);
		char c;
		for (int i = 0; i < (textToEncrypt + randomObfuscatingKey).Length; i++)
		{
			c = inSb[i];
			c = (char)(c ^ key);
			outSb.Append(c);
		}
		return outSb.ToString();
	}

	public static string Decrypt(string textToEncrypt)
	{

		if (string.IsNullOrEmpty(textToEncrypt))
		{
			return "";
		}

		StringBuilder inSb = new StringBuilder(textToEncrypt);
		StringBuilder outSb = new StringBuilder(textToEncrypt.Length);
		char c;
		for (int i = 0; i < textToEncrypt.Length; i++)
		{
			c = inSb[i];
			c = (char)(c ^ key);
			outSb.Append(c);
		}
		return outSb.ToString().Substring(0, textToEncrypt.Length - randomObfuscatingKey.Length);
	}

}
