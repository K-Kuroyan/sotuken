﻿using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace sotsuken
{
    public partial class vpnformInstance : Form
    {
        public vpnformInstance()
        {
            InitializeComponent();
        }

        //変数宣言部
        List<List<string>> infolist = new List<List<string>>();
        string string_tmp;//都合よく使う存在

        private void vpn_Load(object sender, EventArgs e)
        {
            loadVPN();

            //configファイル読み込み
            string config = "";
            ArrayList configal = new ArrayList();
            /*try
            {
                using (StreamReader sr = new StreamReader
                    ("config.txt", Encoding.GetEncoding("Shift_JIS")))
                {
                    while ((config = sr.ReadLine()) != null)
                    {
                        configal.Add(config);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }*/
        }

        private void connectbutton_Click(object sender, EventArgs e)
        {
            int tmp = vpnlist.SelectedIndex;
            if (selectCheck() == true)
            {
                //接続状態がDisconnectedなら接続、Connectedなら注意ウィンドウを出す
                if (infolist[tmp][3] != "Connected")
                {
                    using (var authForm = new authForm(this))
                    {
                        authForm.ShowDialog();
                    }
                }
                else
                {
                    MessageBox.Show("このVPNはすでに接続されています");
                }
                if (infolist[tmp][3] != "")
                {
                    string_tmp = vpnlist.SelectedItem.ToString();
                    IconShow(1, string_tmp);
                }
                loadVPN();
            }
        }

        private void disconnectbutton_Click(object sender, EventArgs e)
        {
            if (selectCheck() == true)
            {
                int tmp = vpnlist.SelectedIndex;
                //接続状態がConnectedなら切断、Disconnectedなら注意ウィンドウを出す
                if (infolist[tmp][3] != "Disconnected ")
                {
                    disconnect(vpnlist.SelectedItem.ToString());
                }
                else
                {
                    MessageBox.Show("このVPNは接続されていません");
                }

                loadVPN();
                if (infolist[tmp][3] != "")
                {
                    string_tmp = vpnlist.SelectedItem.ToString();
                }
            }
        }

        private void editbutton_Click(object sender, EventArgs e)
        {
            if (selectCheck() == true)
            {
                //editfomtにVPN情報を渡す
                using (var subForm = new editForm(this))
                {
                    //editForm表示

                    subForm.ShowDialog();
                }

                loadVPN();
            }
        }

        private void createbutton_Click_1(object sender, EventArgs e)
        {
            //何も渡さずにediformを表示
            Create frm = new Create();
            frm.StartPosition = FormStartPosition.CenterScreen;
            frm.ShowDialog();
            loadVPN();
        }

        private void vpnlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*-------------------------------------------選択行のVPN情報を表示------------------------------------------*/
            /*
           [0] 　VPM名
           [1]　 サーバーアドレス
           [2] 　トンネルタイプ
           [3]　 接続状態
           [4]　 DNSサーバアドレス
            */
            infobox.Clear();
            try
            {
                for (int i = 0; ; i++)
                {
                    if ((string)vpnlist.SelectedItem == infolist[i][0].ToString())
                    {
                        infobox.Text = ("VPN名\t\t：" + infolist[i][0] + "\r\nVPNサーバIP\t：" + infolist[i][1] + "\r\nトンネルタイプ\t：" + infolist[i][2] + "\r\n接続状態\t\t：" + infolist[i][3] + "\r\nDNSサーバIP\t：" + infolist[i][4]);
                        break;
                    }
                }
            }
            catch
            {
            }

        }

        private void deletebutton_Click(object sender, EventArgs e)
        {
            if (selectCheck() == true)
            {
                string delSource = @"Remove-VpnConnection " + vpnlist.SelectedItem.ToString() + " -Force";

                RunPowerShell(delSource, 0);

                loadVPN();
            }

        }
        //ここからメソッド

        public void loadVPN()
        {
            //更新のためにvpnlistをクリア
            vpnlist.Items.Clear();

            //更新のためにinfolistをクリア
            infolist.Clear();

            string src = @"vpnlist.txt";

            if (File.Exists(src))
            {
            }
            else
            {
                File.Create("vpnlist.txt");
            }

            // ファイル属性を取得
            FileAttributes fa = File.GetAttributes(src);
            // 読み取り専用属性を削除（他の属性は変更しない）
            fa = fa & ~FileAttributes.ReadOnly;
            File.SetAttributes(src, fa);
            //VPNデータをもって来てテキストに書き込もうね～
            string refreshsource = @"Get-VpnConnection | Out-File vpnlist.txt -Encoding UTF8";
            RunPowerShell(refreshsource, 1);

            string line = "";
            int vpncnt = 0;
            //取得したデータを取得するリスト
            ArrayList vl = new ArrayList();

            //直下から起動時に作ったtxtデータを持ってくる
            using (StreamReader sr = new StreamReader(
                "vpnlist.txt", Encoding.GetEncoding("UTF-8")))
            {

                //取得したデータをリストに格納
                while ((line = sr.ReadLine()) != null)
                {
                    vl.Add(line);
                }
            }

            //リストに格納したデータを絞って出力
            for (int i = 0; i < vl.Count; i++)
            {
                if (vl[i].ToString().Contains("Name") == true)
                {
                    vpnlist.Items.Add(((string)vl[i]).Remove(0, 24));
                    vpncnt++;
                }
            }

            //読み取り専用属性を追加する
            File.SetAttributes(src, FileAttributes.ReadOnly);

            /*
            各VPNのデータをinfolistに格納
            [0] 　VPM名
            [1]　 サーバーアドレス
            [2] 　トンネルタイプ
            [3]　 接続状態
            [4]　 DNSサーバアドレス
             */
            if (vpncnt != 0)
            {
                string[] infobuffer = new string[5];
                for (int i = 0; i < vl.Count; i++)
                {
                    if (vl[i].ToString().Contains("Name") == true)
                    {
                        infobuffer[0] = ((string)vl[i]).Remove(0, 24);
                    }
                    else if (vl[i].ToString().Contains("ServerAddress") == true)
                    {
                        infobuffer[1] = ((string)vl[i]).Remove(0, 24);
                    }
                    else if (vl[i].ToString().Contains("TunnelType") == true)
                    {
                        infobuffer[2] = ((string)vl[i]).Remove(0, 24);
                    }
                    else if (vl[i].ToString().Contains("ConnectionStatus") == true)
                    {
                        infobuffer[3] = ((string)vl[i]).Remove(0, 24);
                    }
                    else if (vl[i].ToString().Contains("DnsSuffix") == true)
                    {
                        infobuffer[4] = ((string)vl[i]).Remove(0, 24);

                        infolist.Add(new List<string>(new string[] { infobuffer[0], infobuffer[1], infobuffer[2], infobuffer[3], infobuffer[4] }));
                    }
                }
            }

        }

        public static void RunPowerShell(string strCmd, int flg) //flgは成功ウィンドウが必要な場合だけ出力するフラグ
        {

            //PowerShellのエラーを取得するためのオブジェクト
            try
            {
                IList objErrors = null;

                using (Runspace objRunspace = RunspaceFactory.CreateRunspace())
                {

                    objRunspace.Open();
                    RunspaceInvoke objRunspaceInvoke = new RunspaceInvoke(objRunspace);

                    //PowerShellコマンドを実行
                    Collection<PSObject> objResultCollection = objRunspaceInvoke.Invoke(strCmd, null, out objErrors);

                }
                if (flg == 0)
                    MessageBox.Show("成功しました");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public bool selectCheck()
        {
            bool flg = false;
            //何も選択していなければ確認ポップをだす
            if (vpnlist.SelectedItem == null)
            {
                MessageBox.Show("VPNを選択してください");
            }
            else
            {
                flg = true;
            }

            return flg;
        }

        private void IconShow(int flg, string vpn_name)
        {
            if (flg == 1)
            {
                icon f1 = new icon();
                f1.GetVpnName(vpn_name);
                this.WindowState = FormWindowState.Minimized;
                f1.Show();



            }
        }

        public void disconnect(string vpn_name)
        {
            string disSource = @"RasDial " + vpn_name + " /disconnect";
            RunPowerShell(disSource, 1);
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            loadVPN();
        }

    }
}
