﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Enthernet;
using HslCommunication;
using System.Net;

namespace HslCommunicationDemo
{
    public partial class FormComplexNet : Form
    {
        public FormComplexNet( )
        {
            InitializeComponent( );
        }

        private void FormComplexNet_Load( object sender, EventArgs e )
        {
            button2.Enabled = false;
            textBox3.Text = Guid.Empty.ToString( );
        }


        private NetComplexClient complexClient = null;


        private void button1_Click( object sender, EventArgs e )
        {

            if (!IPAddress.TryParse( textBox1.Text, out IPAddress address ))
            {
                MessageBox.Show( "IP地址填写不正确" );
                return;
            }

            if (!int.TryParse( textBox2.Text, out int port ))
            {
                MessageBox.Show( "port填写不正确" );
                return;
            }

            try
            {
                // 连接 connect
                complexClient = new NetComplexClient( );
                complexClient.ClientAlias = textBox9.Text;
                complexClient.EndPointServer = new IPEndPoint( address, port );
                complexClient.Token = new Guid( textBox3.Text );
                complexClient.AcceptString += ComplexClient_AcceptString;
                complexClient.AcceptByte += ComplexClient_AcceptByte;
                complexClient.ClientStart( );

                button1.Enabled = false;
                button2.Enabled = true;
                panel2.Enabled = true;
            }
            catch (Exception ex)
            {
                HslCommunication.BasicFramework.SoftBasic.ShowExceptionMessage( ex );
            }
        }

        private void ComplexClient_AcceptByte( HslCommunication.Core.Net.AppSession session, NetHandle handle, byte[] data )
        {
            // 接收字节数据，
            ShowTextInfo( $"[{session.IpEndPoint}] [{handle}] {HslCommunication.BasicFramework.SoftBasic.ByteToHexString( data )}" );
        }

        private void ComplexClient_AcceptString( HslCommunication.Core.Net.AppSession session, NetHandle handle, string data )
        {
            // 接收字符串
            ShowTextInfo( $"[{session.IpEndPoint}] [{handle}] {data}" );
        }

        private void button2_Click( object sender, EventArgs e )
        {
            // 断开连接 disconnect
            complexClient.ClientClose( );
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void ShowTextInfo( string text )
        {
            if (InvokeRequired)
            {
                Invoke( new Action<string>( ShowTextInfo ), text );
                return;
            }

            textBox8.AppendText( text + Environment.NewLine );
        }

        private void button3_Click( object sender, EventArgs e )
        {
            // 数据发送
            NetHandle handle = new NetHandle( );
            if (textBox5.Text.IndexOf( '.' ) >= 0)
            {
                string[] values = textBox5.Text.Split( '.' );
                handle = new NetHandle( byte.Parse( values[0] ), byte.Parse( values[1] ), ushort.Parse( values[2] ) );
            }
            else
            {
                handle = int.Parse( textBox5.Text );
            }


            if (!int.TryParse( textBox6.Text, out int count ))
            {
                MessageBox.Show( "数据发送次数输入异常" );
                return;
            }

            for (int i = 0; i < count; i++)
            {
                complexClient.Send( handle, textBox4.Text );
            }
        }

        private void button6_Click( object sender, EventArgs e )
        {
            // 多客户端压力测试
            System.Threading.Thread thread = new System.Threading.Thread( ServerPressureTest );
            thread.IsBackground = true;
            thread.Start( );
            button6.Enabled = false;
        }

        private void ServerPressureTest( )
        {
            NetComplexClient[] netComplices = new NetComplexClient[1000];
            for (int i = 0; i < 1000; i++)
            {
                netComplices[i] = new NetComplexClient( );
                netComplices[i].EndPointServer = new IPEndPoint( IPAddress.Parse( textBox1.Text ), int.Parse( textBox2.Text ) );
                netComplices[i].ClientAlias = "Client" + (i + 1);
                netComplices[i].ClientStart( );
            }

            for (int j = 0; j < 1000; j++)
            {
                for (int i = 0; i < 1000; i++)
                {
                    netComplices[i].Send( 1, "测试消息" + (i + 1) );
                }
                System.Threading.Thread.Sleep( 1000 );
            }


            System.Threading.Thread.Sleep( 2000 );
            for (int i = 0; i < 1000; i++)
            {
                netComplices[i].ClientClose( );
            }


            Invoke( new Action( ( ) =>
             {
                 button6.Enabled = true;
             } ) );
        }

    }
}
