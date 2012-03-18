using System;
using System.Diagnostics;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;

namespace Terraria_JP
{
#if WINDOWS || XBOX
    static class Program
    {
        static Waiting form;

        static void Main(string[] args)
        {
            // デフォルトのTerraria.exeではなかった場合、警告して終了
            if (!TitleIsTerraria() && !ExistsBackup())
            {
                MessageBox.Show("既にTerraria.exeが加工済みです。" + Environment.NewLine + "終了します。",
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            // デフォルトのTerraria.exeがあれば、新しいアセンブリを作成
            else
            {
                string old = "";
                if (!TitleIsTerraria() && ExistsBackup())
                {
                    old = "（バックアップ使用）" + Environment.NewLine;
                    File.Copy("Terraria_old.exe", "Terraria.exe", true);
                }

                var result = MessageBox.Show("Terraria.exeを日本語化します。" + Environment.NewLine + old + "数十秒前後かかります。",
                    "注意",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation);

                // 「OK」以外は全部キャンセル
                if (result != DialogResult.OK) Environment.Exit(0);

                // 処理中フォームの表示
                var thread = new Thread(new ThreadStart(Waiting));
                thread.IsBackground = true;
                thread.Start();

                // アセンブリ作成
                MakeAssembly();

                // 処理中フォームをクローズ
                thread.Abort();

                // Terraria.exeをバックアップして、アセンブリで上書き
                File.Copy("Terraria.exe", "Terraria_old.exe", true);
                File.Copy("Terraria_JP/asm_merge.exe", "Terraria.exe", true);
            }

            MessageBox.Show("日本語化が完了しました。" + Environment.NewLine +
                "オリジナルのファイルをTerraria_old.exeにバックアップしました。",
                "完了",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        public static void Waiting()
        {
            form = new Waiting();
            form.Show();

            while (true)
            {
                Thread.Sleep(50);
                Application.DoEvents();
            }
        }

        // Terraria.exeがオリジナルか調べる
        static bool TitleIsTerraria()
        {
            var terraria = AssemblyDefinition.ReadAssembly("Terraria.exe");
            foreach (var attr in terraria.CustomAttributes)
            {
                // タイトル情報を取得
                if (attr.AttributeType.Name == "AssemblyTitleAttribute")
                {
                    var title = (string)attr.ConstructorArguments[0].Value;
                    return (title == "Terraria");
                }
            }
            return false;
        }

        // Terraria_old.exeが存在するか調べる
        static bool ExistsBackup()
        {
            return File.Exists("Terraria_old.exe");
        }

        static void MakeAssembly()
        {
            // 各アセンブリの読み込み
            var asm_base = AssemblyDefinition.ReadAssembly("Terraria_JP/BaseAssembly.exe");
            var asm_tera = AssemblyDefinition.ReadAssembly("Terraria.exe");

            /*
             * Terrariaのアセンブリに以下の加工を行う
             * 　(1) 全てのクラスをpublicにする
             * 　(2) 特定の関数をリネームしてpublicにする
             * 　(3) 特定の関数の末尾に操作用関数を追加する
             */
            foreach (var type in asm_tera.MainModule.GetTypes())
            {
                if (!type.IsNested) type.IsPublic = true;

                // Program.Main
                if (type.Name == "Program") RenameMethod(type, "Main");
                // Lang.dialog, Lang.npcName, Lang.setLang, Lang.itemName
                else if (type.Name == "Lang")
                {
                    RenameMethod(type, "dialog");
                    RenameMethod(type, "npcName");
                    RenameMethod(type, "setLang");
                    RenameMethod(type, "itemName");
                    RenameMethod(type, "toolTip");
                    RenameMethod(type, "toolTip2");
                    RenameMethod(type, "setBonus");
                }
                // Steam.Init
                else if (type.Name == "Steam") RenameMethod(type, "Init");
                // Item.AffixName
                else if (type.Name == "Item") RenameMethod(type, "AffixName");
            }

            // ベースアセンブリの全てのクラスをpublicにする
            foreach (var type in asm_base.MainModule.GetTypes())
            {
                if (!type.IsNested) type.IsPublic = true;
            }

            // 加工したアセンブリを一時的に出力する
            var fs1 = new FileStream("Terraria_JP/asm_base.exe", FileMode.Create);
            asm_base.Write(fs1);
            fs1.Close();
            asm_base = null;
            var fs2 = new FileStream("Terraria_JP/asm_tera.exe", FileMode.Create);
            asm_tera.Write(fs2);
            fs2.Close();
            asm_tera = null;

            // 一時アセンブリをマージする
            //ProcessStartInfo psi = new ProcessStartInfo("Terraria_JP/ILRepack.exe", "/union /ndebug /out:Terraria_JP/asm_merge.exe Terraria_JP/asm_base.exe Terraria_JP/asm_tera.exe");
            ProcessStartInfo psi = new ProcessStartInfo("Terraria_JP/ILRepack.exe", "/union /ndebug /parallel /out:Terraria_JP/asm_merge.exe Terraria_JP/asm_base.exe Terraria_JP/asm_tera.exe");

            psi.RedirectStandardOutput = false;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            if (true)
            {
                Process p = Process.Start(psi);
                p.WaitForExit();
            }

            // 一時アセンブリを削除する
            if (true)
            {
                File.Delete("Terraria_JP/asm_base.exe");
                File.Delete("Terraria_JP/asm_tera.exe");
            }

            // 出力したアセンブリのアーキテクチャを変更する
            var asm_merge = AssemblyDefinition.ReadAssembly("Terraria_JP/asm_merge.exe");
            foreach (var mod in asm_merge.Modules)
            {
                mod.Architecture = TargetArchitecture.I386;
                mod.Attributes |= ModuleAttributes.Required32Bit;
            }

            // 一部の特殊なクラスを書き換える
            var tail = asm_merge.MainModule.GetType("Terraria.Tail");

            foreach (var type in asm_merge.MainModule.GetTypes())
            {
                if (type.Name == "Main")
                {
                    TailMethod(type, tail, "LoadContent");
                    TailMethod(type, tail, "Initialize");
                }
                else if (type.Name == "Item")
                {
                    //TailMethod(type, tail, "SetDefaults");
                    RemakeMethod(type, tail, "SetDefaults");
                }
            }

            var fs3 = new FileStream("Terraria_JP/asm_merge.exe", FileMode.Create);
            asm_merge.Write(fs3);
            fs3.Close();
        }

        static void TailMethod(TypeDefinition type, TypeDefinition tail, string method_name)
        {
            foreach (var method1 in type.Methods)
            {
                if (method1.Name == method_name)
                {
                    // Item.SetDefaultsは特別扱い
                    if (method_name == "SetDefaults")
                    {
                        if (method1.Parameters.Count != 1) continue;
                    }
                    
                    foreach (var method2 in tail.Methods)
                    {
                        // 末尾命令の追加
                        if (method2.Name == "Tail" + method_name)
                        {
                            var instr1 = method1.Body.Instructions;
                            var last = instr1[instr1.Count - 1];
                            if (last.OpCode == OpCodes.Ret) instr1.Remove(last);

                            //foreach (var item in method2.Body.Instructions) instr1.Add(item);
                            var il1 = method1.Body.GetILProcessor();
                            foreach (var item in method2.Body.Instructions) il1.Append(item);
                            method1.Body.MaxStackSize += 5; // これをしないと、Item.SetDefaultsの.maxstacksizeが不足する
                           
                        }
                        // 末尾命令で呼び出されるメソッドの追加
                        else if (method2.Name == "_" + method_name)
                        {
                            var new_method = new MethodDefinition(method2.Name, method2.Attributes, method2.ReturnType);

                            // パラメータの追加
                            foreach (var item in method2.Parameters) new_method.Parameters.Add(item);

                            // ローカル変数の追加
                            foreach (var item in method2.Body.Variables) new_method.Body.Variables.Add(item);

                            // メソッド本体の追加
                            //foreach (var item in method2.Body.Instructions) new_method.Body.Instructions.Add(item);
                            var il_new = new_method.Body.GetILProcessor();
                            foreach (var item in method2.Body.Instructions) il_new.Append(item);

                            // 新メソッドを追加
                            type.Methods.Add(new_method);
                        }
                    }
                    return;
                }
            }
        }

        static void RenameMethod(TypeDefinition type, string method_name)
        {
            foreach (var method in type.Methods)
            {
                if (method.Name == method_name)
                {
                    method.IsPublic = true;

                    var new_method = new MethodDefinition("_" + method.Name, method.Attributes, method.ReturnType);

                    // パラメータのコピー
                    foreach (var par in method.Parameters)
                    {
                        new_method.Parameters.Add(new ParameterDefinition(par.ParameterType));
                    }

                    // ローカル変数のコピー
                    foreach (var variable in method.Body.Variables)
                    {
                        new_method.Body.Variables.Add(new VariableDefinition(variable.VariableType));
                    }

                    // メソッド本体のコピー
                    var il = new_method.Body.GetILProcessor();
                    foreach (var instr in method.Body.Instructions)
                    {
                        il.Append(instr);
                    }

                    // 新しいメソッドを追加する
                    type.Methods.Add(new_method);
                    
                    break;
                }
            }
        }

        static void RemakeMethod(TypeDefinition type, TypeDefinition tail, string method_name)
        {
            // 先に元のメソッドを保存しておく
            RenameMethod(type, "SetDefaults");

            foreach (var method1 in type.Methods)
            {
                if (method1.Name == method_name)
                {
                    // Item.SetDefaultsは特別扱い
                    if (method_name == "SetDefaults")
                    {
                        if (method1.Parameters.Count != 1) continue;
                    }

                    foreach (var method2 in tail.Methods)
                    {
                        // Tailクラスから元のクラスへメソッドをコピー
                        if (method2.Name == method_name)
                        {
                            method1.Body.Instructions.Clear();
                            method1.Body.Variables.Clear();

                            // ローカル変数の追加
                            foreach (var item in method2.Body.Variables) method1.Body.Variables.Add(item);

                            // メソッド本体の追加
                            var il1 = method1.Body.GetILProcessor();
                            foreach (var item in method2.Body.Instructions) il1.Append(item);
                        }
                    }
                    return;
                }
            }
        }
    }
#endif
}

