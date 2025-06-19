using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
  // 固定キーとIV（本番では安全な方法で管理してください）
  private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456"); // 16文字
  private static readonly byte[] IV = Encoding.UTF8.GetBytes("6543210987654321");  // 16文字

  /// 暗号化メソッド
  public static string Encrypt(string plainText) {
      using var aes = Aes.Create();
      aes.Key = Key;
      aes.IV = IV;

      using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
      using var ms = new MemoryStream();
      using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
      using (var sw = new StreamWriter(cs))
      {
          sw.Write(plainText);
      }
      return Convert.ToBase64String(ms.ToArray());
  }

  // 復号化メソッド
  public static string Decrypt(string cipherText) {
    using var aes = Aes.Create();
    aes.Key = Key;
    aes.IV = IV;

    using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
    using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
    using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
    using var sr = new StreamReader(cs);
    return sr.ReadToEnd();
  }
}