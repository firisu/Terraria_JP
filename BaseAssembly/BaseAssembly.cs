using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Xml;
using System.Reflection;
using System.Xml.Serialization;
using System.IO;

namespace Terraria
{
    public static class Program
    {
        public static Settings Setting;

        private static void Main(string[] args)
        {
            Setting = Settings.ReadSetting("Terraria_JP/Settings.xml");

            // XMLをリソースから呼び出す
            var asm = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream("Terraria.language.xml");

            try
            {
                var local = new FileStream("Terraria_JP/language.xml", FileMode.Open);
                if (local != null) stream = local;
            }
            catch
            {
            }

            // XMLファイルを読み込む
            Ja.xml = new XmlDocument();
            Ja.xml.Load(stream);

            // 元々のMain関数を呼び出す
            var type = typeof(Terraria.Program);
            var method = type.GetMethod("_Main");
            method.Invoke(type, new object[] { args });

            return;
        }
    }

    public static class Ja
    {
        const int MAX_NUM = 27;
        public static XmlDocument xml;
        

        public static string GetDialog(int l)
        {
            if (xml == null) return "";

            var xpath = "/language/lang/dialogs";
            var parent = xml.DocumentElement.SelectSingleNode(xpath);
            foreach (XmlElement node in parent.ChildNodes)
            {
                if (node["int"] != null)
                {
                    if (int.Parse(node["int"].InnerText) == l)
                    {
                        if (node["ja"] != null)
                        {
                            var temp = node["ja"].InnerText;

                            Console.WriteLine("未編集：" + Environment.NewLine + temp);

                            // 全角空白文字は改行コードに変換する
                            temp = temp.Replace('　', '\n');

                            // 各キャラクターの名前を変換する
                            var type_main = Type.GetType("Terraria.Main");
                            var fld_player = type_main.GetField("player");
                            var fld_myPlayer = type_main.GetField("myPlayer");
                            var fld_chrName = type_main.GetField("chrName");
                            var type_player = Type.GetType("Terraria.Player");
                            var fld_name = type_player.GetField("name");

                            var chrName = (string[])fld_chrName.GetValue(null);
                            var myPlayer = (int)fld_myPlayer.GetValue(null);
                            var player = (Object[])fld_player.GetValue(null);
                            var name = (string)fld_name.GetValue(player[myPlayer]);

                            Console.WriteLine(temp);
                            temp = temp.Replace("{Player}", name);
                            temp = temp.Replace("{Nurse}", chrName[18]);
                            temp = temp.Replace("{Mechanic}", chrName[124]);
                            temp = temp.Replace("{Demolitionist}", chrName[38]);
                            temp = temp.Replace("{Guide}", chrName[22]);
                            temp = temp.Replace("{Merchant}", chrName[17]);
                            temp = temp.Replace("{Arms Dealer}", chrName[19]);
                            temp = temp.Replace("{Dryad}", chrName[20]);
                            temp = temp.Replace("{Goblin}", chrName[107]);

                            // 改行コードで一行ごとに分ける
                            var strs = temp.Split(new char[] { '\n' });

                            // １ラインごとに処理していき、１行２５文字前後を超えたら自動で改行を挿入
                            for (int x = 0; x < strs.Length; x++)
                            {
                                if (strs[x].Length <= MAX_NUM) continue;

                                int i = 0;
                                int j = 0;
                                bool flag_hankaku = false;
                                while (true)
                                {
                                    if (i >= strs[x].Length) break;

                                    // 半角文字の間は改行しない
                                    flag_hankaku = (0x20 <= strs[x][i] && strs[x][i] <= 0x7D);

                                    // 半角文字でなく、最大文字数を超えていたら改行
                                    if (!flag_hankaku && (j/2) >= (MAX_NUM - 1))
                                    {
                                        strs[x] = strs[x].Insert(i, "\n");
                                        Console.WriteLine("１ライン超えた：" + strs[x]);
                                        j = 0;
                                        i += 2;
                                        continue;
                                    }

                                    i++;

                                    // 半角なら１文字分、全角なら２文字分としてカウント
                                    if (flag_hankaku) j++;
                                    else j += 2;
                                }
                            }

                            // 最後に改行コードで連結
                            var new_text = string.Join("\n", strs);

                            Console.WriteLine("出力テキスト：" + Environment.NewLine + new_text);
                            return new_text;
                        }
                        else return "";
                    }
                }
            }

            return "";
        }

        public static string GetNpcName(int l)
        {
            if (xml == null) return "";

            var xpath = "/language/lang/npcnames";
            var parent = xml.DocumentElement.SelectSingleNode(xpath);
            foreach (XmlElement node in parent.ChildNodes)
            {
                if (node["int"] != null)
                {
                    if (int.Parse(node["int"].InnerText) == l)
                    {
                        if (node["ja"] != null)
                        {
                            return node["ja"].InnerText;
                        }
                    }
                }
            }

            return "";
        }

        public static void setLang(Type type)
        {
            if (xml == null) return;
            var xpath = "/language/lang/";

            var strs = new string[] { "misc", "menu", "gen", "inter", "tip" };
            foreach (var str in strs)
            {
                var parent = xml.DocumentElement.SelectSingleNode(xpath + str + "s");
                foreach (XmlElement node in parent.ChildNodes)
                {
                    if (node["int"] != null)
                    {
                        uint i = 0;
                        if (uint.TryParse(node["int"].InnerText, out i))
                        {
                            if (node["ja"] != null)
                            {
                                type.InvokeMember(str, BindingFlags.SetField, null, null, new object[] { i, node["ja"].InnerText });
                            }
                        }
                    }
                }
            }
        }
    }

    public class Lang
    {
        public static string dialog(int l)
        {
            // オリジナルのテキストを取得
            var type = typeof(Terraria.Lang);
            var method = type.GetMethod("_dialog");
            var str_origin = (string)method.Invoke(null, new object[]{l});

            // XML上のテキストを取得
            var str_ja = Ja.GetDialog(l);

            // 空でない方のテキストを返す
            return (str_ja == "") ? str_origin : str_ja;
        }

        public static string npcName(int l)
        {
            // オリジナルのテキストを取得
            var type = typeof(Terraria.Lang);
            var method = type.GetMethod("_npcName");
            var str_origin = (string)method.Invoke(null, new object[] { l });

            // XML上のテキストを取得
            var str_ja = Ja.GetNpcName(l);

            // 空でない方のテキストを返す
            return (str_ja == "") ? str_origin : str_ja;
        }

        public static void setLang()
        {
            // オリジナルのテキストを取得
            var type = typeof(Terraria.Lang);
            var method = type.GetMethod("_setLang");
            var str_origin = (string)method.Invoke(null, null);

            // XML上のテキストを設定
            Ja.setLang(type);
        }
    }

    public class Steam
    {
        public static bool SteamInit;

        public static void Init()
        {
            if (Program.Setting.NoSteam)
            {
                Steam.SteamInit = true;
                Console.WriteLine("Steam初期化の関数");
            }
            else
            {
                var type = typeof(Terraria.Steam);
                var method = type.GetMethod("_Init");
                method.Invoke(null, null);
            }
        }
    }
}
