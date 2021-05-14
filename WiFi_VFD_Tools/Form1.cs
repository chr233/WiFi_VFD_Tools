using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WiFi_VFD_Tools
{
    public partial class Form1 : Form
    {
        protected WebClient_EX wbc = new WebClient_EX { Encoding = Encoding.UTF8, Timeout = 5000 };

        private readonly RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\WiFi_VFD");

        public Form1()
        {
            InitializeComponent();

            String device_ip = registry.GetValue("device_ip", "192.168.2.6").ToString();
            txt_ip.Text = device_ip;

            String conn_password = registry.GetValue("conn_password", "CHR233").ToString();
            txt_conn_passwd.Text = conn_password;

            cb_mode.SelectedIndex = 0;
            cb_text_mode.SelectedIndex = 0;
        }

        public String get_api_path(String path = null, bool need_pass = true, Dictionary<String, String> args = null)
        {
            String full_url = "http://";
            full_url += txt_ip.Text;
            if (path != null)
            {
                if (!path.StartsWith("/"))
                {
                    full_url += "/";
                }
                full_url += path;
            }

            if (args == null)
            {
                args = new Dictionary<string, string>();
            }

            if (need_pass || args.Count > 0)
            {
                full_url += "?";
                if (need_pass)
                {
                    args.Add("k", txt_conn_passwd.Text);
                }
                foreach (var item in args)
                {
                    full_url += String.Format("{0}={1}&", item.Key, item.Value);
                }
            }
            return full_url;
        }

        public Boolean exec_cmd(String path = null, bool need_pass = true, Dictionary<String, String> args = null)
        {
            ts_msg.Text = "执行中";
            try
            {
                String url = get_api_path(path, need_pass, args);
                String resp = wbc.DownloadString(url);
                if (resp == "success")
                {
                    ts_msg.Text = "操作成功";
                    return true;
                }
                else
                {
                    if (resp.IndexOf("password wrong") != -1)
                    {
                        ts_msg.Text = "密码错误，请设置连接密码";
                    }
                    else if (resp.IndexOf("error") != -1)
                    {
                        ts_msg.Text = "参数错误";
                    }
                    else if (resp.IndexOf("resource not found") != -1)
                    {
                        ts_msg.Text = "API错误";
                    }
                    else
                    {
                        ts_msg.Text = "未知返回值";
                    }
                    SystemSounds.Beep.Play();
                    return false;
                }
            }
            catch (Exception ex)
            {
                ts_msg.Text = "连接失败 " + ex.Message;
                SystemSounds.Beep.Play();
                return false;
            }
        }




        private void ts_lbl3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://blog.chrxw.com/archives/2021/05/07/1569.html");
        }

        private void ts_lbl4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://blog.chrxw.com");
        }


        private void btn_device_test_Click(object sender, EventArgs e)
        {
            if (exec_cmd("/test", false, null))
            {
                ts_msg.Text = "连接成功";
                registry.SetValue("device_ip", txt_ip.Text);
            }
        }

        private void btn_reboot_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要重启吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                exec_cmd("/set/reboot", true, null);
            }
        }
        private void ts_lbl2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://blog.chrxw.com");
        }

        private void btn_set_bright_Click(object sender, EventArgs e)
        {
            var args = new Dictionary<String, String>();
            if (chk_bright_flash.Checked)
            {
                args.Add("f", "1");
            }
            args.Add("b", tb_bright.Value.ToString());
            exec_cmd("/set/bright", true, args);
        }

        private void btn_set_mode_Click(object sender, EventArgs e)
        {
            var args = new Dictionary<String, String>();
            if (chk_mode_flash.Checked)
            {
                args.Add("f", "1");
            }
            args.Add("m", (cb_mode.SelectedIndex + 1).ToString());
            exec_cmd("/set/mode", true, args);
        }


        private void btn_change_passwd_Click(object sender, EventArgs e)
        {
            var args = new Dictionary<String, String>();
            args.Add("kn", txt_new_conn_passwd.Text);
            if (exec_cmd("/set/password", true, args))
            {
                txt_conn_passwd.Text = txt_new_conn_passwd.Text;
                txt_new_conn_passwd.Text = "";
                registry.SetValue("conn_password", txt_conn_passwd.Text);
            }

        }

        private void btn_version_Click(object sender, EventArgs e)
        {
            try
            {
                String url = get_api_path("/version", false, null);
                String version = wbc.DownloadString(url);
                ts_msg.Text = "操作成功";
                MessageBox.Show(version, "固件信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                ts_msg.Text = "读取失败";
            }
        }

        private void btn_set_wifi_Click(object sender, EventArgs e)
        {
            if (txt_wifi_ssid.Text != "" && txt_wifi_passwd.Text != "")
            {
                var args = new Dictionary<String, String>();
                args.Add("s", txt_wifi_ssid.Text);
                args.Add("p", txt_wifi_passwd.Text);
                exec_cmd("/set/wifi", true, args);
            }
        }

        private void btn_set_time_Click(object sender, EventArgs e)
        {
            var args = new Dictionary<String, String>();
            if (chk_time_flash.Checked)
            {
                args.Add("f", "1");
            }
            args.Add("h", chk_24h.Checked ? "1" : "0");
            args.Add("a", chk_apm.Checked ? "1" : "0");
            args.Add("w", chk_week.Checked ? "1" : "0");
            exec_cmd("/set/time/config", true, args);
        }

        private void btn_set_text_mode_Click(object sender, EventArgs e)
        {
            var args = new Dictionary<String, String>();
            if (chk_text_mode_flash.Checked)
            {
                args.Add("f", "1");
            }
            args.Add("m", (cb_text_mode.SelectedIndex).ToString());
            exec_cmd("/set/text/config", true, args);
        }

        private void btn_set_text_string_Click(object sender, EventArgs e)
        {
            var args = new Dictionary<String, String>();
            if (chk_text_string_flash.Checked)
            {
                args.Add("f", "1");
            }
            args.Add("w", txt_text_string.Text);
            exec_cmd("/set/text/group", true, args);
        }

        private void btn_set_text_logo_Click(object sender, EventArgs e)
        {
            var args = new Dictionary<String, String>();
            if (chk_text_logo_flash.Checked)
            {
                args.Add("f", "1");
            }
            int tmp = 0;

            if (chk_logo1.Checked)
                tmp += 1;
            if (chk_logo2.Checked)
                tmp += (1 << 1);
            if (chk_logo3.Checked)
                tmp += (1 << 2);
            if (chk_logo4.Checked)
                tmp += (1 << 3);
            if (chk_logo5.Checked)
                tmp += (1 << 4);
            if (chk_logo6.Checked)
                tmp += (1 << 5);
            if (chk_logo7.Checked)
                tmp += (1 << 6);
            if (chk_logo8.Checked)
                tmp += (1 << 7);
            if (chk_logo9.Checked)
                tmp += (1 << 8);
            if (chk_logo10.Checked)
                tmp += (1 << 9);
            if (chk_logo11.Checked)
                tmp += (1 << 10);
            if (chk_logo12.Checked)
                tmp += (1 << 11);

            args.Add("i", tmp.ToString());
            exec_cmd("/set/text/icon", true, args);
        }

        private void btn_reset_setting_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要恢复出厂设置吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                exec_cmd("/set/reset", true, null);
            }
        }

        private void tb_proc_value_Scroll(object sender, EventArgs e)
        {
            nm_proc_value.Value = tb_proc_value.Value;
        }

        private void nm_proc_value_ValueChanged(object sender, EventArgs e)
        {
            tb_proc_value.Value = (int)nm_proc_value.Value;
        }

        private void btn_set_proc_text_Click(object sender, EventArgs e)
        {
            var args = new Dictionary<String, String>();
            if (chk_proc_text_flash.Checked)
            {
                args.Add("f", "1");
            }
            args.Add("w", txt_proc_string.Text);
            exec_cmd("/set/proc/string", true, args);
        }

        private void btn_set_proc_value_Click(object sender, EventArgs e)
        {
            var args = new Dictionary<String, String>();
            if (chk_proc_value_flash.Checked)
            {
                args.Add("f", "1");
            }
            args.Add("v", nm_proc_value.Value.ToString());
            exec_cmd("/set/proc/value", true, args);
        }

        private void btn_proc_setting_Click(object sender, EventArgs e)
        {
            var args = new Dictionary<String, String>();
            if (chk_proc_setting_flash.Checked)
            {
                args.Add("f", "1");
            }
            args.Add("i", chk_logobar.Checked ? "1" : "0");
            args.Add("n", chk_percent.Checked ? "1" : "0");
            args.Add("l", chk_label.Checked ? "1" : "0");
            exec_cmd("/set/proc/config", true, args);
        }

        private void txt_proc_string_TextChanged(object sender, EventArgs e)
        {
            txt_text_string.Text = txt_proc_string.Text;
        }

        private void txt_text_string_TextChanged(object sender, EventArgs e)
        {
            txt_proc_string.Text = txt_text_string.Text;
        }

        private void btn_logo_all_on_Click(object sender, EventArgs e)
        {
            chk_logo1.Checked = true;
            chk_logo2.Checked = true;
            chk_logo3.Checked = true;
            chk_logo4.Checked = true;
            chk_logo5.Checked = true;
            chk_logo6.Checked = true;
            chk_logo7.Checked = true;
            chk_logo8.Checked = true;
            chk_logo9.Checked = true;
            chk_logo10.Checked = true;
            chk_logo11.Checked = true;
            chk_logo12.Checked = true;
            btn_set_text_logo_Click(sender, e);
        }

        private void btn_logo_all_off_Click(object sender, EventArgs e)
        {
            chk_logo1.Checked = false;
            chk_logo2.Checked = false;
            chk_logo3.Checked = false;
            chk_logo4.Checked = false;
            chk_logo5.Checked = false;
            chk_logo6.Checked = false;
            chk_logo7.Checked = false;
            chk_logo8.Checked = false;
            chk_logo9.Checked = false;
            chk_logo10.Checked = false;
            chk_logo11.Checked = false;
            chk_logo12.Checked = false;
            btn_set_text_logo_Click(sender, e);
        }
    }
}
