using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Terraria
{
    public class Settings
    {
        // Steamチェックを行うかどうか
        // true ：Steam経由でなくても起動できる
        // false：Steam経由でないと起動できない
        public bool NoSteam { get; set; }

        // コンストラクタ
        public Settings()
        {
            NoSteam = false;
        }

        // 読み込み関数
        public static Settings ReadSetting(string filename)
        {
            // デフォルトのセッティング
            var setting = new Settings();

            // 設定ファイルを読み込む
            var siri = new XmlSerializer(typeof(Settings));
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.Open);
                setting = (Settings)siri.Deserialize(fs);
            }
            catch (FileNotFoundException e)
            {
                // ファイルが無かったら新しく作る
                fs = new FileStream(filename, FileMode.Create);
                siri.Serialize(fs, setting);
            }
            finally
            {
                fs.Close();
            }

            return setting;
        }
    }
}
