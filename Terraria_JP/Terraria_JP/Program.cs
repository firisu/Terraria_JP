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
            if (!TitleIsTerraria())
            {
                MessageBox.Show("既にTerraria.exeが加工済みです。" + Environment.NewLine + "終了します。",
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            // デフォルトのTerraria.exeがあれば、新しいアセンブリを作成
            else
            {
                var result = MessageBox.Show("Terraria.exeを日本語化します。" + Environment.NewLine + "数十秒前後かかります。",
                    "注意",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation);

                // 「OK」以外は全部キャンセル
                if (result != DialogResult.OK) Environment.Exit(0);

                // 処理中フォームの表示
                var thread = new Thread(new ThreadStart(Waiting));
                thread.IsBackground = true;
                thread.Start();

                // スプライトフォントのバックアップとコピー
                var files = Directory.GetFiles("Terraria_JP/Fonts", "*.xnb");
                var font_dir = "Content" + Path.DirectorySeparatorChar + "Fonts" + Path.DirectorySeparatorChar;
                Directory.CreateDirectory(font_dir + "old");
                foreach (var file in files)
	            {
                    var file_name = Path.GetFileName(file);
                    File.Copy(font_dir + file_name, font_dir + "old" + Path.DirectorySeparatorChar + file_name, true);
                    File.Copy(file, font_dir + file_name, true);
	            }

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

        static void MakeAssembly()
        {
            // 各アセンブリの読み込み
            var asm_base = AssemblyDefinition.ReadAssembly("Terraria_JP/BaseAssembly.exe");
            var asm_tera = AssemblyDefinition.ReadAssembly("Terraria.exe");

            /*
             * Terrariaのアセンブリに以下の加工を行う
             * 　(1) 全てのクラスをpublicにする
             * 　(2) 特定の関数をリネームしてpublicにする
             */
            foreach (var type in asm_tera.MainModule.GetTypes())
            {
                if (!type.IsNested) type.IsPublic = true;

                // Program.Main()をリネーム
                if (type.Name == "Program")
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Name == "Main")
                        {
                            method.IsPublic = true;
                            MethodDup(type, method);
                            break;
                        }
                    }
                }
                // Lang.dialog()をリネーム
                else if (type.Name == "Lang")
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Name == "dialog")
                        {
                            method.IsPublic = true;
                            MethodDup(type, method);
                            break;
                        }
                    }
                }
                // Steam.Init()をリネーム
                else if (type.Name == "Steam")
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Name == "Init")
                        {
                            method.IsPublic = true;
                            MethodDup(type, method);
                            break;
                        }
                    }
                }
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

            Process p = Process.Start(psi);
            //var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            //Console.WriteLine("Output: " + Environment.NewLine + output);

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
            var fs3 = new FileStream("Terraria_JP/asm_merge.exe", FileMode.Create);
            asm_merge.Write(fs3);
            fs3.Close();
        }

        static void MethodDup(TypeDefinition type, MethodDefinition method)
        {
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
            return;
        }
    }
#endif
}

