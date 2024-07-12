
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class JsonFile
{
    static public void Write<T>(T t, string sourcePath)
    {
        string wholeString = ToString(t, Newtonsoft.Json.Formatting.Indented);
        JsonWrite.SaveFile(ref wholeString, sourcePath);
    }
    static public T Load<T>(string sourcePath)
    {
        return ToObject<T>(JsonWrite.LoadFile(sourcePath));
    }

    static public string ToString<T>(T t, Newtonsoft.Json.Formatting formatting)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(t, formatting);
    }
    static public T ToObject<T>(string jsonstring)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonstring);
    }
        
}

public static class JsonWrite
{
    private static string[] SupportExtensions = new string[] { ".sav", ".txt", ".html", ".htm", ".xml", ".bytes", ".json", ".csv", ".yaml", ".fnt" };
    private static bool isEncrypt = false;
    private static string KEY_1 = "ryojvlzmdalyglrj";
    private static string KEY_2 = "hcxilkqbbhczfeultgbskdmaunivmfuo";        
    public static string LoadFile(string sourcePath)
    {
        try
        {
            if (IsNullOrWhiteSpace(ref sourcePath))
            {
                Constants.LogMessage("[FileIO]", $"Error : Load File Path ({ sourcePath})");
                return "";
            }

            FileInfo theSourceFile = null;
            StreamReader sr = null;
            string newData = null;

            theSourceFile = new FileInfo(sourcePath);

            if (theSourceFile != null && theSourceFile.Exists)
            {
                sr = theSourceFile.OpenText();

                newData = DecryptString(sr.ReadToEnd());
                sr.Close();
            }
            else
            {
                Constants.LogMessage("[FileIO]", "LoadFile : sourcePath == NULL && referencePath == NULL");
            }

            if (IsNullOrWhiteSpace(ref newData))
            {
                Constants.LogMessage("[FileIO]", $"LoadFile : The File is Empty : {sourcePath}");
            }

            return newData;

        }
        catch (Exception e)
        {
            Constants.LogMessage("[FileIO]", $"LoadFile : Load File Error : {sourcePath}:  {e}");

            return "";
        }
    }

    public static void SaveFile(ref string wholeString, string targetPath)
    {
        try
        {
            if (IsNullOrWhiteSpace(ref targetPath))
            {
                Constants.LogMessage("[FileIO]", $"Error : Save File Path ({targetPath})");
                return;
            }

            AnalyzeFolder(targetPath);

            StreamWriter sw = new StreamWriter(targetPath);
            sw.Write(EncryptString(wholeString));
            sw.Flush();
            sw.Close();

        }
        catch (Exception e)
        {
            Constants.LogMessage("[FileIO]", $"SaveFile : Save File Error : {targetPath}: {e}");
        }
    }

    private static void AnalyzeFolder(string targetPath)
    {
        int slashPlace = targetPath.LastIndexOf("/");

        if (slashPlace > 0)
        {
            targetPath = targetPath.Remove(slashPlace);

            if (!Directory.Exists(targetPath))
            {
                Constants.LogMessage("[FileIO]", $"AnalyzeFolder : Create Directory: {targetPath}");

                Directory.CreateDirectory(targetPath);
            }
        }
    }

    private static bool IsNullOrWhiteSpace(ref string value)
    {
        if (value != null)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static string EncryptString(string ClearText)
    {
        if (!isEncrypt)
            return ClearText;

#if UNITY_WP8
return ClearText;		
#else
        try
        {
            byte[] clearTextBytes = Encoding.UTF8.GetBytes(ClearText);
            SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();
            MemoryStream ms = new MemoryStream();
            byte[] rgbIV = Encoding.ASCII.GetBytes(KEY_1);
            byte[] key = Encoding.ASCII.GetBytes(KEY_2);
            CryptoStream cs = new CryptoStream(ms, rijn.CreateEncryptor(key, rgbIV), CryptoStreamMode.Write);

            cs.Write(clearTextBytes, 0, clearTextBytes.Length);
            cs.Close();

            return Convert.ToBase64String(ms.ToArray());
        }
        catch
        {
            return ClearText;
        }
#endif
    }

    private static string DecryptString(string EncryptedText)
    {
        if (!isEncrypt)
            return EncryptedText;

#if UNITY_WP8
return EncryptedText;		
#else
        try
        {
            byte[] encryptedTextBytes = Convert.FromBase64String(EncryptedText);
            SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();
            MemoryStream ms = new MemoryStream();
            byte[] rgbIV = Encoding.ASCII.GetBytes(KEY_1);
            byte[] key = Encoding.ASCII.GetBytes(KEY_2);
            CryptoStream cs = new CryptoStream(ms, rijn.CreateDecryptor(key, rgbIV), CryptoStreamMode.Write);

            cs.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);
            cs.Close();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
        catch
        {
            return EncryptedText;
        }
#endif
    }
}