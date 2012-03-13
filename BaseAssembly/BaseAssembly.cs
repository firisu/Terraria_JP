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
            var xml = new XmlDocument();
            xml.Load(stream);

            // XMLの内容を文字列に保存する
            var xpath = "/language/lang";
            var node_lang = xml.DocumentElement.SelectSingleNode(xpath);
            Ja.language = new Dictionary<string,Dictionary<int,string>>();

            // 各種ランゲージデータ（items, prefixsなど）を読み込んでいく
            foreach (XmlNode node1 in node_lang.ChildNodes)
            {
                var index = node1.LocalName;
                var dic = new Dictionary<int, string>();
                Ja.language.Add(index, dic);

                // 個別のランゲージデータ（item, prefixなど）を読み込む
                foreach (XmlNode node2 in node1.ChildNodes)
                {
                    // 個別の項目がカラで無ければ、辞書に追加
                    if (node2["int"] != null)
                    {
                        int i = 0;
                        if (int.TryParse(node2["int"].InnerText, out i))
                        {
                            if (node2["ja"] != null)
                            {
                                dic.Add(i, node2["ja"].InnerText);
                            }
                        }
                    }
                }
            }

            // XMLを解放
            xml = null;

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
        public static Dictionary<string, Dictionary<int, string>> language;

        public static string GetDialog(int l)
        {
            if (language == null) return "";

            // 辞書を取得
            Dictionary<int, string> dic;
            if (language.TryGetValue("dialogs", out dic))
            {
                // テキストを取得
                var temp = "";
                if (dic.TryGetValue(l, out temp))
                {
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
                        bool not_newline = false;
                        while (true)
                        {
                            if (i >= strs[x].Length) break;
                            char c = strs[x][i];

                            // 半角文字の間は改行しない
                            not_newline = (0x20 <= c && c <= 0x7D);

                            // 禁則文字の間は改行しない
                            not_newline = not_newline || c == '。' || c == '、' || c == '「' || c == '」';

                            // 半角文字でなく、最大文字数を超えていたら改行
                            if (!not_newline && (j / 2) >= (MAX_NUM - 1))
                            {
                                strs[x] = strs[x].Insert(i, "\n");
                                Console.WriteLine("１ライン超えた：" + strs[x]);
                                j = 0;
                                i += 2;
                                continue;
                            }

                            i++;

                            // 半角なら１文字分、全角なら２文字分としてカウント
                            if (not_newline) j++;
                            else j += 2;
                        }
                    }

                    // 最後に改行コードで連結
                    var new_text = string.Join("\n", strs);
                    return new_text;
                }
            }
            return "";
        }

        public static string GetNpcName(int l)
        {
            if (language == null) return "";

            // 辞書を取得
            Dictionary<int, string> dic;
            if (language.TryGetValue("npcnames", out dic))
            {
                // テキストを取得
                var name = "";
                if (dic.TryGetValue(l, out name))
                {
                    return name;
                }
            }
            return "";
        }

        public static string GetItemName(int l)
        {
            if (language == null) return "";

            // 辞書を取得
            Dictionary<int, string> dic;
            if (language.TryGetValue("items", out dic))
            {
                // テキストを取得
                var name = "";
                if (dic.TryGetValue(l, out name))
                {
                    return name;
                }
            }
            return "";
        }

        public static string GetPrefix(int l)
        {
            if (language == null) return "";

            // 辞書を取得
            Dictionary<int, string> dic;
            if (language.TryGetValue("prefixs", out dic))
            {
                // テキストを取得
                var name = "";
                if (dic.TryGetValue(l, out name))
                {
                    return name;
                }
            }
            return "";
        }

        // Terraria.Langのstaticメンバーに値をセットする
        public static void setLang(Type type)
        {
            if (language == null) return;

            var strs = new string[] { "misc", "menu", "gen", "inter", "tip"};
            foreach (var str in strs)
            {
                // 存在する辞書のみ取得
                Dictionary<int, string> dic;
                if (language.TryGetValue(str+"s", out dic))
                {
                    foreach (var pair in dic)
                    {
                        if (pair.Key < 0)
                        {
                            Console.WriteLine("キー値がマイナスです。キー：{0}　バリュー：{1}", pair.Key, pair.Value);
                            continue;
                        }
                        type.InvokeMember(str, BindingFlags.SetField, null, null, new object[] { pair.Key, pair.Value });
                    }
                }
                else
                {
                    Console.WriteLine("辞書データが見つかりません：" + str);
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

        public static string itemName(int l)
        {
            // オリジナルのテキストを取得
            var type = typeof(Terraria.Lang);
            var method = type.GetMethod("_itemName");
            var str_origin = (string)method.Invoke(null, new object[] { l });

            // XML上のテキストを取得
            var str_ja = Ja.GetItemName(l);

            // 空でない方のテキストを返す
            return (str_ja == "") ? str_origin : str_ja;
        }
    }

    public class Item
    {
        public string AffixName()
        {
            // オリジナルのテキストを取得
            var type = typeof(Terraria.Item);
            var method = type.GetMethod("_AffixName");
            var str_origin = (string)method.Invoke(this, null);

            // プレフィクスとアイテムのフィールドを取得
            var f_prefix = type.GetField("prefix");
            var f_name = type.GetField("name");

            // フィールドから値を取得
            var prefix = Ja.GetPrefix((byte)f_prefix.GetValue(this));
            var name = (string)f_name.GetValue(this);

            // 名前が空でなければ、日本語を返す
            if (name != "")
            {
                if (prefix != "") return name + "（" + prefix + "）";
                else return name;
            }

            // 名前が空ならば、元の名前を返す
            return str_origin;
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
