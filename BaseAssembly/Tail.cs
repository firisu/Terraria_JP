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
    // Tail が付いているメソッドは末尾追加用のメソッド。
    // _ が付いているメソッドを呼び出すためだけに存在する。
    class Tail
    {
        public void TailLoadContent()
        {
            Console.WriteLine("TailLoadContent");

            Type.GetType("Terraria.Main").GetMethod("_LoadContent").Invoke(this, null);
        }

        public void _LoadContent()
        {
            Console.WriteLine("_LoadContent");

            // フィールドを取得してくる
            var type = this.GetType();
            
            var fontItemStack = type.GetField("fontItemStack");
            var fontMouseText = type.GetField("fontMouseText");
            var fontDeathText = type.GetField("fontDeathText");

            Console.WriteLine("スプライトフォント取得済み");

            var type_base = base.GetType();
            var Content = (ContentManager)type_base.GetProperty("Content").GetValue(this, null);

            Console.WriteLine("コンテントマネージャー全部取得済み");

            var test = (SpriteFont)fontMouseText.GetValue(this);
            Console.WriteLine("マウスフォントの文字数：" + test.Characters.Count);

            // 日本語用のスプライトフォントを読み込ませる
            var font_dir = ".." + Path.DirectorySeparatorChar + "Terraria_JP" + Path.DirectorySeparatorChar + "Fonts" + Path.DirectorySeparatorChar;
            fontItemStack.SetValue(this, (SpriteFont)Content.Load<SpriteFont>(font_dir + "Item_Stack"));
            fontMouseText.SetValue(this, (SpriteFont)Content.Load<SpriteFont>(font_dir + "Mouse_Text"));
            fontDeathText.SetValue(this, (SpriteFont)Content.Load<SpriteFont>(font_dir + "Death_Text"));
            var fontCombatText = (SpriteFont)Content.Load<SpriteFont>(font_dir + "Combat_Text");
            var fontCombatCrit = (SpriteFont)Content.Load<SpriteFont>(font_dir + "Combat_Crit");

            test = (SpriteFont)fontMouseText.GetValue(this);
            Console.WriteLine("マウスフォント（日本語）の文字数：" + test.Characters.Count);

            // fontCombatTextは配列なのでセットの仕方が特殊
            type.InvokeMember("fontCombatText", BindingFlags.SetField, null, this, new object[] { 0, fontCombatText });
            type.InvokeMember("fontCombatText", BindingFlags.SetField, null, this, new object[] { 1, fontCombatCrit });

            Console.WriteLine("日本語スプライト読み込み完了"); 
        }
    }
}
