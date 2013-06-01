﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Messages;
using Messages.custom_msgs;
using Ros_CSharp;
using XmlRpc_Wrapper;
using Int32 = Messages.std_msgs.Int32;
using String = Messages.std_msgs.String;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;
using System.Threading;
using cm = Messages.uvc_camera.camera_sliders;
namespace CameraSlidersUC
{
    public class SliderStuff
    {
        int index;
        private Slider _slider;
        private Label _label;
        public int cameraNumber;
        public string topicIn, topicOut;
        private int slider_default = -1;
        private bool inited = false;
        private Action<int> fireMessage;
        public SliderStuff(NodeHandle nh, int i, string param, Slider s, Label l, Action<int> fireMessage)
        {
            _slider = s;
            _label = l;
            index = i;
            s.ValueChanged += sliderChanged;
            this.fireMessage = fireMessage;
        }
        void sliderChanged(object Sender, RoutedPropertyChangedEventArgs<double> dub)
        {
            Value = (int)Math.Round(dub.NewValue);
        }
        public void callback(int i, cm msg)
        {
            if (index == i)
            switch (index)
            {
                case 0:
                    {
                        if (!inited)
                        {
                            slider_default = msg.brightness;
                            return;
                        }
                        _slider.Dispatcher.BeginInvoke(new Action(() => { _slider.Value = msg.brightness; }));
                        _label.Dispatcher.BeginInvoke(new Action(() => { _label.Content = "" + msg.brightness; }));
                    } break;

                case 1:
                    {
                        if (!inited)
                        {
                            slider_default = msg.contrast;
                            return;
                        }
                        _slider.Dispatcher.BeginInvoke(new Action(() => { _slider.Value = msg.contrast; }));
                        _label.Dispatcher.BeginInvoke(new Action(() => { _label.Content = "" + msg.contrast; }));
                    } break;

                case 2:
                    {
                        if (!inited)
                        {
                            slider_default = msg.exposure;
                            return;
                        }
                        _slider.Dispatcher.BeginInvoke(new Action(() => { _slider.Value = msg.exposure; }));
                        _label.Dispatcher.BeginInvoke(new Action(() => { _label.Content = "" + msg.exposure; }));
                    }break;

                case 3:
                    {
                        if (!inited)
                        {
                            slider_default = msg.gain;
                            return;
                        }
                        _slider.Dispatcher.BeginInvoke(new Action(() => { _slider.Value = msg.gain; }));
                        _label.Dispatcher.BeginInvoke(new Action(() => { _label.Content = "" + msg.gain; }));
                    }break;
                case 4:
                    {
                        if (!inited)
                        {
                            slider_default = msg.saturation;
                            return;
                        }
                        _slider.Dispatcher.BeginInvoke(new Action(() => { _slider.Value = msg.saturation; }));
                        _label.Dispatcher.BeginInvoke(new Action(() => { _label.Content = "" + msg.saturation; }));
                    }break;
                case 5:
                    {
                        if (!inited)
                        {
                            slider_default = msg.wbt;
                            return;
                        }
                        _slider.Dispatcher.BeginInvoke(new Action(() => { _slider.Value = msg.wbt; }));
                        _label.Dispatcher.BeginInvoke(new Action(() => { _label.Content = "" + msg.wbt; }));
                    }break;
            }
                    
        }
        public int Value
        {
            get { return (int)Math.Round(_slider.Value); }
            set { if (fireMessage != null) fireMessage(cameraNumber); }
        }
    }

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CSUC : UserControl
    {
        public int exp, foc;
        NodeHandle node;
        SliderStuff[][] SUBS;
        Publisher<m.Int32>[] pub_exposureauto;
        //Publisher<m.Int32>[] pub_wbtauto;
        Publisher<m.Int32>[] pub_focusauto;
        private Publisher<cm>[] pub = new Publisher<cm>[4];
        private Subscriber<cm>[] sub = new Subscriber<cm>[4];
        private void cb0(cm msg) { cb(0, msg); }
        private void cb1(cm msg) { cb(1, msg); }
        private void cb2(cm msg) { cb(2, msg); }
        private void cb3(cm msg) { cb(3, msg); }
        private void cb(int c, cm msg)
        {
            for(int i=0; i<SUBS[c].Length;i++)
                SUBS[c][i].callback(i, msg);
        }
        public CSUC()
        {
            InitializeComponent();
        }

            //ROS
            private void UserControl_Loaded(object sender, RoutedEventArgs e)
            {
                new Thread(() =>
                {
                    while (!ROS.initialized)
                    {
                        Thread.Sleep(200);
                    }
                    node = new NodeHandle();

                    //setup up temporary arrays to make setup clean
                    Slider[][] sliders = new[]
                                         { new []{ MC1_Brigh_Sl, MC1_Cont_Sl, MC1_Exp_Sl, MC1_Gain_Sl, MC1_Sat_Sl, MC1_WBT_Sl, MC1_Foc_Sl}, 
                    new []{ RC_Brigh_Sl, RC_Cont_Sl, RC_Exp_Sl, RC_Gain_Sl, RC_Sat_Sl, RC_WBT_Sl, RC_Foc_Sl }, 
                    new []{ MC3_Brigh_Sl, MC3_Cont_Sl, MC3_Exp_Sl, MC3_Gain_Sl, MC3_Sat_Sl, MC3_WBT_Sl, MC3_Foc_Sl }, 
                    new []{ MC4_Brigh_Sl, MC4_Cont_Sl, MC4_Exp_Sl, MC4_Gain_Sl, MC4_Sat_Sl, MC4_WBT_Sl, MC4_Foc_Sl }};
                    Label[][] labels = new[]
                                       { new[]{ MC1_Brigh_Lvl, MC1_Cont_Lvl, MC1_Exp_Lvl, MC1_Gain_Lvl, MC1_Sat_Lvl, MC1_WBT_Lvl, MC1_Foc_Lvl }, 
                    new[]{ RC_Brigh_Lvl, RC_Cont_Lvl, RC_Exp_Lvl, RC_Gain_Lvl, RC_Sat_Lvl, RC_WBT_Lvl, RC_Foc_Lvl }, 
                    new[]{ MC3_Brigh_Lvl, MC3_Cont_Lvl, MC3_Exp_Lvl, MC3_Gain_Lvl, MC3_Sat_Lvl, MC3_WBT_Lvl, MC3_Foc_Lvl }, 
                    new[]{ MC4_Brigh_Lvl, MC4_Cont_Lvl, MC4_Exp_Lvl, MC4_Gain_Lvl, MC4_Sat_Lvl, MC4_WBT_Lvl, MC4_Foc_Lvl}};
                    string[] info = new[] { "camera0", "camera1", "camera2", "camera3" };
                    //end setup

                    //setup persistent array of slider stuff for storage
                    SUBS = new[] { new SliderStuff[6], new SliderStuff[6], new SliderStuff[6], new SliderStuff[6] };
                    
                    pub_exposureauto = new Publisher<m.Int32>[4];
                    //pub_wbtauto = new Publisher<m.Int32>[4];
                    pub_focusauto = new Publisher<m.Int32>[4];

                    ////// INITIALIZE THIS CAMS PUB AND SUB
                    pub = new Publisher<cm>[4];
                    sub = new Subscriber<cm>[]{node.subscribe<cm>("/camera0/sliders", 1, cb0),
                    node.subscribe<cm>("/camera1/sliders", 1, cb1),
                    node.subscribe<cm>("/camera2/sliders", 1, cb2),
                    node.subscribe<cm>("/camera3/sliders", 1, cb3)};

                    for (int i = 0; i < info.Length; i++)
                    {
                        //pub_exposureauto[i] = node.advertise<m.Int32>("camera" + i + "/exposureauto", 1);
                        //pub_wbtauto[i] = node.advertise<m.Int32>("camera" + i + "/wbtauto", 1);
                        //pub_focusauto[i] = node.advertise<m.Int32>("camera" + i + "/focusauto", 1);

                        pub[i] = node.advertise<cm>(info[i] + "/sliders", 1);

                        for (int j = 0; j < 6; j++)
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                SUBS[i][j] = new SliderStuff(node, j, info[i], sliders[i][j], labels[i][j], new Action<int>(fire));
                            }));
                        }
                    }
                    
                    while (!ROS.shutting_down)
                    {
                        ROS.spinOnce(node);
                        Thread.Sleep(100);
                    }
                }).Start();
            }

        private void fire(int cam)
        {
            cm msg = new cm{ brightness = SUBS[cam][0].Value, contrast = SUBS[cam][1].Value, exposure = SUBS[cam][2].Value, gain = SUBS[cam][3].Value, saturation = SUBS[cam][4].Value, wbt = SUBS[cam][5].Value, exposure_auto = exp, focus_auto = foc };
            pub[cam].publish(msg);
        }

            private void MC1_Exp_CB_Checked(object sender, RoutedEventArgs e)
            {
                MC1_Exp_Sl.IsEnabled = false;
                //3
                exp = 3;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void MC1_Exp_CB_Unchecked(object sender, RoutedEventArgs e)
            {
                MC1_Exp_Sl.IsEnabled = true;
                //1
                exp = 1;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }


            private void MC1_Foc_CB_Checked(object sender, RoutedEventArgs e)
            {
                MC1_Foc_Sl.IsEnabled = false;
                foc = 1;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void MC1_Foc_CB_Unchecked(object sender, RoutedEventArgs e)
            {
                MC1_Foc_Sl.IsEnabled = true;
                foc = 0;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }
            //END Main Camera 1 Sliders Changes

            //Rear Camera Slider Changes
            private void RC_Exp_CB_Checked(object sender, RoutedEventArgs e)
            {
                RC_Exp_Sl.IsEnabled = false;
                //3
                exp = 3;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void RC_Exp_CB_Unchecked(object sender, RoutedEventArgs e)
            {
                RC_Exp_Sl.IsEnabled = true;
                //1
                exp = 1;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void RC_Foc_CB_Checked(object sender, RoutedEventArgs e)
            {
                RC_Foc_Sl.IsEnabled = false;
                foc = 1;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void RC_Foc_CB_Unchecked(object sender, RoutedEventArgs e)
            {
                RC_Foc_Sl.IsEnabled = true;
                foc = 0;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }
            //END Rear Camera Slider Changes

            //Main Camera 3 Slider Changes
            private void MC3_Exp_CB_Checked(object sender, RoutedEventArgs e)
            {
                MC3_Exp_Sl.IsEnabled = false;
                //3
                exp = 3;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void MC3_Exp_CB_Unchecked(object sender, RoutedEventArgs e)
            {
                MC3_Exp_Sl.IsEnabled = true;
                //1
                exp = 1;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void MC3_Foc_CB_Checked(object sender, RoutedEventArgs e)
            {
                MC3_Foc_Sl.IsEnabled = false;
                foc = 1;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void MC3_Foc_CB_Unchecked(object sender, RoutedEventArgs e)
            {
                MC3_Foc_Sl.IsEnabled = true;
                foc = 0;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }
            //END Main Camera 3 Slider Changes

            //Main Camera 4 Slider Changes
            private void MC4_Exp_CB_Checked(object sender, RoutedEventArgs e)
            {
                MC4_Exp_Sl.IsEnabled = false;
                //3
                exp = 3;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void MC4_Exp_CB_Unchecked(object sender, RoutedEventArgs e)
            {
                MC4_Exp_Sl.IsEnabled = true;
                //1
                exp = 1;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void MC4_Foc_CB_Checked(object sender, RoutedEventArgs e)
            {
                MC4_Foc_Sl.IsEnabled = false;
                foc = 1;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }

            private void MC4_Foc_CB_Unchecked(object sender, RoutedEventArgs e)
            {
                MC4_Foc_Sl.IsEnabled = true;
                foc = 0;
                fire(MainCameraSliderTabControl.SelectedIndex);
            }
            //End Maine 4 Slider Changed
            private void MainTab_SelectionChanged(object sender, RoutedEventArgs e)
            {
            }
        }
    }
