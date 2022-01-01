/*
  Rogue Legacy Enhanced

  This project is based on modified disassembly of Rogue Legacy's engine, with permission to do so by its creators.
  Therefore, former creators copyright notice applies to original disassembly. 

  Disassembled source Copyright(C) 2011-2015, Cellar Door Games Inc.
  Rogue Legacy(TM) is a trademark or registered trademark of Cellar Door Games Inc. All Rights Reserved.
*/

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace RogueCastle
{
    public class DialogueManager : IDisposable
    {
        private static Dictionary<string, Dictionary<string, DialogueObj>> m_languageArray;
        private static string m_currentLanguage;
        public static bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Console.WriteLine("Disposing Dialogue Manager");
                foreach (var current in m_languageArray)
                {
                    foreach (var current2 in current.Value) current2.Value.Dispose();
                    current.Value.Clear();
                }

                m_languageArray.Clear();
                m_languageArray = null;
                IsDisposed = true;
            }
        }

        public static void Initialize()
        {
            m_languageArray = new Dictionary<string, Dictionary<string, DialogueObj>>();
        }

        public static void LoadLanguageDocument(ContentManager content, string fileName)
        {
            LoadLanguageDocument(content.RootDirectory + "\\" + fileName + ".txt");
        }

        public static void LoadLanguageDocument(string fullFilePath)
        {
            var fileInfo = new FileInfo(fullFilePath);
            using (var streamReader = fileInfo.OpenText())
            {
                ParseDocument(streamReader);
            }
        }

        private static void ParseDocument(StreamReader reader)
        {
            var num = 0;
            var key = "";
            var item = "";
            string text = null;
            var list = new List<string>();
            var list2 = new List<string>();
            var flag = true;
            string text2;
            while ((text2 = reader.ReadLine()) != null)
            {
                if (text2 != "" && text2.IndexOf("//") != 0)
                {
                    var text3 = text2.Substring(text2.IndexOf(" ") + 1);
                    if (num == 0 && !text2.Contains("@language"))
                    {
                        throw new Exception("Cannot create text dictionary from file. Unspecified language type.");
                    }

                    if (text2.Contains("@language"))
                    {
                        SetLanguage(text3);
                    }
                    else if (text2.Contains("@label"))
                    {
                        if (!flag)
                        {
                            if (text != null)
                            {
                                list2.Add(text);
                                text = null;
                            }

                            AddText(key, list.ToArray(), list2.ToArray());
                            flag = true;
                        }

                        if (flag)
                        {
                            flag = false;
                            key = text3;
                            list.Clear();
                            list2.Clear();
                            text = null;
                            item = "";
                        }
                    }
                    else if (text2.Contains("@title"))
                    {
                        item = text3;
                    }
                    else if (text2.Contains("@text"))
                    {
                        list.Add(item);
                        if (text != null)
                        {
                            list2.Add(text);
                        }

                        text = text3;
                    }
                    else
                    {
                        text = text + "\n" + text2;
                    }
                }

                num++;
            }

            if (text != null)
            {
                list2.Add(text);
            }

            AddText(key, list.ToArray(), list2.ToArray());
        }

        public static void LoadLanguageBinFile(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    var num = 0;
                    var key = "";
                    var item = "";
                    string text = null;
                    var list = new List<string>();
                    var list2 = new List<string>();
                    var flag = true;
                    while (true)
                    {
                        var text2 = binaryReader.ReadString();
                        if (text2 != "" && text2.IndexOf("//") != 0)
                        {
                            var text3 = text2.Substring(text2.IndexOf(" ") + 1);
                            if (num == 0 && !text2.Contains("@language"))
                            {
                                break;
                            }

                            if (text2.Contains("@language"))
                            {
                                SetLanguage(text3);
                            }
                            else if (text2.Contains("@label"))
                            {
                                if (!flag)
                                {
                                    if (text != null)
                                    {
                                        list2.Add(text);
                                        text = null;
                                    }

                                    AddText(key, list.ToArray(), list2.ToArray());
                                    flag = true;
                                }

                                if (flag)
                                {
                                    flag = false;
                                    key = text3;
                                    list.Clear();
                                    list2.Clear();
                                    text = null;
                                    item = "";
                                }
                            }
                            else if (text2.Contains("@title"))
                            {
                                item = text3;
                            }
                            else if (text2.Contains("@text"))
                            {
                                list.Add(item);
                                if (text != null)
                                {
                                    list2.Add(text);
                                }

                                text = text3;
                            }
                            else if (text2 != "eof")
                            {
                                text = text + "\n" + text2;
                            }
                        }

                        num++;
                        if (text2 == "eof")
                        {
                            goto Block_18;
                        }
                    }

                    throw new Exception("Cannot create text dictionary from file. Unspecified language type.");
                    Block_18:
                    if (!flag)
                    {
                        if (text != null)
                        {
                            list2.Add(text);
                        }

                        AddText(key, list.ToArray(), list2.ToArray());
                    }
                }
            }
        }

        public static void SetLanguage(string language)
        {
            m_currentLanguage = language;
            if (!m_languageArray.ContainsKey(m_currentLanguage))
            {
                Console.WriteLine("Adding language dictionary for language: " + language);
                m_languageArray.Add(language, new Dictionary<string, DialogueObj>());
            }
        }

        public static void AddText(string key, string[] speakers, string[] text)
        {
            if (m_currentLanguage == null)
            {
                Console.WriteLine("Call SetLanguage() before attempting to add text to a specified language.");
                return;
            }

            if (m_languageArray[m_currentLanguage].ContainsKey(key))
            {
                Console.WriteLine("Cannot add text. Text with title already specified.");
                return;
            }

            var value = new DialogueObj(speakers, text);
            m_languageArray[m_currentLanguage].Add(key, value);
        }

        public static DialogueObj GetText(string key)
        {
            return m_languageArray[m_currentLanguage][key];
        }

        public static string GetCurrentLanguage()
        {
            return m_currentLanguage;
        }
    }
}