using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class MyUtils
{
    public static string GetAppFolderLocalPath(this string s)
    {
        return Environment.GetEnvironmentVariable("LOCALAPPDATA");
    }

    public static bool IsFileExists(this string s)
    {
        return File.Exists(s);
    }

    public static bool IsDirectoryExists(this string s)
    {
        return Directory.Exists(s);
    }


    public enum WriteToFileInOneShotConfig
    {
        Utf8NoBomWindowsLineEnding,
        Utf8NoBomUnixLineEnding,
        Utf8BomWindowsLineEnding,
        Utf8BomUnixLineEnding,
    }

    public static string WriteToFileInOneShot(this string content, string path, WriteToFileInOneShotConfig config = WriteToFileInOneShotConfig.Utf8NoBomWindowsLineEnding)
    {
        if (config == WriteToFileInOneShotConfig.Utf8NoBomWindowsLineEnding)
        {
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }
        else if (config == WriteToFileInOneShotConfig.Utf8NoBomUnixLineEnding)
        {
            File.WriteAllText(path, content.ToUnixLineEnding(), new UTF8Encoding(false));
        }
        else if (config == WriteToFileInOneShotConfig.Utf8BomWindowsLineEnding)
        {
            File.WriteAllText(path, content, new UTF8Encoding(true));
        }
        else if (config == WriteToFileInOneShotConfig.Utf8BomUnixLineEnding)
        {
            File.WriteAllText(path, content.ToUnixLineEnding(), new UTF8Encoding(true));
        }

        return content;
    }

    public static string ToWindowsLineEnding(this string s)
    {
        return Regex.Replace(s, @"(?<!\r)\n", "\r\n");
    }

    public static string ToUnixLineEnding(this string s)
    {
        return Regex.Replace(s, @"\r\n", "\n");
    }


    public static bool IsNullOrEmpty(this string me)
    {
        return string.IsNullOrEmpty(me);
    }

    public static bool IsNotNullOrEmpty(this string me)
    {
        return !string.IsNullOrEmpty(me);
    }


    public static string[] Split(this string me, string splitter, bool remove_empty_elements = false)
    {
        return me.Split(new[] {splitter}, remove_empty_elements ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
    }


    public static void ClearFolder(this string folder_name, bool delete_itself = false)
    {
        var dir = new DirectoryInfo(folder_name);
        dir.ClearFolder(delete_itself);
    }

    public static void ClearFolder(this DirectoryInfo folder_name, bool delete_itself = false)
    {
        if (!folder_name.Exists) return;

        foreach (var fi in folder_name.EnumerateFiles())
            try
            {
                fi.Delete();
            }
            catch (Exception)
            {
            }


        foreach (var di in folder_name.EnumerateDirectories())
        {
            di.FullName.ClearFolder();
            try
            {
                di.Delete();
            }
            catch (Exception)
            {
            }
        }

        if (delete_itself)
            try
            {
                folder_name.Delete();
            }
            catch (Exception)
            {
            }
    }


    public static string ReplaceRegex(this string s, string regex, string replacement)
    {
        if (s.IsNotNullOrEmpty() && regex.IsNotNullOrEmpty()) return Regex.Replace(s, regex, replacement);

        return s;
    }

    public static string GoUpNLevels(this string path, int level = 1, string separator_regex_plz_always_use_character_class_form = @"[\\/]")
    {
        if (path == null || path.Length <= 1 || level <= 0) return path;

        var sep = separator_regex_plz_always_use_character_class_form;
        var old_path = "";
        while (old_path != path && level >= 1)
        {
            old_path = path;
            var regex_str = @"(?i)" + sep + "(?:" + sep.ToggleCharacterClass() + ")+(?:" + sep + ")?$";
            path = Regex.Replace(path, regex_str, "");
            level--;
            if (Regex.IsMatch(path, "(?i)^[a-z]:$")) path = path + "\\";
        }

        return path;
    }

    public static string ToggleCharacterClass(this string regex_character_class_string_form)
    {
        if (regex_character_class_string_form.IsNullOrEmpty() || regex_character_class_string_form.Length < 3 || regex_character_class_string_form[0] != '[' || regex_character_class_string_form[regex_character_class_string_form.Length - 1] != ']') throw new ArgumentException("Not valid character class form.");

        if (regex_character_class_string_form[1] == '^')
            return regex_character_class_string_form.ReplaceRegex(@"^\[\^?", "[");
        return regex_character_class_string_form.ReplaceRegex(@"^\[?", "[^");
    }
}