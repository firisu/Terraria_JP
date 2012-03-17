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
            Type.GetType("Terraria.Main").GetMethod("_LoadContent").Invoke(this, null);
        }

        public void _LoadContent()
        {
            // フィールドを取得してくる
            var type = this.GetType();
            
            var fontItemStack = type.GetField("fontItemStack");
            var fontMouseText = type.GetField("fontMouseText");
            var fontDeathText = type.GetField("fontDeathText");

            var type_base = base.GetType();
            var Content = (ContentManager)type_base.GetProperty("Content").GetValue(this, null);

            var test = (SpriteFont)fontMouseText.GetValue(this);

            // 日本語用のスプライトフォントを読み込ませる
            var font_dir = ".." + Path.DirectorySeparatorChar + "Terraria_JP" + Path.DirectorySeparatorChar + "Fonts" + Path.DirectorySeparatorChar;
            fontItemStack.SetValue(this, (SpriteFont)Content.Load<SpriteFont>(font_dir + "Item_Stack"));
            fontMouseText.SetValue(this, (SpriteFont)Content.Load<SpriteFont>(font_dir + "Mouse_Text"));
            fontDeathText.SetValue(this, (SpriteFont)Content.Load<SpriteFont>(font_dir + "Death_Text"));
            var fontCombatText = (SpriteFont)Content.Load<SpriteFont>(font_dir + "Combat_Text");
            var fontCombatCrit = (SpriteFont)Content.Load<SpriteFont>(font_dir + "Combat_Crit");

            test = (SpriteFont)fontMouseText.GetValue(this);

            // fontCombatTextは配列なのでセットの仕方が特殊
            type.InvokeMember("fontCombatText", BindingFlags.SetField, null, this, new object[] { 0, fontCombatText });
            type.InvokeMember("fontCombatText", BindingFlags.SetField, null, this, new object[] { 1, fontCombatCrit });
        }

        public void TailInitialize()
        {
            Type.GetType("Terraria.Main").GetMethod("_Initialize").Invoke(this, null);
        }

        public void _Initialize()
        {
            // これを追加しないとアイテム名の処理が上手くいかない
            var type = Type.GetType("Terraria.Main");
            for (int i = 0; i < 604; i++)
            {
                type.InvokeMember("itemName", BindingFlags.SetField, null, this, new object[] { i, Terraria.Ja.GetItemName_en(i) });
            }

            // レシピ作成を再度行う
            type = Type.GetType("Terraria.Recipe");
            type.InvokeMember("numRecipes", BindingFlags.SetField, null, null, new object[] { 0 });
            var mi = type.GetMethod("SetupRecipes").Invoke(null, null);
        }
    }
}
